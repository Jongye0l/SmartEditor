using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using ADOFAI.Editor.Actions;
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
    [JAPatch(typeof(scnEditor), "MultiCutFloors", PatchType.Transpiler, false)]
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
                list[i - 3] = new CodeInstruction(OpCodes.Ldarg_2);
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(LevelEvent), typeof(int)));
                list.RemoveRange(i - 2, 2);
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
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(LevelEvent)));
                list.RemoveRange(i - 3, 3);
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

    [JAPatch(typeof(scnEditor), nameof(SelectDecoration), PatchType.Transpiler, false, ArgumentTypesType = [typeof(LevelEvent), typeof(bool), typeof(bool), typeof(bool), typeof(bool)])]
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

    [JAPatch(typeof(scnEditor), nameof(DuplicateDecorations), PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "PasteDecorations", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "PasteEvents", PatchType.Transpiler, false)]
    [JAPatch(typeof(PasteEventsEditorAction), "Execute", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DuplicateDecorations(IEnumerable<CodeInstruction> instructions) {
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

    [JAPatch(typeof(scnEditor), nameof(DragDecorationsStart), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DragDecorationsStart(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(DecorationLocationChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list.RemoveRange(i - 4, 4);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(FlipFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldarg_2);
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(FlipFloorsScope).Constructor(typeof(scrFloor), typeof(bool)));
                list.RemoveRange(i - 2, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(FlipSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(FlipFloorsScope).Constructor(typeof(bool)));
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteFloors), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteFloors(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(PasteFloorsScope).Constructor());
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteHitsoundSingleTile), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteHitsoundSingleTile(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(PasteHitSoundScope).Constructor());
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteTrackColor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteTrackColor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldc_I4_0);
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(PasteTrackColorScope).Constructor());
                list.RemoveRange(i - 2, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteTrackColorSingleTile), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteTrackColorSingleTile(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldc_I4_1);
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(PasteTrackColorScope).Constructor());
                list.RemoveRange(i - 2, 2);
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
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                list[i - 3] = new CodeInstruction(OpCodes.Stloc, local);
                list[i - 2] = new CodeInstruction(OpCodes.Ldloc, local);
                list.RemoveRange(i - 1, 2);
            } else if(code.operand is MethodInfo { Name: "RemoveEvent" }) {
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
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i - 3] = new CodeInstruction(OpCodes.Ldarg_2);
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(RotateFloorsScope).Constructor(typeof(scrFloor), typeof(int)));
                list.RemoveRange(i - 2, 2);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(RotateFloor180), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloor180(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
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
                if(first) {
                    list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                    list.RemoveRange(i - 4, 4);
                    first = false;
                } else {
                    list[i - 4].opcode = OpCodes.Ldc_I4_1;
                    list[i] = new CodeInstruction(OpCodes.Newobj, typeof(PasteFloorsScope).Constructor(typeof(bool)));
                    list.RemoveRange(i - 3, 3);
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
                if(first) {
                    list[i] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor([])) { labels = list[i - 4].labels };
                    list.RemoveRange(i - 4, 4);
                    first = false;
                } else {
                    list[i - 4].opcode = OpCodes.Ldc_I4_0;
                    list[i] = new CodeInstruction(OpCodes.Newobj, typeof(PasteFloorsScope).Constructor(typeof(bool)));
                    list.RemoveRange(i - 3, 3);
                }
            }
        }
        return list;
    }
}