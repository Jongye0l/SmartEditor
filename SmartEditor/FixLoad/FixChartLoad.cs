using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI.Editor.Actions;
using HarmonyLib;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;

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

    [JAPatch(typeof(scnEditor), "DeleteFloor", PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> DeleteFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1);
                codes[i - 2] = new CodeInstruction(OpCodes.Ldc_I4_1);
                codes[i - 1] = new CodeInstruction(OpCodes.Call, ((Delegate) DeleteTileUpdate.UpdateTile).Method);
                codes.RemoveAt(i);
            }
        return codes;
    }

    [JAPatch(typeof(scnEditor), "DeleteMultiSelection", PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> DeleteMultiSelection(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> codes = instructions.ToList();
        LocalBuilder local = generator.DeclareLocal(typeof(int));
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];
            if(code.operand is MethodInfo { Name: "DeselectAllFloors" }) {
                codes.InsertRange(i - 1, [
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(typeof(scnEditor), "selectedFloors")),
                    new CodeInstruction(OpCodes.Callvirt, typeof(List<scrFloor>).Method("get_Count")),
                    new CodeInstruction(OpCodes.Stloc, local)
                ]);
                i += 4;
            }
            if(code.operand is MethodInfo { Name: "DeleteFloor" } && codes[i - 1].opcode == OpCodes.Ldc_I4_1) {
                codes[i - 1].opcode = OpCodes.Ldc_I4_0;
                codes.InsertRange(i + 2, [
                    codes[i - 2].Clone(),
                    new CodeInstruction(OpCodes.Ldloc, local),
                    new CodeInstruction(OpCodes.Call, ((Delegate) DeleteTileUpdate.UpdateTile).Method)
                ]);
                break;
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), "DeleteSubsequentFloors", PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> DeleteSubsequentFloors(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            if(codes[i].operand is MethodInfo { Name: "DeleteFloor" } && codes[i - 1].opcode == OpCodes.Ldc_I4_1) {
                codes[i - 1].opcode = OpCodes.Ldc_I4_0;
                codes.InsertRange(i + 2, [
                    codes[i - 4].Clone(),
                    new CodeInstruction(OpCodes.Call, ((Delegate) DeleteTileUpdate.UpdateTileSubsequent).Method)
                ]);
                break;
            }
        }
        return codes;
    }


    [JAPatch(typeof(scnEditor), "DeletePrecedingFloors", PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> DeletePrecedingFloors(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            if(codes[i].operand is MethodInfo { Name: "DeleteFloor" } && codes[i - 1].opcode == OpCodes.Ldc_I4_1) {
                codes[i - 1].opcode = OpCodes.Ldc_I4_0;
                codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, ((Delegate) DeleteTileUpdate.UpdateTilePreceding).Method));
                break;
            }
        }
        return codes;
    }

    public static void DrawEditor() {
        scnEditor editor = scnEditor.instance;
        editor.Invoke("DrawFloorOffsetLines");
        editor.Invoke("DrawFloorNums");
        editor.Invoke("DrawMultiPlanet");
    }

    [JAPatch(typeof(FixChartLoad), nameof(DrawEditor), PatchType.Transpiler, false)]
    private static IEnumerable<CodeInstruction> DrawEditorTranspiler(IEnumerable<CodeInstruction> instructions) {
        return [
            new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(scnEditor), "instance")),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(scnEditor), "DrawFloorOffsetLines")),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(scnEditor), "DrawFloorNums")),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(scnEditor), "DrawMultiPlanet")),
            new CodeInstruction(OpCodes.Ret)
        ];
    }

    [JAPatch(typeof(ToggleFloorNumsEditorAction), "Execute", PatchType.Transpiler, false)]
    private static IEnumerable<CodeInstruction> ToggleFloorNumsEditorActionTranspiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i].operand = typeof(scnEditor).Method("DrawFloorNums");
                codes.RemoveRange(i - 2, 2);
            }
        }
        return codes;
    }
}