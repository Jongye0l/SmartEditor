using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using ADOFAI.LevelEditor.Controls;
using HarmonyLib;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using SmartEditor.FixLoad;
using SmartEditor.FixLoad.CustomSaveState.Scope;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace SmartEditor;

public class SpeedPauseConverter() : Feature(Main.Instance, nameof(SpeedPauseConverter), patchClass: typeof(SpeedPauseConverter)) {
    private static PropertyControl_Export convertPause;
    private static PropertyControl_Export convertSetSpeed;

    protected override void OnEnable() {
        LevelEventInfo levelEventInfo = GCS.levelEventsInfo["SetSpeed"];
        if(levelEventInfo.propertiesInfo.ContainsKey("ConvertPause")) return;
        PropertyInfo propertyInfo = new(new Dictionary<string, object> {
            { "name", "ConvertPause" },
            { "type", "Export" }
        }, levelEventInfo) {
            order = 0
        };
        levelEventInfo.propertiesInfo.TryAdd(propertyInfo.name, propertyInfo);
        levelEventInfo = GCS.levelEventsInfo["Pause"];
        if(levelEventInfo.propertiesInfo.ContainsKey("ConvertSetSpeed")) return;
        propertyInfo = new PropertyInfo(new Dictionary<string, object> {
            { "name", "ConvertSetSpeed" },
            { "type", "Export" }
        }, levelEventInfo) {
            order = 0
        };
        levelEventInfo.propertiesInfo.TryAdd(propertyInfo.name, propertyInfo);
    }

    protected override void OnDisable() {
        LevelEventInfo levelEventInfo = GCS.levelEventsInfo["SetSpeed"];
        levelEventInfo.propertiesInfo.Remove("ConvertPause");
        levelEventInfo = GCS.levelEventsInfo["Pause"];
        levelEventInfo.propertiesInfo.Remove("ConvertSetSpeed");
    }

    [JAPatch(typeof(PropertyControl), nameof(Setup), PatchType.Postfix, false)]
    private static void Setup(PropertyControl __instance) {
        if(__instance is not PropertyControl_Export export) return;
        if(__instance.propertyInfo.name == "ConvertPause") {
            export.exportButton.onClick.RemoveAllListeners();
            export.exportButton.onClick.AddListener(ConvertPause);
            export.buttonText.text = Main.Instance.Localization["SpeedPauseConverter.ConvertPause"];
            convertPause = export;
        } else if(__instance.propertyInfo.name == "ConvertSetSpeed") {
            export.exportButton.onClick.RemoveAllListeners();
            export.exportButton.onClick.AddListener(ConvertSetSpeed);
            export.buttonText.text = Main.Instance.Localization["SpeedPauseConverter.ConvertSetSpeed"];
            convertSetSpeed = export;
        }
    }

    private static void ConvertPause() {
        scnEditor editor = ADOBase.editor;
        LevelEvent currentEvent = convertPause.propertiesPanel.inspectorPanel.selectedEvent;
        IDisposable scope = FixChartLoad.instance.Enabled ? new SpeedPauseConvertScope(currentEvent) : new SaveStateScope(editor);
        try {
            scrFloor curFloor = editor.floors[currentEvent.floor];
            scrFloor preFloor = curFloor.prevfloor;
            double angle = GetAngle(curFloor);
            editor.events.Remove(currentEvent);
            LevelEvent levelEvent = typeof(LevelEvent).New<LevelEvent>(curFloor.seqID, LevelEventType.Pause);
            levelEvent["duration"] = (float) ((preFloor.speed / curFloor.speed - 1) * angle / 180);
            editor.events.Add(levelEvent);
            editor.levelEventsPanel.selectedEventType = LevelEventType.Pause;
            editor.DecideInspectorTabsAtSelected();
            editor.levelEventsPanel.ShowPanel(LevelEventType.Pause);
            if(scope is SpeedPauseConvertScope speedScope) speedScope.levelEvent = levelEvent;
            editor.ApplyEventsToFloors();
            editor.ShowEventIndicators(editor.selectedFloors[0]);
        } finally {
            scope.Dispose();
        }
    }

    private static void ConvertSetSpeed() {
        scnEditor editor = ADOBase.editor;
        LevelEvent currentEvent = convertSetSpeed.propertiesPanel.inspectorPanel.selectedEvent;
        IDisposable scope = FixChartLoad.instance.Enabled ? new SpeedPauseConvertScope(currentEvent) : new SaveStateScope(editor);
        try {
            scrFloor curFloor = editor.floors[currentEvent.floor];
            scrFloor preFloor = curFloor.prevfloor;
            double angle = GetAngle(curFloor);
            editor.events.Remove(currentEvent);
            LevelEvent levelEvent = typeof(LevelEvent).New<LevelEvent>(curFloor.seqID, LevelEventType.SetSpeed);
            levelEvent["beatsPerMinute"] = (float) (editor.levelData.bpm * preFloor.speed / (currentEvent.GetFloat("duration") + (angle / 180)) * angle / 180);
            editor.events.Add(levelEvent);
            editor.levelEventsPanel.selectedEventType = LevelEventType.SetSpeed;
            editor.DecideInspectorTabsAtSelected();
            editor.levelEventsPanel.ShowPanel(LevelEventType.SetSpeed);
            if(scope is SpeedPauseConvertScope speedScope) speedScope.levelEvent = levelEvent;
            editor.ApplyEventsToFloors();
            editor.ShowEventIndicators(editor.selectedFloors[0]);
        } finally {
            scope.Dispose();
        }
    }

    private static double GetAngle(scrFloor prevFloor) {
        scrFloor curFloor = prevFloor.nextfloor;
        double prevAngle = prevFloor.floatDirection;
        if(prevAngle == 999) prevAngle = prevFloor.prevfloor.floatDirection;
        double curAngle = curFloor.floatDirection;
        if(curAngle == 999) curAngle = curFloor.nextfloor?.floatDirection ?? prevAngle + 180;
        return NormalizeAngle((180 + prevAngle - curAngle) * (curFloor.isCCW ? -1 : 1));
    }

    private static double NormalizeAngle(double angle) {
        angle = Math.Round(angle, 4);
        if(angle == 999) return angle;
        while(angle < 0) angle += 360;
        angle %= 360;
        return angle == 0 ? 360 : angle;
    }

    [JAPatch(typeof(PropertyControl), nameof(UpdateEnabled), PatchType.Prefix, false)]
    private static bool UpdateEnabled(PropertyControl __instance) {
        if(__instance is not PropertyControl_Export export) return true;
        if(export.propertyInfo.name == "ConvertPause") {
            int seqId = export.propertiesPanel.inspectorPanel.selectedEvent.floor;
            bool enable = true;
            bool firstSetSpeed = true;
            LevelEvent currentEvent = export.propertiesPanel.inspectorPanel.selectedEvent;
            if((SpeedType) currentEvent["speedType"] == SpeedType.Bpm) {
                scrFloor curFloor = scnEditor.instance.floors[seqId];
                if(currentEvent.GetFloat("beatsPerMinute") / scnEditor.instance.levelData.bpm > curFloor.prevfloor.speed) enable = false;
            } else if(currentEvent.GetFloat("bpmMultiplier") > 1) enable = false;
            foreach(LevelEvent evnt in scnEditor.instance.events) {
                if(evnt.floor != seqId) continue;
                switch(evnt.eventType) {
                    case LevelEventType.SetSpeed:
                        if(firstSetSpeed) firstSetSpeed = false;
                        else goto case LevelEventType.Pause;
                        break;
                    case LevelEventType.Pause:
                    case LevelEventType.Hold:
                    case LevelEventType.FreeRoam:
                        enable = false;
                        break;
                }
                if(!enable) break;
            }
            export.SetEnabled(enable);
            return false;
        }
        if(export.propertyInfo.name == "ConvertSetSpeed") {
            int seqId = export.propertiesPanel.inspectorPanel.selectedEvent.floor;
            bool enable = true;
            bool firstPause = true;
            foreach(LevelEvent evnt in scnEditor.instance.events) {
                if(evnt.floor != seqId) continue;
                switch(evnt.eventType) {
                    case LevelEventType.Pause:
                        if(firstPause) firstPause = false;
                        else goto case LevelEventType.SetSpeed;
                        break;
                    case LevelEventType.SetSpeed:
                    case LevelEventType.Hold:
                    case LevelEventType.FreeRoam:
                        enable = false;
                        break;
                }
                if(!enable) break;
            }
            export.SetEnabled(enable);
            return false;
        }
        export.SetEnabled(true, SteamIntegration.initialized);
        return false;
    }

    [JAPatch(typeof(PropertiesPanel), nameof(RenderControl), PatchType.Transpiler, false)]
    private static IEnumerable<CodeInstruction> RenderControl(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> instructionList = instructions.ToList();
        foreach(CodeInstruction t in instructionList) {
            if(t.operand is not FieldInfo field || field != SimpleReflect.Field(typeof(SteamIntegration), "initialized")) continue;
            t.opcode = OpCodes.Ldc_I4_1;
            t.operand = null;
            break;
        }
        return instructionList;
    }
}