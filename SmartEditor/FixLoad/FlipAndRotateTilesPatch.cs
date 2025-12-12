using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JALib.Core.Patch;
using JALib.Tools;
using Patches = FlipAndRotateTiles.Patches;

namespace SmartEditor.FixLoad;

public static class FlipAndRotateTilesPatch {
    public static IEnumerable<CodeInstruction> Setup(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction cur = list[i];
            // remove RemakePath all calls
            if(cur.operand is MethodInfo { Name: "RemakePath" }) {
                CodeInstruction prev = list[i - 3];
                list[i + 1].WithLabels(prev.labels).WithBlocks(prev.blocks);
                list.RemoveRange(i - 3, 4);
            }
        }
        return list;
    }

    [JAPatch(typeof(scnEditor), nameof(FlipSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            // ---- original code(vanilla) C# ----
            // this.RemakePath();
            // ---- original code(patched) C# ----
            // FlipTileUpdate.UpdateTileSelection(horizontal);
            // ---- replaced code C# ----
            // FlipAndRotateTilesPatch.RunFlipSelectionPatch(horizontal);
            // FlipTileUpdate.UpdateTileSelection(horizontal);
            // ---- original code(vanilla) code IL ----
            // IL_0078: ldarg.0      // this
            // IL_0079: ldc.i4.1
            // IL_007a: ldc.i4.1
            // IL_007b: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- original code(patched) code IL ----
            // ldarg.1      // horizontal
            // call         FlipTileUpdate::UpdateTileSelection(bool)
            // ---- replaced code IL ----
            // ldarg.1      // horizontal
            // call         FlipAndRotateTilesPatch::RunFlipSelectionPatch(bool)
            // ldarg.1      // horizontal
            // call         FlipTileUpdate::UpdateTileSelection(bool)
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

    [JAReversePatch(typeof(Patches.FlipSelectionPatch), "Postfix", ReversePatchType.AllCombine)]
    public static void RunFlipSelectionPatch(bool horizontal) {
        _ = Transpiler(null);
        throw new NotImplementedException();
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Setup(instructions);
    }

    [JAPatch(typeof(scnEditor), nameof(FlipFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FlipFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            // ---- original code(vanilla) C# ----
            // this.RemakePath();
            // ---- original code(patched) C# ----
            // FlipTileUpdate.UpdateTile(floor.seqID, 1, horizontal);
            // ---- replaced code C# ----
            // FlipAndRotateTilesPatch.RunFlipPatch(floor, horizontal, remakePatch);
            // FlipTileUpdate.UpdateTile(floor.seqID, 1, horizontal);
            // ---- original code(vanilla) code IL ----
            // IL_0078: ldarg.0      // this
            // IL_0079: ldc.i4.1
            // IL_007a: ldc.i4.1
            // IL_007b: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- original code(patched) code IL ----
            // ldarg.1      // floor
            // ldfld        int32 scrFloor::seqID
            // ldc.i4.1
            // ldarg.2      // horizontal
            // call         FlipTileUpdate::UpdateTile(int32, int32, bool)
            // ---- replaced code IL ----
            // ldarg.1      // floor
            // ldarg.2      // horizontal
            // ldarg.3      // remakePath
            // call         FlipAndRotateTilesPatch::RunFlipPatch(scrFloor, bool, bool)
            // ldarg.1      // floor
            // ldfld        int32 scrFloor::seqID
            // ldc.i4.1
            // ldarg.2      // horizontal
            // call         FlipTileUpdate::UpdateTile(int32, int32, bool)
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


    [JAReversePatch(typeof(Patches.FlipFloorPatch), "Postfix", ReversePatchType.AllCombine)]
    public static void RunFlipPatch(scrFloor floor, bool horizontal, bool remakePath) {
        _ = Transpiler(null);
        throw new NotImplementedException();
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Setup(instructions);
    }

    [JAPatch(typeof(scnEditor), nameof(RotateSelection), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            // ---- original code(vanilla) C# ----
            // this.RemakePath();
            // ---- original code(patched) C# ----
            // RotateTileUpdate.UpdateTileSelection(CW, false);
            // ---- replaced code C# ----
            // FlipAndRotateTilesPatch.RunRotateSelectionPatch(CW);
            // FlipTileUpdate.UpdateTileSelection(horizontal);
            // ---- original code(vanilla) code IL ----
            // IL_0078: ldarg.0      // this
            // IL_0079: ldc.i4.1
            // IL_007a: ldc.i4.1
            // IL_007b: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- original code(patched) code IL ----
            // ldarg.1      // CW
            // ldc.i4.0
            // call         RotateTileUpdate::UpdateTileSelection(bool, bool)
            // ---- replaced code IL ----
            // ldarg.1      // CW
            // call         FlipAndRotateTilesPatch::RunRotateSelectionPatch(bool)
            // ldarg.1      // CW
            // ldc.i4.0
            // call         RotateTileUpdate::UpdateTileSelection(bool, bool)
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

    [JAReversePatch(typeof(Patches.RotateSelectionPatch), "Postfix", ReversePatchType.AllCombine)]
    public static void RunRotateSelectionPatch(bool cw) {
        _ = Transpiler(null);
        throw new NotImplementedException();
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Setup(instructions);
    }

    [JAPatch(typeof(scnEditor), nameof(RotateFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            // ---- original code(vanilla) C# ----
            // this.RemakePath();
            // ---- original code(patched) C# ----
            // RotateTileUpdate.UpdateTile(floor.seqID, 1, CW, false);
            // ---- replaced code C# ----
            // FlipAndRotateTilesPatch.RunRotatePatch(floor, CW, remakePath);
            // RotateTileUpdate.UpdateTile(floor.seqID, 1, CW, false);
            // ---- original code(vanilla) code IL ----
            // IL_0078: ldarg.0      // this
            // IL_0079: ldc.i4.1
            // IL_007a: ldc.i4.1
            // IL_007b: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- original code(patched) code IL ----
            // ldarg.1      // floor
            // ldfld        int32 scrFloor::seqID
            // ldc.i4.1
            // ldarg.2      // CW
            // ldc.i4.0
            // call         RotateTileUpdate::UpdateTile(int32, int32, bool, bool)
            // ---- replaced code IL ----
            // ldarg.1      // floor
            // ldarg.2      // CW
            // ldarg.3      // remakePath
            // call         FlipAndRotateTilesPatch::RunRotatePatch(scrFloor, bool, bool)
            // ldarg.1      // floor
            // ldfld        int32 scrFloor::seqID
            // ldc.i4.1
            // ldarg.2      // CW
            // ldc.i4.0
            // call         RotateTileUpdate::UpdateTile(int32, int32, bool, bool)
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

    [JAReversePatch(typeof(Patches.RotateFloorPatch), "Postfix", ReversePatchType.AllCombine)]
    public static void RunRotatePatch(scrFloor floor, bool cw, bool remakePath) {
        _ = Transpiler(null);
        throw new NotImplementedException();
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Setup(instructions);
    }

    [JAPatch(typeof(scnEditor), nameof(RotateFloor180), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> RotateFloor180(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            // ---- original code(vanilla) C# ----
            // this.RemakePath();
            // ---- original code(patched) C# ----
            // RotateTileUpdate.UpdateTile(floor.seqID, 1, false, true);
            // ---- replaced code C# ----
            // FlipAndRotateTilesPatch.RunRotate180Patch(floor, remakePath);
            // RotateTileUpdate.UpdateTile(floor.seqID, 1, false, true);
            // ---- original code(vanilla) code IL ----
            // IL_0078: ldarg.0      // this
            // IL_0079: ldc.i4.1
            // IL_007a: ldc.i4.1
            // IL_007b: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- original code(patched) code IL ----
            // ldarg.1      // floor
            // ldfld        int32 scrFloor::seqID
            // ldc.i4.1
            // ldc.i4.0
            // ldc.i4.1
            // call         RotateTileUpdate::UpdateTile(int32, int32, bool, bool)
            // ---- replaced code IL ----
            // ldarg.1      // floor
            // ldarg.2      // remakePath
            // call         FlipAndRotateTilesPatch::RunRotate180Patch(scrFloor, bool)
            // ldarg.1      // floor
            // ldfld        int32 scrFloor::seqID
            // ldc.i4.1
            // ldc.i4.0
            // ldc.i4.1
            // call         RotateTileUpdate::UpdateTile(int32, int32, bool, bool)
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

    [JAReversePatch(typeof(Patches.RotateFloor180Patch), "Postfix", ReversePatchType.AllCombine)]
    public static void RunRotate180Patch(scrFloor floor, bool remakePath) {
        _ = Transpiler(null);
        throw new NotImplementedException();
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => Setup(instructions);
    }
    
    [JAPatch(typeof(Patches.FlipSelectionPatch), "Postfix", PatchType.Prefix, false)]
    [JAPatch(typeof(Patches.FlipFloorPatch), "Postfix", PatchType.Prefix, false)]
    [JAPatch(typeof(Patches.RotateSelectionPatch), "Postfix", PatchType.Prefix, false)]
    [JAPatch(typeof(Patches.RotateFloorPatch), "Postfix", PatchType.Prefix, false)]
    [JAPatch(typeof(Patches.RotateFloor180Patch), "Postfix", PatchType.Prefix, false)]
    public static bool PatchDisable() => false;
}