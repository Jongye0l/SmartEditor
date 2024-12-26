using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using HarmonyLib;
using JALib.Core.Patch;
using JALib.Tools;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class ScopePatch {
    [JAPatch(typeof(scnEditor), nameof(CreateFloor), PatchType.Transpiler, false, ArgumentTypesType = [typeof(float), typeof(bool), typeof(bool)])]
    public static IEnumerable<CodeInstruction> CreateFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(CreateFloorScope).Constructor());
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(DeleteFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DeleteFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(DeleteFloorScope).Constructor(typeof(int)));
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(DeleteSingleSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DeleteSingleSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(DeleteFloorScope).Constructor(typeof(bool)));
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(DeleteMultiSelection), PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "DeletePrecedingFloors", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "DeleteSubsequentFloors", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DeleteMultiSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
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
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list[i - 3] = new CodeInstruction(OpCodes.Stloc, local);
                list[i - 2] = new CodeInstruction(OpCodes.Ldloc, local);
                list.RemoveRange(i - 1, 2);
            } else if(code.operand is MethodInfo { Name: "RemoveEvents" }) {
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
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(List<LevelEvent>)));
                list.RemoveRange(i - 3, 3);
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
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list[i - 3] = new CodeInstruction(OpCodes.Stloc, local);
                list[i - 2] = new CodeInstruction(OpCodes.Ldloc, local);
                list.RemoveRange(i - 1, 2);
            } else if(code.operand is MethodInfo { Name: "RemoveEvent" }) {
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
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldc_I4_0);
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(LevelEvent), typeof(bool)));
                list.RemoveRange(i - 2, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(CutDecoration), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> CutDecoration(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldc_I4_1);
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(LevelEvent), typeof(bool)));
                list.RemoveRange(i - 2, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(MultiCutDecorations), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> MultiCutDecorations(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_0;
                list[i - 3] = new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(typeof(scnEditor), "selectedDecorations"));
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(List<LevelEvent>)));
                list.RemoveRange(i - 2, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(SelectDecoration), PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "DeselectDecoration", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "DeselectAllDecorations", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "SwitchToEditMode", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> SelectDecoration(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(SelectDecorationScope).Constructor()) { labels = list[i - 4].labels };
                list.RemoveRange(i - 4, 4);
            }
        }
        return list;
    }

    [JAPatch(typeof(PasteEventsEditorAction), "Execute", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteEvents(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list.RemoveRange(i - 4, 4);
            }
        }
        return list;
    }
}