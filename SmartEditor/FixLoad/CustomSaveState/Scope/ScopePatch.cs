using System;
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
        CodeInstruction scope = null;
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode == OpCodes.Newobj && (ConstructorInfo) code.operand == typeof(SaveStateScope).Constructor()) {
                list[i - 4].opcode = OpCodes.Ldarg_1;
                list[i] = new CodeInstruction(OpCodes.Newobj, typeof(CreateFloorScope).Constructor());
                list.RemoveRange(i - 3, 3);
                i -= 2;
                scope = ReverseLocal(list[i]);
            } else if(code.operand is MethodInfo { Name: "DeleteFloor" }) {
                list.InsertRange(i + 1, [
                    scope,
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Stfld, typeof(CreateFloorScope).GetField("deleted"))
                ]);
            }
        }
        return list;
    }

    private static CodeInstruction ReverseLocal(CodeInstruction code) {
        if(code.opcode == OpCodes.Stloc) return new CodeInstruction(OpCodes.Ldloc, code.operand);
        if(code.opcode == OpCodes.Stloc_0) return new CodeInstruction(OpCodes.Ldloc_0);
        if(code.opcode == OpCodes.Stloc_1) return new CodeInstruction(OpCodes.Ldloc_1);
        if(code.opcode == OpCodes.Stloc_2) return new CodeInstruction(OpCodes.Ldloc_2);
        if(code.opcode == OpCodes.Stloc_3) return new CodeInstruction(OpCodes.Ldloc_3);
        throw new Exception("Invalid opcode: " + code);
    }
}