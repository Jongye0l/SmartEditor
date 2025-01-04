using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JALib.Core.Patch;
using Patches = FlipAndRotateTiles.Patches;

namespace SmartEditor.FixLoad;

public class FlipAndRotateTilesPatch {
    public static int customRunner;

    public static IEnumerable<CodeInstruction> Setup(IEnumerable<CodeInstruction> instructions, CodeInstruction code) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction cur = list[i];
            if(cur.operand is MethodInfo { Name: "RemakePath" }) {
                CodeInstruction prev = list[i - 3];
                list[i + 1].WithLabels(prev.labels).WithBlocks(prev.blocks);
                list.RemoveRange(i - 3, 4);
            }
        }
        return list;
    }

    [JAPatch(typeof(Patches.FlipSelectionPatch), "Postfix", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipSelectionPatch(IEnumerable<CodeInstruction> instructions) =>
        Setup(instructions, new CodeInstruction(OpCodes.Ldc_I4_1));

    [JAPatch(typeof(scnEditor), nameof(FlipSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            if(list[i].operand is MethodInfo { Name: "UpdateTileSelection" }) {
                list.InsertRange(i, [
                    new CodeInstruction(OpCodes.Call, ((Delegate) RunFlipSelectionPatch).Method),
                    new CodeInstruction(OpCodes.Ldarg_1)
                ]);
                break;
            }
        }
        return list;
    }

    public static void RunFlipSelectionPatch(bool horizontal) {
        customRunner |= 1;
        try {
            Patches.FlipSelectionPatch.Postfix(horizontal);
        } finally {
            customRunner &= ~1;
        }
    }

    [JAPatch(typeof(Patches.FlipFloorPatch), "Postfix", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipFloorPatch(IEnumerable<CodeInstruction> instructions) =>
        Setup(instructions, new CodeInstruction(OpCodes.Ldc_I4_2));

    [JAPatch(typeof(scnEditor), nameof(FlipFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            if(list[i].operand is MethodInfo { Name: "UpdateTile" }) {
                list.InsertRange(i - 5, [
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Call, ((Delegate) RunFlipPatch).Method)
                ]);
                break;
            }
        }
        return list;
    }

    public static void RunFlipPatch(scrFloor floor, bool horizontal, bool remakePath) {
        customRunner |= 2;
        try {
            Patches.FlipFloorPatch.Postfix(floor, horizontal, remakePath);
        } finally {
            customRunner &= ~2;
        }
    }

    [JAPatch(typeof(Patches.RotateSelectionPatch), "Postfix", PatchType.Transpiler, false, Debug = true)]
    public static IEnumerable<CodeInstruction> RotateSelectionPatch(IEnumerable<CodeInstruction> instructions) =>
        Setup(instructions, new CodeInstruction(OpCodes.Ldc_I4_4));

    [JAPatch(typeof(scnEditor), nameof(RotateSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            if(list[i].operand is MethodInfo { Name: "UpdateTileSelection" }) {
                list.InsertRange(i - 1, [
                    new CodeInstruction(OpCodes.Call, ((Delegate) RunRotateSelectionPatch).Method),
                    new CodeInstruction(OpCodes.Ldarg_1)
                ]);
                break;
            }
        }
        return list;
    }

    public static void RunRotateSelectionPatch(bool cw) {
        customRunner |= 4;
        try {
            Patches.RotateSelectionPatch.Postfix(cw);
        } finally {
            customRunner &= ~4;
        }
    }

    [JAPatch(typeof(Patches.RotateFloorPatch), "Postfix", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloorPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator) =>
        Setup(instructions, new CodeInstruction(OpCodes.Ldc_I4_8));

    [JAPatch(typeof(scnEditor), nameof(RotateFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            if(list[i].operand is MethodInfo { Name: "UpdateTile" }) {
                list.InsertRange(i - 6, [
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Call, ((Delegate) RunRotatePatch).Method)
                ]);
                break;
            }
        }
        return list;
    }

    public static void RunRotatePatch(scrFloor floor, bool cw, bool remakePath) {
        customRunner |= 8;
        try {
            Patches.RotateFloorPatch.Postfix(floor, cw, remakePath);
        } finally {
            customRunner &= ~8;
        }
    }

    [JAPatch(typeof(Patches.RotateFloor180Patch), "Postfix", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloor180Patch(IEnumerable<CodeInstruction> instructions, ILGenerator generator) =>
        Setup(instructions, new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte) 16));

    [JAPatch(typeof(scnEditor), nameof(RotateFloor180), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloor180(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            if(list[i].operand is MethodInfo { Name: "UpdateTile" }) {
                list.InsertRange(i - 6, [
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Call, ((Delegate) RunRotate180Patch).Method)
                ]);
                break;
            }
        }
        return list;
    }

    public static void RunRotate180Patch(scrFloor floor, bool remakePath) {
        customRunner |= 16;
        try {
            Patches.RotateFloor180Patch.Postfix(floor, remakePath);
        } finally {
            customRunner &= ~16;
        }
    }
}