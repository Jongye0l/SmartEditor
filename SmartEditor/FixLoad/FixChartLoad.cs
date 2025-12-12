using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using ADOFAI.Editor.Actions;
using HarmonyLib;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using SmartEditor.FixLoad.CustomSaveState;
using SmartEditor.FixLoad.CustomSaveState.Scope;
using UnityEngine;
using UnityModManagerNet;

namespace SmartEditor.FixLoad;

public class FixChartLoad : Feature {
    public static FixChartLoad instance;
    public static JAPatcher patcher;

    public FixChartLoad() : base(Main.Instance, nameof(FixChartLoad), true, typeof(FixChartLoad)) {
        instance = this;
        patcher = Patcher;
        SaveStatePatch.Patch(patcher);
        ScopePatch.Patch(patcher);
        FlipAndRotateTilesAPI.CheckMod();
    }

    protected override void OnGUI() {
        if(!scnEditor.instance) GUILayout.Label("Open the editor to use this feature.");
        else if(GUILayout.Button("Reload Chart")) scnEditor.instance.RemakePath();
    }

    [JAPatch(typeof(scnEditor), nameof(CreateFloor), PatchType.Transpiler, false, ArgumentTypesType = [typeof(float), typeof(bool), typeof(bool)])]
    internal static IEnumerable<CodeInstruction> CreateFloor(IEnumerable<CodeInstruction> instructions) {
        // ---- Code Patches ----
        //         }
        //     }
        // (-) this.UpdateDecorationObjects();
        // 
        // 없어도 되는 코드 지우기
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
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // InsertTileUpdate.UpdateTile(sequenceID)
            // ---- original code IL ----
            // IL_0012: ldarg.0      // this
            // IL_0013: ldc.i4.1
            // IL_0014: ldc.i4.1
            // IL_0015: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_0012: ldarg.1      // sequenceID
            // IL_0013: call         InsertTileUpdate::UpdateTile(int)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1);
                codes[i - 2] = new CodeInstruction(OpCodes.Call, ((Delegate) InsertTileUpdate.UpdateTile).Method);
                codes.RemoveRange(i - 1, 2);
            }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(DeleteFloor), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> DeleteFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++)
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // DeleteTileUpdate.UpdateTile(sequenceID, 1)
            // ---- original code IL ----
            // IL_0280: ldarg.0      // this
            // IL_0281: ldc.i4.1
            // IL_0282: ldc.i4.1
            // IL_0283: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_0280: ldarg.1      // sequenceID
            // IL_0281: ldc.i4.1
            // IL_0282: call         DeleteTileUpdate::UpdateTile(int32, int32)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1);
                codes[i - 2] = new CodeInstruction(OpCodes.Ldc_I4_1);
                codes[i - 1] = new CodeInstruction(OpCodes.Call, ((Delegate) DeleteTileUpdate.UpdateTile).Method);
                codes.RemoveAt(i);
            }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(DeleteMultiSelection), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> DeleteMultiSelection(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> codes = instructions.ToList();
        LocalBuilder local = generator.DeclareLocal(typeof(int));
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];
            // ---- original code C# ----
            // this.DeselectAllFloors();
            // ---- replaced code C# ----
            // local = this.selectedFloors.Count;
            // ---- original code IL ----
            // IL_004a: ldarg.0      // this
            // IL_004b: call         instance void scnEditor::DeselectAllFloors()
            // ---- replaced code IL ----
            // IL_004a: ldarg.0
            // IL_004b: ldfld        instance System.Collections.Generic.List`1<scrFloor> scnEditor::selectedFloors
            // IL_0051: callvirt     instance int32 System.Collections.Generic.List`1<scrFloor>::get_Count()
            // IL_0056: stloc        local(patch)
            if(code.operand is MethodInfo { Name: "DeselectAllFloors" }) {
                codes.InsertRange(i - 1, [
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(typeof(scnEditor), "selectedFloors")),
                    new CodeInstruction(OpCodes.Callvirt, typeof(List<scrFloor>).Method("get_Count")),
                    new CodeInstruction(OpCodes.Stloc, local)
                ]);
                i += 4;
            }
            // ---- original code C# ----
            // this.DeleteFloor(seqId1);
            // ---- replaced code C# ----
            // this.DeleteFloor(seqId1, false);
            // DeleteTileUpdate.UpdateTile(seqId1, local);
            // ---- original code IL ----
            // IL_006a: ldarg.0      // this
            // IL_006b: ldloc.1      // seqId1
            // IL_006c: ldc.i4.1
            // IL_006d: call         instance bool scnEditor::DeleteFloor(int32, bool)
            // IL_0072: pop
            // ---- replaced code IL ----
            // IL_006a: ldarg.0      // this
            // IL_006b: ldloc.1      // seqId1
            // IL_006c: ldc.i4.0
            // IL_006d: call         instance bool scnEditor::DeleteFloor(int32, bool)
            // IL_0072: pop
            // IL_0073: ldloc.1      // seqId1
            // IL_0074: ldloc        local(patch)
            // IL_0079: call         DeleteTileUpdate::UpdateTile(int32, int32)
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

    [JAPatch(typeof(scnEditor), nameof(DeleteSubsequentFloors), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> DeleteSubsequentFloors(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.DeleteFloor(seqId + 1);
            // ---- replaced code C# ----
            // this.DeleteFloor(seqId + 1, false);
            // DeleteTileUpdate.UpdateTile(seqId + 1);
            // ---- original code IL ----
            // IL_004d: ldarg.0      // this
            // IL_004e: ldloc.1      // seqId
            // IL_004f: ldc.i4.1
            // IL_0050: add
            // IL_0051: ldc.i4.1
            // IL_0052: call         instance bool scnEditor::DeleteFloor(int32, bool)
            // IL_0057: pop
            // ---- replaced code IL ----
            // IL_004d: ldarg.0      // this
            // IL_004e: ldloc.1      // seqId
            // IL_004f: ldc.i4.1
            // IL_0050: add
            // IL_0051: ldc.i4.0
            // IL_0052: call         instance bool scnEditor::DeleteFloor(int32, bool)
            // IL_0057: pop
            // IL_0058: ldloc.1      // seqId
            // IL_0059: call         DeleteTileUpdate::UpdateTileSubsequent(int32)
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


    [JAPatch(typeof(scnEditor), nameof(DeletePrecedingFloors), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> DeletePrecedingFloors(IEnumerable<CodeInstruction> instructions) {
        // ---- original code C# ----
        // this.DeleteFloor(1);
        // ---- replaced code C# ----
        // this.DeleteFloor(1, false);
        // DeleteTileUpdate.UpdateTilePreceding();
        // ---- original code IL ----
        // IL_0040: ldarg.0      // this
        // IL_0041: ldc.i4.1
        // IL_0042: ldc.i4.1
        // IL_0043: call         instance bool scnEditor::DeleteFloor(int32, bool)
        // IL_0048: pop
        // ---- replaced code IL ----
        // IL_0040: ldarg.0      // this
        // IL_0041: ldc.i4.1
        // IL_0042: ldc.i4.0
        // IL_0043: call         instance bool scnEditor::DeleteFloor(int32, bool)
        // IL_0048: pop
        // IL_0049: call         DeleteTileUpdate::UpdateTilePreceding()
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
        editor.Invoke("DrawMultiPlanet");
    }

    [JAPatch(typeof(FixChartLoad), nameof(DrawEditor), PatchType.Transpiler, false)]
    private static IEnumerable<CodeInstruction> DrawEditorTranspiler(IEnumerable<CodeInstruction> instructions) {
        return [
            new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(scnEditor), "instance")),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(scnEditor), "DrawFloorOffsetLines")),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(scnEditor), "DrawMultiPlanet")),
            new CodeInstruction(OpCodes.Ret)
        ];
    }

    [JAPatch(typeof(ToggleFloorNumsEditorAction), "Execute", PatchType.Transpiler, false)]
    private static IEnumerable<CodeInstruction> ToggleFloorNumsEditorActionTranspiler(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // this.DrawFloorNums();
            // ---- original code IL ----
            // IL_002c: ldarg.1      // editor
            // IL_002d: ldc.i4.1
            // IL_002e: ldc.i4.1
            // IL_002f: callvirt     instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_002c: ldarg.1      // editor
            // IL_002d: callvirt     instance void scnEditor::DrawFloorNums()
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i].operand = typeof(scnEditor).Method("DrawFloorNums");
                codes.RemoveRange(i - 2, 2);
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(FlipFloor), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> FlipFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // FlipTileUpdate.UpdateTile(floor.seqID, 1, horizontal);
            // ---- original code IL ----
            // IL_00d9: ldarg.0      // this
            // IL_00da: ldc.i4.1
            // IL_00db: ldc.i4.1
            // IL_00dc: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_00d9: ldarg.1      // floor
            // IL_00da: ldfld        int32 scrFloor::seqID
            // IL_00df: ldc.i4.1
            // IL_00e0: ldarg.2      // horizontal
            // IL_00e1: call         FlipTileUpdate::UpdateTile(int32, int32, bool)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1);
                codes[i - 2] = new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(typeof(scrFloor), "seqID"));
                codes[i - 1] = new CodeInstruction(OpCodes.Ldc_I4_1);
                codes[i++] = new CodeInstruction(OpCodes.Ldarg_2);
                codes.Insert(i, new CodeInstruction(OpCodes.Call, ((Delegate) FlipTileUpdate.UpdateTile).Method));
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(FlipSelection), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> FlipSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // FlipTileUpdate.UpdateTileSelection(horizontal);
            // ---- original code IL ----
            // IL_0078: ldarg.0      // this
            // IL_0079: ldc.i4.1
            // IL_007a: ldc.i4.1
            // IL_007b: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_0078: ldarg.1      // horizontal
            // IL_0079: call         FlipTileUpdate::UpdateTileSelection(bool)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1) { labels = codes[i - 3].labels };
                codes[i - 2] = new CodeInstruction(OpCodes.Call, ((Delegate) FlipTileUpdate.UpdateTileSelection).Method);
                codes.RemoveRange(i - 1, 2);
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(PasteFloors), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> PasteFloors(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // PasteTileUpdate.UpdateTile();
            // ---- original code IL ----
            // IL_01f5: ldarg.0      // this
            // IL_01f6: ldc.i4.1
            // IL_01f7: ldc.i4.1
            // IL_01f8: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_01f5: call         PasteTileUpdate::UpdateTile()
            if(code.operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Call, ((Delegate) PasteTileUpdate.UpdateTile).Method);
                codes.RemoveRange(i - 2, 3);
            // ---- original code C# ----
            // this.events.Add(this.CopyEvent(levelEvent, seqId));
            // ---- replaced code C# ----
            // LevelEvent local = this.CopyEvent(levelEvent, seqId);
            // this.events.Add(local);
            // SaveStatePatch.Add(local);
            // ---- original code IL ----
            // IL_013f: ldarg.0      // this
            // IL_0140: call         instance class EventsArray`1<class ADOFAI.LevelEvent> scnEditor::get_events()
            // IL_0145: ldarg.0      // this
            // IL_0146: ldloc.s      levelEvent
            // IL_0148: ldloc.1      // seqId
            // IL_0149: call         instance class ADOFAI.LevelEvent scnEditor::CopyEvent(class ADOFAI.LevelEvent, int32)
            // IL_014e: callvirt     instance void class EventsArray`1<class ADOFAI.LevelEvent>::Add(!0/*class ADOFAI.LevelEvent*/)
            // ---- replaced code IL ----
            // IL_013f: ldarg.0      // this
            // IL_0140: call         instance class EventsArray`1<class ADOFAI.LevelEvent> scnEditor::get_events()
            // IL_0145: ldarg.0      // this
            // IL_0146: ldloc.s      levelEvent
            // IL_0148: ldloc.1      // seqId
            // IL_0149: call         instance class ADOFAI.LevelEvent scnEditor::CopyEvent(class ADOFAI.LevelEvent, int32)
            // IL_014e: stloc        local(patch)
            // IL_0153: ldloc        local(patch)
            // IL_0158: callvirt     instance void class EventsArray`1<class ADOFAI.LevelEvent>::Add(!0/*class ADOFAI.LevelEvent*/)
            // IL_015d: ldloc        local(patch)
            // IL_0162: call         SaveStatePatch::Add(class ADOFAI.LevelEvent)
            } else if(code.opcode == OpCodes.Callvirt && typeof(EventsArray<LevelEvent>).Method(nameof(SaveStatePatch.Add)) == (MethodInfo) code.operand) {
                LocalBuilder local = generator.DeclareLocal(typeof(LevelEvent));
                codes[i] = new CodeInstruction(OpCodes.Stloc, local) { labels = code.labels };
                codes.InsertRange(i + 1, [
                    new CodeInstruction(OpCodes.Ldloc, local),
                    new CodeInstruction(OpCodes.Callvirt, typeof(List<LevelEvent>).Method(nameof(SaveStatePatch.Add))),
                    new CodeInstruction(OpCodes.Ldloc, local),
                    new CodeInstruction(OpCodes.Call, ((Action<LevelEvent>) SaveStatePatch.Add).Method)
                ]);
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(RemoveEventAtSelected), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> RemoveEventAtSelected(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // FixChartLoad.RemoveEventAtSelectedUpdate();
            // ---- original code IL ----
            // IL_00eb: ldarg.0      // this
            // IL_00ec: ldc.i4.1
            // IL_00ed: ldc.i4.1
            // IL_00ee: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_00eb: call         FixChartLoad::RemoveEventAtSelectedUpdate()
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Call, ((Delegate) RemoveEventAtSelectedUpdate).Method);
                codes.RemoveRange(i - 2, 3);
            }
        }
        return codes;
    }

    private static void RemoveEventAtSelectedUpdate() {
        try {
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            scnGame.instance.ApplyEventsToFloors(levelMaker.listFloors);
            levelMaker.DrawHolds();
            DrawEditor();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    [JAPatch(typeof(scnEditor), nameof(RotateFloor), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> RotateFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // RotateTileUpdate.UpdateTile(floor.seqID, 1, CW, false);
            // ---- original code IL ----
            // IL_00ce: ldarg.0      // this
            // IL_00cf: ldc.i4.1
            // IL_00d0: ldc.i4.1
            // IL_00d1: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_00ce: ldarg.1      // floor
            // IL_00cf: ldfld        int32 scrFloor::seqID
            // IL_00d4: ldc.i4.1
            // IL_00d5: ldarg.2      // CW
            // IL_00d6: ldc.i4.0
            // IL_00d7: call         RotateTileUpdate::UpdateTile(int32, int32, bool, bool)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1);
                codes[i - 2] = new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(typeof(scrFloor), "seqID"));
                codes[i - 1] = new CodeInstruction(OpCodes.Ldc_I4_1);
                codes[i++] = new CodeInstruction(OpCodes.Ldarg_2);
                codes.InsertRange(i, [
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Call, ((Delegate) RotateTileUpdate.UpdateTile).Method)
                ]);
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(RotateSelection), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> RotateSelection(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // RotateTileUpdate.UpdateTileSelection(CW, false);
            // ---- original code IL ----
            // IL_0078: ldarg.0      // this
            // IL_0079: ldc.i4.1
            // IL_007a: ldc.i4.1
            // IL_007b: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_0078: ldarg.1      // CW
            // IL_0079: ldc.i4.0
            // IL_007a: call         RotateTileUpdate::UpdateTileSelection(bool, bool)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1) { labels = codes[i - 3].labels };
                codes[i - 2] = new CodeInstruction(OpCodes.Ldc_I4_0);
                codes[i - 1] = new CodeInstruction(OpCodes.Call, ((Delegate) RotateTileUpdate.UpdateTileSelection).Method);
                codes.RemoveAt(i);
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(RotateFloor180), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> RotateFloor180(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // RotateTileUpdate.UpdateTile(floor.seqID, 1, false, true);
            // ---- original code IL ----
            // IL_00db: ldarg.0      // this
            // IL_00dc: ldc.i4.1
            // IL_00dd: ldc.i4.1
            // IL_00de: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_00db: ldarg.1      // floor
            // IL_00dc: ldfld        int32 scrFloor::seqID
            // IL_00e1: ldc.i4.1
            // IL_00e2: ldc.i4.0
            // IL_00e3: ldc.i4.1
            // IL_00e4: call         RotateTileUpdate::UpdateTile(int32, int32, bool, bool)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldarg_1);
                codes[i - 2] = new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(typeof(scrFloor), "seqID"));
                codes[i - 1] = new CodeInstruction(OpCodes.Ldc_I4_1);
                codes[i++] = new CodeInstruction(OpCodes.Ldc_I4_0);
                codes.InsertRange(i, [
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Call, ((Delegate) RotateTileUpdate.UpdateTile).Method)
                ]);
            }
        }
        return codes;
    }

    [JAPatch(typeof(scnEditor), nameof(RotateSelection180), PatchType.Transpiler, false)]
    internal static IEnumerable<CodeInstruction> RotateSelection180(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = instructions.ToList();
        for(int i = 0; i < codes.Count; i++) {
            // ---- original code C# ----
            // this.RemakePath();
            // ---- replaced code C# ----
            // RotateTileUpdate.UpdateTileSelection(false, true);
            // ---- original code IL ----
            // IL_0077: ldarg.0      // this
            // IL_0078: ldc.i4.1
            // IL_0079: ldc.i4.1
            // IL_007a: call         instance void scnEditor::RemakePath(bool, bool)
            // ---- replaced code IL ----
            // IL_0077: ldc.i4.0
            // IL_0078: ldc.i4.1
            // IL_0079: call         RotateTileUpdate::UpdateTileSelection(bool, bool)
            if(codes[i].operand is MethodInfo { Name: "RemakePath" }) {
                codes[i - 3] = new CodeInstruction(OpCodes.Ldc_I4_0) { labels = codes[i - 3].labels };
                codes[i - 2] = new CodeInstruction(OpCodes.Ldc_I4_1);
                codes[i - 1] = new CodeInstruction(OpCodes.Call, ((Delegate) RotateTileUpdate.UpdateTileSelection).Method);
                codes.RemoveAt(i);
            }
        }
        return codes;
    }

    [JAPatch(typeof(UnityModManager.ModEntry), "Load", PatchType.Postfix, false)]
    public static void ModLoadCheck() {
        FlipAndRotateTilesAPI.CheckMod();
    }

    private static MethodInfo GetRadiusMethod = SimpleReflect.Property(typeof(scrController), VersionControl.releaseNumber < 134 ? "startRadius" : "tileSize").GetGetMethod();

    public static double GetRadius() => GetRadiusMethod.Invoke<float>(scrController.instance);
}