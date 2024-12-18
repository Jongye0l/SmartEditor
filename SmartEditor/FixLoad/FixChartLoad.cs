using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JALib.Core;
using JALib.Core.Patch;

namespace SmartEditor.FixLoad;

public class FixChartLoad : Feature {

    public FixChartLoad() : base(Main.Instance, nameof(FixChartLoad), true, typeof(FixChartLoad)) {
    }

    [JAPatch(typeof(scnEditor), "CreateFloor", PatchType.Transpiler, false, ArgumentTypesType = [typeof(float), typeof(bool), typeof(bool)])]
    internal static IEnumerable<CodeInstruction> CreateFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++)
            if(codes[i].operand is MethodInfo { Name: "UpdateDecorationObjects" }) {
                List<Label> label = codes[i - 1].labels;
                codes.RemoveRange(i - 1, 2);
                codes[i - 1].WithLabels(label);
            }
        return codes;
    }

    [JAPatch(typeof(scnEditor), "InsertFloatFloor", PatchType.Transpiler, false)]
    [JAPatch(typeof(scnEditor), "InsertCharFloor", PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> InsertFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1);
                codes[i - 2] = new CodeInstruction(OpCodes.Call, ((Delegate) InsertTileUpdate.UpdateTile).Method);
                codes.RemoveRange(i - 1, 2);
            }
        return codes;
    }


}