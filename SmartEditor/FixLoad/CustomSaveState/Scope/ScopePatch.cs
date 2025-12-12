using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using ADOFAI.Editor.Actions;
using ADOFAI.LevelEditor.Controls;
using HarmonyLib;
using JALib.Core.Patch;
using JALib.Tools;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class ScopePatch {
    public static void Patch(JAPatcher patcher) {
        patcher.AddPatch(typeof(ScopePatch));
        foreach(MethodInfo method in typeof(PropertyControl_LongText).Methods().Concat(typeof(PropertyControl_Text).Methods()))
            if(method.Name.StartsWith("<Setup>")) patcher.AddPatch(((Delegate) Save).Method, new JAPatchAttribute(method, PatchType.Transpiler, false));
        foreach(MethodInfo method in typeof(PropertyControl_DecorationsList).Methods())
            if(method.Name.StartsWith("<Awake>")) patcher.AddPatch(((Delegate) SelectDecoration).Method, new JAPatchAttribute(method, PatchType.Transpiler, false));
        foreach(MethodInfo method in typeof(PropertyControl_FilterProperties).Methods())
            if(method.Name.StartsWith("<ReloadFilterProperties>")) patcher.AddPatch(((Delegate) ChangeEventDisable).Method, new JAPatchAttribute(method, PatchType.Transpiler, false));
        foreach(MethodInfo method in typeof(PropertiesPanel).Methods())
            if(method.Name.StartsWith("<RenderControl>")) patcher.AddPatch(((Delegate) ChangeEventDisable).Method, new JAPatchAttribute(method, PatchType.Transpiler, false));
    }

    [JAPatch(typeof(scnEditor), nameof(CreateFloor), PatchType.Transpiler, false, ArgumentTypesType = [typeof(float), typeof(bool), typeof(bool)])]
    public static IEnumerable<CodeInstruction> CreateFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new CreateFloorScope(floorAngle))
            // ---- original code IL ----
            // IL_0022: ldarg.0      // this
            // IL_0023: ldc.i4.0
            // IL_0024: ldc.i4.1
            // IL_0025: ldc.i4.0
            // IL_0026: newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // floorAngle
            // newobj       instance void CreateFloorScope::.ctor(float)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(CreateFloorScope).Constructor());
                list.RemoveRange(i - 2, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(DeleteFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DeleteFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new DeleteFloorScope(sequenceIndex))
            // ---- original code IL ----
            // IL_000a: ldarg.0      // this
            // IL_000b: ldc.i4.0
            // IL_000c: ldc.i4.1
            // IL_000d: ldc.i4.0
            // IL_000e: newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // sequenceIndex
            // newobj       instance void DeleteFloorScope::.ctor(int)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(DeleteFloorScope).Constructor(typeof(int)));
                list.RemoveRange(i - 2, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(DeleteSingleSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DeleteSingleSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new DeleteFloorScope(backspace))
            // ---- original code IL ----
            // IL_0035: ldarg.0      // this
            // IL_0036: ldc.i4.0
            // IL_0037: ldc.i4.1
            // IL_0038: ldc.i4.0
            // IL_0039: newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // backspace
            // newobj       instance void DeleteFloorScope::.ctor(bool)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(DeleteFloorScope).Constructor(typeof(bool)));
                list.RemoveRange(i - 2, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(DeleteMultiSelection), PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "DeletePrecedingFloors", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "DeleteSubsequentFloors", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "MultiCutFloors", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DeleteMultiSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new DeleteMultiFloorScope())
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // newobj       instance void DeleteMultiFloorScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(DeleteMultiFloorScope).Constructor()) { labels = list[i - 4].labels };
                list.RemoveRange(i - 4, 4);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), "DeselectFloors", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), nameof(SelectFloor), PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "MultiSelectFloors", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> SelectFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new SelectFloorScope())
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldc.i4.0
            // newobj       instance void SelectFloorScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 1].labels = list[i - 4].labels;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(SelectFloorScope).Constructor());
                list.RemoveRange(i - 4, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(CutFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> CutFloor(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> list = instructions.ToList();
        LocalBuilder local = generator.DeclareLocal(typeof(DeleteFloorScope));
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // DeleteFloorScope local;
            // using (local = new DeleteFloorScope(sequenceIndex))
            // ---- original code IL ----
            // IL_000d: ldarg.0      // this
            // IL_000e: ldc.i4.0
            // IL_000f: ldc.i4.1
            // IL_0010: ldc.i4.0
            // IL_0011: newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // newobj       instance void EventsChangeScope::.ctor()
            // stloc        local
            // ldloc        local
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list[i - 3] = new CodeInstruction(OpCodes.Stloc, local);
                list[i - 2] = new CodeInstruction(OpCodes.Ldloc, local);
                list.RemoveRange(i - 1, 2);
            } 
            // ---- original code C# ----
            // this.RemoveEvents(events);
            // ---- replace code C# ----
            // this.RemoveEvents(events);
            // local.SetEvents(events);
            // ---- original code IL ----
            // IL_00ab: ldarg.0      // this
            // IL_00ac: ldloc.2      // events
            // IL_00ad: call         instance void scnEditor::RemoveEvents(class [mscorlib]System.Collections.Generic.List`1<class ADOFAI.LevelEvent>)
            // ---- replace code IL ----
            // ldarg.0      // this
            // ldloc.2      // events
            // call         instance void scnEditor::RemoveEvents(class [mscorlib]System.Collections.Generic.List`1<class ADOFAI.LevelEvent>)
            // ldloc        local
            // ldloc.2      // events
            // call        instance void EventsChangeScope::SetEvents(class [mscorlib]System.Collections.Generic.List`1<class ADOFAI.LevelEvent>)
            else if(code.operand is MethodInfo { Name: "RemoveEvents" }) {
                list.InsertRange(i + 1, [
                    new CodeInstruction(OpCodes.Ldloc, local),
                    list[i - 1],
                    new CodeInstruction(OpCodes.Call, typeof(EventsChangeScope).Method("SetEvents"))
                ]);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(RemoveEvents), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RemoveEvents(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new EventsChangeScope(events))
            // ---- original code IL ----
            // IL_000c: ldarg.0      // this
            // IL_000d: ldc.i4.0
            // IL_000e: ldc.i4.1
            // IL_000f: ldc.i4.0
            // IL_0010: newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // events
            // newobj       instance void EventsChangeScope::.ctor(class [mscorlib]System.Collections.Generic.List`1<class ADOFAI.LevelEvent>)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(List<LevelEvent>)));
                list.RemoveRange(i - 2, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(AddEventAtSelected), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> AddEventAtSelected(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> list = instructions.ToList();
        LocalBuilder local = generator.DeclareLocal(typeof(EventsChangeScope));
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // EventsChangeScope local;
            // using (local = new EventsChangeScope())
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // newobj       instance void EventsChangeScope::.ctor()
            // stloc        local
            // ldloc        local
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list[i - 3] = new CodeInstruction(OpCodes.Stloc, local);
                list[i - 2] = new CodeInstruction(OpCodes.Ldloc, local);
                list.RemoveRange(i - 1, 2);
            }
            // ---- original code C# ----
            // this.RemoveEvent(eventToRemove);
            // ---- replace code C# ----
            // this.RemoveEvent(eventToRemove);
            // local.DeleteEvents(eventToRemove);
            // ---- original code IL ----
            // IL_02dc: ldarg.0      // this
            // IL_02dd: ldloc.3      // evnt
            // IL_02de: ldc.i4.0
            // IL_02df: call         instance void scnEditor::RemoveEvent(class ADOFAI.LevelEvent, bool)
            // ---- replace code IL ----
            // ldarg.0      // this
            // ldloc.3      // evnt
            // ldc.i4.0
            // call         instance void scnEditor::RemoveEvent(class ADOFAI.LevelEvent, bool)
            // ldloc        local
            // ldloc.3      // evnt
            // call        instance void EventsChangeScope::DeleteEvents(class ADOFAI.LevelEvent)
            else if(code.operand is MethodInfo { Name: "RemoveEvent" }) {
                list.InsertRange(i + 1, [
                    new CodeInstruction(OpCodes.Ldloc, local),
                    list[i - 2],
                    new CodeInstruction(OpCodes.Call, typeof(EventsChangeScope).Method("DeleteEvents"))
                ]);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(AddDecoration), PatchType.Transpiler, false, ArgumentTypesType = [typeof(LevelEvent), typeof(int)])]
    public static IEnumerable<CodeInstruction> AddDecoration(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new EventsChangeScope(dec, index))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // dec
            // ldarg.2      // index
            // newobj       instance void EventsChangeScope::.ctor(class ADOFAI.LevelEvent, int32)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldarg_2);
                list[i - 2] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(LevelEvent), typeof(int)));
                list.RemoveRange(i - 1, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), "CutDecoration", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), nameof(RemoveEvent), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RemoveEvent(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new EventsChangeScope(evnt))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // evnt
            // newobj       instance void EventsChangeScope::.ctor(class ADOFAI.LevelEvent)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(LevelEvent)));
                list.RemoveRange(i - 2, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(MultiCutDecorations), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> MultiCutDecorations(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new EventsChangeScope(this.selectedDecorations))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.0      // this
            // ldfld        class System.Collections.Generic.List`1<ADOFAI.LevelEvent> scnEditor::selectedDecorations
            // newobj       instance void EventsChangeScope::.ctor(class System.Collections.Generic.List`1<ADOFAI.LevelEvent>)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_0;
                list[i - 3] = new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(typeof(scnEditor), "selectedDecorations"));
                list[i - 2] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(List<LevelEvent>)));
                list.RemoveRange(i - 1, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(SelectDecoration), PatchType.Transpiler, false, ArgumentTypesType = [typeof(LevelEvent), typeof(bool), typeof(bool), typeof(bool), typeof(bool)])]
    [JAPatch(typeof(scnEditor), "DeselectDecoration", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "DeselectAllDecorations", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "SwitchToEditMode", PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_List), "SelectItemsInRange", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> SelectDecoration(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new SelectDecorationScope())
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // newobj       instance void SelectDecorationScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(SelectDecorationScope).Constructor()) { labels = list[i - 4].labels };
                list.RemoveRange(i - 3, 4);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(DuplicateDecorations), PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "PasteDecorations", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "PasteEvents", PatchType.Transpiler, false)]
    [JAPatch(typeof(PasteEventsEditorAction), "Execute", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DuplicateDecorations(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new EventsChangeScope())
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // newobj       instance void EventsChangeScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list.RemoveRange(i - 3, 4);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(DragDecorationsStart), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DragDecorationsStart(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new DecorationLocationChangeScope())
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // newobj       instance void DecorationLocationChangeScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(DecorationLocationChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list.RemoveRange(i - 3, 4);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(FlipFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new FlipFloorsScope(floor, horizontal))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // floor
            // ldarg.2      // horizontal
            // newobj       instance void FlipFloorsScope::.ctor(class ADOFAI.scrFloor, bool)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldarg_2);
                list[i - 2] = new CodeInstruction(OpCodes.Newobj, typeof(FlipFloorsScope).Constructor(typeof(scrFloor), typeof(bool)));
                list.RemoveRange(i - 1, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(FlipSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new FlipFloorsScope(horizontal))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // horizontal
            // newobj       instance void FlipFloorsScope::.ctor(bool)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(FlipFloorsScope).Constructor(typeof(bool)));
                list.RemoveRange(i - 2, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteFloors), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteFloors(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new PasteFloorsScope(alsoPasteDecorations))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // alsoPasteDecorations
            // newobj       instance void PasteFloorsScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(PasteFloorsScope).Constructor());
                list.RemoveRange(i - 2, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteHitsoundSingleTile), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteHitsoundSingleTile(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new PasteHitSoundScope(id))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // id
            // newobj       instance void PasteHitSoundScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(PasteHitSoundScope).Constructor());
                list.RemoveRange(i - 2, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteTrackColor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteTrackColor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new PasteTrackColorScope(id, false))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // id
            // ldc.i4.0     
            // newobj       instance void PasteTrackColorScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldc_I4_0);
                list[i - 2] = new CodeInstruction(OpCodes.Newobj, typeof(PasteTrackColorScope).Constructor());
                list.RemoveRange(i - 1, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteTrackColorSingleTile), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteTrackColorSingleTile(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new PasteTrackColorScope(id, true))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // id
            // ldc.i4.1
            // newobj       instance void PasteTrackColorScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldc_I4_1);
                list[i - 2] = new CodeInstruction(OpCodes.Newobj, typeof(PasteTrackColorScope).Constructor());
                list.RemoveRange(i - 1, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(RemoveEventAtSelected), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RemoveEventAtSelected(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> list = instructions.ToList();
        LocalBuilder local = generator.DeclareLocal(typeof(EventsChangeScope));
        LocalBuilder local2 = generator.DeclareLocal(typeof(LevelEvent));
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // EventsChangeScope local;
            // using (local = new EventsChangeScope())
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // newobj       instance void EventsChangeScope::.ctor()
            // stloc        local
            // ldloc        local
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list[i - 3] = new CodeInstruction(OpCodes.Stloc, local);
                list[i - 2] = new CodeInstruction(OpCodes.Ldloc, local);
                list.RemoveRange(i - 1, 2);
            } else if(code.operand is MethodInfo { Name: "RemoveEvent" }) {
                // ---- original code C# ----
                // this.RemoveEvent(evnt);
                // ---- replace code C# ----
                // LevelEvent local2 = evnt;
                // local.DeleteEvents(local2);
                // this.RemoveEvent(local2);
                // ---- original code IL ----
                // IL_006d: ldarg.0      // this
                // IL_006e: ldloc.1      // selectedFloorEvents
                // IL_006f: ldloc.0      // index
                // IL_0070: callvirt     instance !0/*class ADOFAI.LevelEvent*/ class [mscorlib]System.Collections.Generic.List`1<class ADOFAI.LevelEvent>::get_Item(int32)
                // IL_0075: ldc.i4.0
                // IL_0076: call         instance void scnEditor::RemoveEvent(class ADOFAI.LevelEvent, bool)
                // ---- replace code IL ----
                // ldarg.0      // this
                // ldloc.1      // selectedFloorEvents
                // ldloc.0      // index
                // callvirt     instance !0/*class ADOFAI.LevelEvent*/ class [mscorlib]System.Collections.Generic.List`1<class ADOFAI.LevelEvent>::get_Item(int32)
                // stloc.s      local2
                // ldloc        local
                // ldloc.s      local2
                // call         instance void EventsChangeScope::DeleteEvents(class ADOFAI.LevelEvent)
                // ldloc.s      local2
                // ldc.i4.0
                // call         instance void scnEditor::RemoveEvent(class ADOFAI.LevelEvent, bool)
                list.InsertRange(i - 1, [
                    new CodeInstruction(OpCodes.Stloc, local2),
                    new CodeInstruction(OpCodes.Ldloc, local),
                    new CodeInstruction(OpCodes.Ldloc, local2),
                    new CodeInstruction(OpCodes.Call, typeof(EventsChangeScope).Method("DeleteEvents")),
                    new CodeInstruction(OpCodes.Ldloc, local2)
                ]);
                i += 5;
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(RotateFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new RotateFloorsScope(floor, CW))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // floor
            // ldarg.2      // CW
            // newobj       instance void RotateFloorsScope::.ctor(class ADOFAI.scrFloor, int32)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldarg_2);
                list[i - 2] = new CodeInstruction(OpCodes.Newobj, typeof(RotateFloorsScope).Constructor(typeof(scrFloor), typeof(int)));
                list.RemoveRange(i - 1, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(RotateFloor180), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloor180(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new RotateFloorsScope(floor, 2))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // floor
            // ldc.i4.2
            // newobj       instance void RotateFloorsScope::.ctor(class ADOFAI.scrFloor, int32)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldc_I4_2);
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(RotateFloorsScope).Constructor(typeof(scrFloor), typeof(int)));
                list.RemoveRange(i - 2, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(RotateSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new RotateFloorsScope(CW))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.1      // CW
            // newobj       instance void RotateFloorsScope::.ctor(int32)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(RotateFloorsScope).Constructor(typeof(int)));
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(RotateSelection180), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateSelection180(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this))
            // ---- replace code C# ----
            // using (new RotateFloorsScope(2))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldc.i4.2
            // newobj       instance void RotateFloorsScope::.ctor(int32)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldc_I4_2;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(RotateFloorsScope).Constructor(typeof(int)));
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(PasteFloorEditorAction), "Execute", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteFloorEditorAction(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        bool first = true;
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                // ---- original code C# ----
                // using (new SaveStateScope(this))
                // ---- replace code C# ----
                // using (new EventsChangeScope())
                // ---- original code IL ----
                // ldarg.0      // this
                // ldc.i4.0
                // ldc.i4.1
                // ldc.i4.0
                // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
                // ---- replace code IL ----
                // newobj       instance void EventsChangeScope::.ctor()
                if(first) {
                    list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                    list.RemoveRange(i - 3, 4);
                    first = false;
                }
                // ---- original code C# ----
                // using (new SaveStateScope(this))
                // ---- replace code C# ----
                // using (new PasteFloorsScope(1))
                // ---- original code IL ----
                // ldarg.0      // this
                // ldc.i4.0
                // ldc.i4.1
                // ldc.i4.0
                // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
                // ---- replace code IL ----
                // ldc.i4.1
                // newobj       instance void PasteFloorsScope::.ctor(bool)
                else {
                    list[i - 4].opcode = OpCodes.Ldc_I4_1;
                    list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(PasteFloorsScope).Constructor(typeof(bool)));
                    list.RemoveRange(i - 2, 3);
                }
            }
        }
        return list;
    }

    [JAPatch(typeof(PasteFloorWithoutDecorationsEditorAction), "Execute", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteFloorWithoutDecorationsEditorAction(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        bool first = true;
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                // ---- original code C# ----
                // using (new SaveStateScope(this))
                // ---- replace code C# ----
                // using (new EventsChangeScope())
                // ---- original code IL ----
                // ldarg.0      // this
                // ldc.i4.0
                // ldc.i4.1
                // ldc.i4.0
                // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
                // ---- replace code IL ----
                // newobj       instance void EventsChangeScope::.ctor()
                if(first) {
                    list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                    list.RemoveRange(i - 3, 4);
                    first = false;
                }
                // ---- original code C# ----
                // using (new SaveStateScope(this))
                // ---- replace code C# ----
                // using (new PasteFloorsScope(false))
                // ---- original code IL ----
                // ldarg.0      // this
                // ldc.i4.0
                // ldc.i4.1
                // ldc.i4.0
                // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
                // ---- replace code IL ----
                // ldc.i4.0
                // newobj       instance void PasteFloorsScope::.ctor(bool)
                else {
                    list[i - 4].opcode = OpCodes.Ldc_I4_0;
                    list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(PasteFloorsScope).Constructor(typeof(bool)));
                    list.RemoveRange(i - 2, 3);
                }
            }
        }
        return list;
    }

    [JAPatch(typeof(InspectorPanel), nameof(ShowPanel), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> ShowPanel(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(this.editor))
            // ---- replace code C# ----
            // using (new ShowPanelScope(this, eventType))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.0      // this
            // ldarg.1      // eventType
            // newobj       instance void ShowPanelScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_0;
                list[i - 3] = new CodeInstruction(OpCodes.Ldarg_1);
                list[i - 2] = new CodeInstruction(OpCodes.Newobj, typeof(ShowPanelScope).Constructor());
                list.RemoveRange(i - 1, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(PropertyControl_Bool), "SetValue", PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_Color), "OnChange", PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_File), "OnRightClick", PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_File), "ProcessFile", PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_FloatPair), nameof(Save), PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_MinMaxGradient), nameof(Save), PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_Rating), "SetInt", PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_Toggle), "SelectVar", PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_Vector2), "SetVectorVals", PatchType.Transpiler, false)]
    [JAPatch(typeof(PropertyControl_Vector2Range), "SetVectorVals", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> Save(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(editor))
            // ---- replace code C# ----
            // using (new EventValueChangeScope(this))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.0      // this
            // newobj       instance void EventValueChangeScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_0;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventValueChangeScope).Constructor());
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    public static IEnumerable<CodeInstruction> ChangeEventDisable(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(editor))
            // ---- replace code C# ----
            // using (new EventDisableChangeScope(this))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.0      // this
            // newobj       instance void EventDisableChangeScope::.ctor()
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_0;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventDisableChangeScope).Constructor());
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(PropertyControl_List), nameof(EndDrag), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> EndDrag(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            // ---- original code C# ----
            // using (new SaveStateScope(editor))
            // ---- replace code C# ----
            // using (new DecoDragScope(this))
            // ---- original code IL ----
            // ldarg.0      // this
            // ldc.i4.0
            // ldc.i4.1
            // ldc.i4.0
            // newobj       instance void SaveStateScope::.ctor(class scnEditor, bool, bool, bool)
            // ---- replace code IL ----
            // ldarg.0      // this
            // newobj       instance void DecoDragScope::.ctor(class PropertyControl_List)
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_0;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(DecoDragScope).Constructor(typeof(PropertyControl_List)));
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }
}