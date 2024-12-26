using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(DeleteFloorScope).Constructor());
                list.RemoveRange(i - 3, 3);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), "DeselectFloors", PatchType.Transpiler, false, Debug = true)]
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
}