using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using ADOFAI.Editor.ParticleEditor;
using HarmonyLib;
using JALib.Core.Patch;
using JALib.Tools;
using UnityEngine;

namespace SmartEditor.FixLoad.CustomSaveState;

public class SaveStatePatch {
    public static FieldInfo saveStateLastFrame = SimpleReflect.Field(typeof(scnEditor), "saveStateLastFrame");
    public static MethodInfo unsavedChanges = typeof(scnEditor).Setter("unsavedChanges");
    public static List<ChangedEventCache> changedEvents = [];
    public static List<ChangedFloorCache> changedFloors = [];
    public static Dictionary<EventKey, EventValue> changedEventValues = new();
    public static List<LevelState> undoStates = new(100);
    public static List<LevelState> redoStates = [];
    public static DefaultLevelState currentState;

    public static void Patch(JAPatcher patcher) {
        patcher.AddPatch(typeof(SaveStatePatch));
        foreach(MethodInfo method in typeof(scnEditor).Methods())
            if(method.Name.StartsWith("<NewLevel>")) patcher.AddPatch(NewLevel, new JAPatchAttribute(method, PatchType.Transpiler, false));
        foreach(Type type in typeof(scnEditor).GetNestedTypes(AccessTools.all))
            if(type.Name.StartsWith("<OpenLevelCo>")) patcher.AddPatch(NewLevel, new JAPatchAttribute(type.Method("MoveNext"), PatchType.Transpiler, false));
    }

    [JAPatch(typeof(scnEditor), nameof(SaveState), PatchType.Replace, false)]
    public static void SaveState(scnEditor __instance, bool clearRedo = false, bool dataHasChanged = true) {
        scnEditor editor = __instance;
        if(editor.changingState != 0 || !editor.initialized) return;
        int[] selectedFloors = null;
        if(!editor.SelectionIsEmpty())
            if(editor.SelectionIsSingle()) selectedFloors = [editor.selectedFloors[0].seqID];
            else {
                selectedFloors = new int[editor.selectedFloors.Count];
                for(int i = 0; i < selectedFloors.Length; i++) selectedFloors[i] = editor.selectedFloors[i].seqID;
            }
        int[] currentDecorationItemIndices = new int[editor.selectedDecorations.Count];
        int index = 0;
        foreach(LevelEvent selectedDecoration in editor.selectedDecorations) {
            currentDecorationItemIndices[index] = scrDecorationManager.GetDecorationIndex(selectedDecoration);
            ++index;
        }
        InspectorPanel panel = editor.levelEventsPanel;
        currentState = new DefaultLevelState {
            selectedFloors = selectedFloors,
            selectedDecorationIndices = currentDecorationItemIndices,
            settingsEventType = editor.settingsPanel.selectedEventType,
            floorEventType = panel.selectedEventType,
            floorEventTypeIndex = panel.EventNumOfTab(panel.selectedEventType)
        };
        if(undoStates.Count >= 100) undoStates.RemoveAt(0);
        undoStates.Add(currentState);
        if(clearRedo) redoStates.Clear();
        saveStateLastFrame.SetValue(editor, Time.frameCount);
        if(dataHasChanged) unsavedChanges.Invoke(editor, [true]);
        changedEvents.Clear();
        changedFloors.Clear();
        changedEventValues.Clear();
    }

    [JAPatch(typeof(SaveStateScope), "Dispose", PatchType.Postfix, false)]
    public static void SaveStateScopeDispose() {
        if(currentState == null) return;
        currentState.changedEvents = changedEvents.ToArray();
        changedEvents.Clear();
        currentState.changedFloors = changedFloors.ToArray();
        changedFloors.Clear();
        currentState.changedEventValues = changedEventValues;
        changedEventValues = new Dictionary<EventKey, EventValue>();
        currentState = null;
    }

    [JAPatch(typeof(EventsArray<LevelEvent>), nameof(Add), PatchType.Postfix, false, ArgumentTypesType = [typeof(LevelEvent)])]
    [JAPatch(typeof(DecorationsArray<LevelEvent>), nameof(Add), PatchType.Postfix, false, ArgumentTypesType = [typeof(LevelEvent)])]
    [JAPatch(typeof(DecorationsArray<LevelEvent>), nameof(Insert), PatchType.Postfix, false, ArgumentTypesType = [typeof(int), typeof(LevelEvent)])]
    public static void Add(LevelEvent item) {
        if(currentState != null) changedEvents.Add(new ChangedEventCache(ChangedEventCache.Action.Add, item));
    }

    [JAPatch(typeof(List<LevelEvent>), nameof(Remove), PatchType.Postfix, false, ArgumentTypesType = [typeof(LevelEvent)])]
    public static void Remove(object item) {
        if(currentState != null && item is LevelEvent @event) changedEvents.Add(new ChangedEventCache(ChangedEventCache.Action.Remove, @event));
    }

    [JAPatch(typeof(List<LevelEvent>), nameof(RemoveAll), PatchType.Prefix, false, ArgumentTypesType = [typeof(Predicate<LevelEvent>)])]
    public static void RemoveAll(object __instance, Predicate<LevelEvent> match) {
        if(currentState == null || __instance is not List<LevelEvent> list) return;
        foreach(LevelEvent @event in list) if(match(@event)) changedEvents.Add(new ChangedEventCache(ChangedEventCache.Action.Remove, @event));
    }

    [JAPatch(typeof(LevelEvent), "[].set", PatchType.Prefix, false)]
    public static void Set(LevelEvent __instance, string key, object value) {
        if(currentState == null) return;
        EventKey eventKey = new(__instance, key);
        if(!changedEventValues.TryGetValue(eventKey, out EventValue eventValue)) changedEventValues[eventKey] = eventValue = new EventValue(__instance[key]);
        eventValue.newValue = value;
    }

    [JAPatch(typeof(List<float>), nameof(Add), PatchType.Postfix, false, ArgumentTypesType = [typeof(float)])]
    public static void Add(List<float> __instance, float item) => Insert(__instance, __instance.Count, item);

    [JAPatch(typeof(List<float>), nameof(Insert), PatchType.Postfix, false, ArgumentTypesType = [typeof(int), typeof(float)])]
    public static void Insert(List<float> __instance, int index, float item) {
        if(currentState == null || __instance != scnEditor.instance.levelData.angleData) return;
        changedFloors.Add(new ChangedFloorCache(ChangedFloorCache.Action.Add, item, index));
        foreach(ChangedEventCache @event in changedEvents) if(@event.action == ChangedEventCache.Action.Remove && @event.@event.floor >= index) @event.@event.floor++;
    }

    [JAPatch(typeof(List<float>), nameof(RemoveAt), PatchType.Prefix, false, ArgumentTypesType = [typeof(int)])]
    public static void RemoveAt(List<float> __instance, int index) {
        if(currentState == null || __instance != scnEditor.instance.levelData.angleData) return;
        changedFloors.Add(new ChangedFloorCache(ChangedFloorCache.Action.Remove, __instance[index], index));
        foreach(ChangedEventCache @event in changedEvents) if(@event.action == ChangedEventCache.Action.Remove && @event.@event.floor >= index) @event.@event.floor--;
    }

    [JAPatch(typeof(scnEditor), "PasteEvents", PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> FixAddEvent(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];
            if(code.opcode == OpCodes.Callvirt && typeof(EventsArray<LevelEvent>).Method(nameof(Add)) == (MethodInfo) code.operand) {
                LocalBuilder local = generator.DeclareLocal(typeof(LevelEvent));
                codes[i] = new CodeInstruction(OpCodes.Stloc, local) { labels = code.labels };
                codes.InsertRange(i + 1, [
                    new CodeInstruction(OpCodes.Ldloc, local),
                    new CodeInstruction(OpCodes.Callvirt, typeof(List<LevelEvent>).Method(nameof(Add))),
                    new CodeInstruction(OpCodes.Ldloc, local),
                    new CodeInstruction(OpCodes.Call, ((Action<LevelEvent>) Add).Method)
                ]);
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(UndoOrRedo), PatchType.Replace, false)]
    public static void UndoOrRedo2(bool redo) => UndoOrRedo(redo);
    public static void UndoOrRedo(bool redo) {
        try {
            scnEditor editor = scnEditor.instance;
            bool redo2 = redo;
            if(editor.changingState != 0) return;
            List<LevelState> source = redo2 ? redoStates : undoStates;
            if(source.Count == 0) return;
            LevelState levelState = source.Last();
            using(new SaveStateScope(editor, false, false, true)) {
                if(levelState is DefaultLevelState defaultLevelState) {
                    foreach(ChangedEventCache cache in defaultLevelState.changedEvents) {
                        if(cache.action == ChangedEventCache.Action.Add && redo2 || cache.action == ChangedEventCache.Action.Remove && !redo2) editor.events.Add(cache.@event);
                        else editor.events.Remove(cache.@event);
                    }
                    foreach(KeyValuePair<EventKey, EventValue> pair in defaultLevelState.changedEventValues) pair.Key.levelEvent[pair.Key.key] = redo2 ? pair.Value.newValue : pair.Value.defaultValue;
                    List<float> angleData = editor.levelData.angleData;
                    if(redo2)
                        foreach(ChangedFloorCache cache in defaultLevelState.changedFloors) {
                            if(cache.action == ChangedFloorCache.Action.Add) {
                                angleData.Insert(cache.index, cache.angle);
                                foreach(LevelEvent @event in editor.events.Concat(editor.decorations))
                                    if(@event.floor >= cache.index)
                                        @event.floor++;
                            } else {
                                angleData.RemoveAt(cache.index);
                                foreach(LevelEvent @event in editor.events.Concat(editor.decorations))
                                    if(@event.floor > cache.index)
                                        @event.floor--;
                            }
                        }
                    else
                        for(int i = defaultLevelState.changedFloors.Length - 1; i >= 0; i--) {
                            ChangedFloorCache cache = defaultLevelState.changedFloors[i];
                            if(cache.action == ChangedFloorCache.Action.Add) {
                                angleData.RemoveAt(cache.index);
                                foreach(LevelEvent @event in editor.events.Concat(editor.decorations))
                                    if(@event.floor > cache.index)
                                        @event.floor--;
                            } else {
                                angleData.Insert(cache.index, cache.angle);
                                foreach(LevelEvent @event in editor.events.Concat(editor.decorations))
                                    if(@event.floor >= cache.index)
                                        @event.floor++;
                            }
                        }
                    foreach(int index in defaultLevelState.selectedDecorationIndices) {
                        if(editor.customLevel.levelData.decorations.Count > index)
                            editor.SelectDecoration(editor.customLevel.levelData.decorations[index], false, false, true);
                    }
                    if(!editor.SelectionDecorationIsEmpty()) {
                        LevelEvent selectedDecoration = editor.selectedDecorations[^1];
                        editor.levelEventsPanel.ShowInspector(true, true);
                        editor.levelEventsPanel.ShowPanel(selectedDecoration.eventType);
                    }
                    UndoTileUpdate.UpdateTile(defaultLevelState, redo2);
                    editor.propertyControlDecorationsList.RefreshItemsList(true);
                    bool reselect = false;
                    int[] selectedFloors = defaultLevelState.selectedFloors;
                    int[] currentSelectedFloors = null;
                    if(!editor.SelectionIsEmpty()) {
                        if(selectedFloors == null) reselect = true;
                        else if(editor.SelectionIsSingle()) {
                            currentSelectedFloors = [editor.selectedFloors[0].seqID];
                            if(selectedFloors.Length != 1 || selectedFloors[0] != editor.selectedFloors[0].seqID) reselect = true;
                        } else if(editor.selectedFloors.Count == selectedFloors.Length) {
                            currentSelectedFloors = new int[editor.selectedFloors.Count];
                            for(int i = 0; i < currentSelectedFloors.Length; i++) {
                                currentSelectedFloors[i] = editor.selectedFloors[i].seqID;
                                if(editor.selectedFloors[i].seqID != selectedFloors[i]) {
                                    reselect = true;
                                    break;
                                }
                            }
                        } else reselect = true;
                    } else if(selectedFloors != null) reselect = true;
                    if(reselect) {
                        editor.DeselectFloors();
                        if(selectedFloors != null) {
                            if(selectedFloors.Length > 1) editor.MultiSelectFloors(editor.floors[selectedFloors[0]], editor.floors[selectedFloors[^1]]);
                            else editor.SelectFloor(editor.floors[selectedFloors[0]]);
                        }
                    }
                    defaultLevelState.selectedFloors = currentSelectedFloors;
                    if(selectedFloors is { Length: 1 }) editor.levelEventsPanel.ShowPanel(defaultLevelState.floorEventType, defaultLevelState.floorEventTypeIndex);
                    editor.settingsPanel.ShowPanel(defaultLevelState.settingsEventType);
                    if(editor.particleEditor.gameObject.activeSelf && editor.particleEditor.SelectedEvent != null) {
                        if(editor.selectedDecorations.Count == 0) editor.HideParticleEditor();
                        else {
                            ParticleEditor particleEditor = editor.particleEditor;
                            List<LevelEvent> selectedDecorations = editor.selectedDecorations;
                            LevelEvent ev = selectedDecorations[^1];
                            particleEditor.SetEvent(ev);
                        }
                    }
                } else if(redo) levelState.Redo();
                else levelState.Undo();
                source.RemoveAt(source.Count - 1);
                if(redo2) undoStates.Add(levelState);
                else redoStates.Add(levelState);
            }
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static IEnumerable<CodeInstruction> NewLevel(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];
            if(code.operand is FieldInfo { Name: nameof(undoStates) }) {
                codes[i - 1] = new CodeInstruction(OpCodes.Ldsfld, SimpleReflect.Field(typeof(SaveStatePatch), nameof(undoStates)));
                codes.RemoveAt(i);
                codes[i] = new CodeInstruction(OpCodes.Callvirt, typeof(List<LevelState>).Method(nameof(List<LevelState>.Clear)));
            } else if(code.operand is FieldInfo { Name: nameof(redoStates) }) {
                codes[i - 1] = new CodeInstruction(OpCodes.Ldsfld, SimpleReflect.Field(typeof(SaveStatePatch), nameof(redoStates)));
                codes.RemoveAt(i);
                codes[i] = new CodeInstruction(OpCodes.Callvirt, typeof(List<LevelState>).Method(nameof(List<LevelState>.Clear)));
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), "Awake", PatchType.Postfix, false)]
    public static void EditorAwake() {
        undoStates.Clear();
        redoStates.Clear();
    }

    public struct EventKey : IEquatable<EventKey> {
        public LevelEvent levelEvent;
        public string key;

        public EventKey(LevelEvent levelEvent, string key) {
            this.levelEvent = levelEvent;
            this.key = key;
        }

        public override bool Equals(object obj) => obj is EventKey value && levelEvent == value.levelEvent && key == value.key;
        public override int GetHashCode() => HashCode.Combine(levelEvent, key);
        public bool Equals(EventKey other) => Equals(levelEvent, other.levelEvent) && key == other.key;
    }

    public class EventValue {
        public object defaultValue;
        public object newValue;

        public EventValue(object defaultValue) {
            this.defaultValue = defaultValue;
        }
    }
}