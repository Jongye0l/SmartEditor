using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using HarmonyLib;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace SmartEditor;

public class BGAMod() : Feature(Main.Instance, nameof(BGAMod), patchClass: typeof(BGAMod), settingType: typeof(JASetting)) {

    protected override void OnEnable() {
        if(!Setting.Get<bool>("AutoEnable", out _)) {
            Setting.Set("AutoEnable", true);
            Main.Instance.SaveSetting();
            Enabled = false;
        }
        if(scrController.instance && !scrController.instance.paused) Play();
    }

    [JAPatch(typeof(scnEditor), nameof(Play), PatchType.Postfix, false)]
    public static void Play() {
        foreach(scrFloor floor in scnEditor.instance.floors) floor.startScale = floor.transform.localScale = Vector3.zero;
        foreach(scrPlanet planet in scrController.instance.planetarySystem.allPlanets) {
            for(int i = 0; i < planet.transform.childCount; i++)
                planet.transform.GetChild(i).gameObject.SetActive(false);
            planet.gameObject.GetComponent<PlanetRenderer>().sprite.visible = false;
        }
        scrConductor.instance.hitSoundVolume = 0;
        scrController.instance.errorMeter.gameObject.SetActive(false);
        if(scnEditor.instance) {
            scnEditor.instance.autoImage.enabled = false;
            scnEditor.instance.buttonAuto.enabled = false;
            if(scnEditor.instance.editorDifficultySelector.gameObject.activeSelf) scnEditor.instance.editorDifficultySelector.gameObject.SetActive(false);
            if(scnEditor.instance.buttonNoFail.gameObject.activeSelf) scnEditor.instance.buttonNoFail.gameObject.SetActive(false);
        } else {
            scrUIController.instance.noFailImage.enabled = false;
            scrUIController.instance.difficultyImage.enabled = false;
            if(scrUIController.instance.difficultyContainer.gameObject.activeSelf) scrUIController.instance.difficultyContainer.gameObject.SetActive(false);
            if(scrUIController.instance.difficultyFadeContainer.gameObject.activeSelf) scrUIController.instance.difficultyFadeContainer.gameObject.SetActive(false);
        }
    }

    [JAPatch(typeof(scnEditor), nameof(SwitchToEditMode), PatchType.Postfix, false)]
    public static void SwitchToEditMode() {
        scnEditor.instance.autoImage.enabled = true;
        scnEditor.instance.buttonAuto.enabled = true;
    }

    [JAPatch(typeof(scnGame), nameof(ApplyEvent), PatchType.Postfix, false)]
    public static void ApplyEvent(LevelEvent evnt, List<scrFloor> floors, int? customFloorID, ffxPlusBase __result) {
        int num1 = customFloorID ?? evnt.floor;
        scrFloor floor = floors[num1];
        if(__result is ffxMoveFloorPlus ffxMoveFloorPlus) ffxMoveFloorPlus.scaleUsed = false;
        if(evnt.eventType == LevelEventType.SetHitsound) floor.setHitsound = null;
    }

    [JAPatch(typeof(scrController), nameof(ShowHitText), PatchType.Prefix, false)]
    public static bool ShowHitText() => false;

    [JAPatch(typeof(scrPlanet), nameof(MarkFail), PatchType.Replace, false)]
    [JAPatch(typeof(scrPlanet), "MarkMiss", PatchType.Replace, false)]
    public static object MarkFail() => null;

    [JAPatch(typeof(scrController), nameof(FailAction), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> FailAction(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> instructionList = instructions.ToList();
        // ---- remove code C# ----
        // foreach (scrMissIndicator scrMissIndicator in this.missesOnCurrFloor)
        //     scrMissIndicator.StartBlinking();
        // ---- remove code IL ----
        // IL_01c9: ldarg.0      // this
        // IL_01ca: ldfld        class [mscorlib]System.Collections.Generic.List`1<class scrMissIndicator> scrController::missesOnCurrFloor
        // IL_01cf: callvirt     instance valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<!0/*class scrMissIndicator*/> class [mscorlib]System.Collections.Generic.List`1<class scrMissIndicator>::GetEnumerator()
        // IL_01d4: stloc.s      V_5
        // .try
        // {
        //   IL_01d6: br.s         IL_01e4
        //   // start of loop, entry point: IL_01e4
        //     IL_01d8: ldloca.s     V_5
        //     IL_01da: call         instance !0/*class scrMissIndicator*/ valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class scrMissIndicator>::get_Current()
        //     IL_01df: callvirt     instance void scrMissIndicator::StartBlinking()
        //     IL_01e4: ldloca.s     V_5
        //     IL_01e6: call         instance bool valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class scrMissIndicator>::MoveNext()
        //     IL_01eb: brtrue.s     IL_01d8
        //   // end of loop
        //   IL_01ed: leave.s      IL_01fd
        // } // end of .try
        // finally
        // {
        //   IL_01ef: ldloca.s     V_5
        //   IL_01f1: constrained. valuetype [mscorlib]System.Collections.Generic.List`1/Enumerator<class scrMissIndicator>
        //   IL_01f7: callvirt     instance void [mscorlib]System.IDisposable::Dispose()
        //   IL_01fc: endfinally
        // } // end of finally
        for(int i = 0; i < instructionList.Count; i++) {
            if(instructionList[i].operand is not FieldInfo { Name: "missesOnCurrFloor" }) continue;
            instructionList[i + 16].labels = instructionList[i - 1].labels;
            instructionList.RemoveRange(i - 1, 16);
            break;
        }
        return instructionList;
    }

    [JAPatch(typeof(scrShowIfDebug), nameof(Update), PatchType.Replace, false)]
    public static void Update(Text ___txt) => ___txt.enabled = false;

    [JAPatch(typeof(scrController), nameof(OnLandOnPortal), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> OnLandOnPortal(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> instructionList = instructions.ToList();
        for(int i = 0; i < instructionList.Count; i++) {
            // ---- remove code C# ----
            // scrFlash.Flash(new Color?(Color.white.WithAlpha(0.4f)));
            // ---- remove code IL ----
            // IL_001c: call         valuetype [UnityEngine.CoreModule]UnityEngine.Color [UnityEngine.CoreModule]UnityEngine.Color::get_white()
            // IL_0021: ldc.r4       0.4
            // IL_0026: call         valuetype [UnityEngine.CoreModule]UnityEngine.Color ExtensionMethods::WithAlpha(valuetype [UnityEngine.CoreModule]UnityEngine.Color, float32)
            // IL_002b: newobj       instance void valuetype [mscorlib]System.Nullable`1<valuetype [UnityEngine.CoreModule]UnityEngine.Color>::.ctor(!0/*valuetype [UnityEngine.CoreModule]UnityEngine.Color*/)
            // IL_0030: ldc.r4       -1
            // IL_0035: call         void scrFlash::Flash(valuetype [mscorlib]System.Nullable`1<valuetype [UnityEngine.CoreModule]UnityEngine.Color>, float32)
            if(instructionList[i].operand is not MethodInfo { Name: "Flash" }) continue;
            instructionList.RemoveRange(i - 5, 6);
            break;
        }
        return instructionList;
    }

    [JAPatch(typeof(scrController), nameof(OnLandOnPortal), PatchType.Postfix, false)]
    public static void OnLandOnPortal(scrController __instance) {
        __instance.txtCongrats.gameObject.SetActive(false);
        __instance.txtResults.gameObject.SetActive(false);
        __instance.txtAllStrictClear.gameObject.SetActive(false);
    }

    [JAPatch(typeof(scrController), "paused.set", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> SetPaused(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> instructionList = instructions.ToList();
        for(int i = 0; i < instructionList.Count; i++) {
            // ---- remove code C# ----
            // if ((bool) (UnityEngine.Object) this.errorMeter && this.gameworld && Persistence.hitErrorMeterSize != ErrorMeterSize.Off)
            //     this.errorMeter.gameObject.SetActive(!value);
            // ---- remove code IL ----
            // IL_0012: ldarg.0      // this
            // IL_0013: ldfld        class scrHitErrorMeter scrController::errorMeter
            // IL_0018: call         bool [UnityEngine.CoreModule]UnityEngine.Object::op_Implicit(class [UnityEngine.CoreModule]UnityEngine.Object)
            // IL_001d: brfalse.s    IL_0042
            //
            // IL_001f: ldarg.0      // this
            // IL_0020: ldfld        bool scrController::gameworld
            // IL_0025: brfalse.s    IL_0042
            // IL_0027: call         valuetype ErrorMeterSize Persistence::get_hitErrorMeterSize()
            // IL_002c: brfalse.s    IL_0042
            //
            // // [304 9 - 304 53]
            // IL_002e: ldarg.0      // this
            // IL_002f: ldfld        class scrHitErrorMeter scrController::errorMeter
            // IL_0034: callvirt     instance class [UnityEngine.CoreModule]UnityEngine.GameObject [UnityEngine.CoreModule]UnityEngine.Component::get_gameObject()
            // IL_0039: ldarg.1      // 'value'
            // IL_003a: ldc.i4.0
            // IL_003b: ceq
            // IL_003d: callvirt     instance void [UnityEngine.CoreModule]UnityEngine.GameObject::SetActive(bool)
            if(instructionList[i].operand is not FieldInfo { Name: "errorMeter" }) continue;
            instructionList.RemoveRange(i - 1, 16);
        }
        return instructionList;
    }

    [JAPatch(typeof(scrPlanet), nameof(MoveToNextFloor), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> MoveToNextFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> instructionList = instructions.ToList();
        // ---- remove code C# ----
        // if ((bool) (UnityEngine.Object) this.controller.errorMeter)
        //     this.controller.errorMeter.wrapperRectTransform.gameObject.SetActive(false);
        // ---- remove code IL ----
        // IL_027a: ldarg.0      // this
        // IL_027b: call         instance class scrController scrPlanet::get_controller()
        // IL_0280: ldfld        class scrHitErrorMeter scrController::errorMeter
        // IL_0285: call         bool [UnityEngine.CoreModule]UnityEngine.Object::op_Implicit(class [UnityEngine.CoreModule]UnityEngine.Object)
        // IL_028a: brfalse      IL_0396
        //
        // // [789 11 - 789 86]
        // IL_028f: ldarg.0      // this
        // IL_0290: call         instance class scrController scrPlanet::get_controller()
        // IL_0295: ldfld        class scrHitErrorMeter scrController::errorMeter
        // IL_029a: ldfld        class [UnityEngine.CoreModule]UnityEngine.RectTransform scrHitErrorMeter::wrapperRectTransform
        // IL_029f: callvirt     instance class [UnityEngine.CoreModule]UnityEngine.GameObject [UnityEngine.CoreModule]UnityEngine.Component::get_gameObject()
        // IL_02a4: ldc.i4.0
        // IL_02a5: callvirt     instance void [UnityEngine.CoreModule]UnityEngine.GameObject::SetActive(bool)
        for(int i = 0; i < instructionList.Count; i++) {
            if(instructionList[i].operand is not FieldInfo { Name: "errorMeter" }) continue;
            instructionList.RemoveRange(i - 2, 12);
        }
        return instructionList;
    }

    [JAPatch(typeof(TaroCutsceneScript), nameof(DisplayText), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> DisplayText(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> instructionList = instructions.ToList();
        // ---- remove code C# ----
        // if ((bool) (UnityEngine.Object) ADOBase.controller.errorMeter)
        //     ADOBase.controller.errorMeter.wrapperRectTransform.gameObject.SetActive(false);
        // ---- remove code IL ----
        // IL_016a: call         class scrController ADOBase::get_controller()
        // IL_016f: ldfld        class scrHitErrorMeter scrController::errorMeter
        // IL_0174: call         bool [UnityEngine.CoreModule]UnityEngine.Object::op_Implicit(class [UnityEngine.CoreModule]UnityEngine.Object)
        // IL_0179: brfalse      IL_0275
        //
        // // [298 11 - 298 89]
        // IL_017e: call         class scrController ADOBase::get_controller()
        // IL_0183: ldfld        class scrHitErrorMeter scrController::errorMeter
        // IL_0188: ldfld        class [UnityEngine.CoreModule]UnityEngine.RectTransform scrHitErrorMeter::wrapperRectTransform
        // IL_018d: callvirt     instance class [UnityEngine.CoreModule]UnityEngine.GameObject [UnityEngine.CoreModule]UnityEngine.Component::get_gameObject()
        // IL_0192: ldc.i4.0
        // IL_0193: callvirt     instance void [UnityEngine.CoreModule]UnityEngine.GameObject::SetActive(bool)
        for(int i = 0; i < instructionList.Count; i++) {
            if(instructionList[i].operand is not FieldInfo { Name: "errorMeter" }) continue;
            instructionList.RemoveRange(i - 2, 10);
        }
        return instructionList;
    }
    
    [JAPatch(typeof(ffxChangeTrack), nameof(PrepFloor), PatchType.Prefix, false)]
    public static bool PrepFloor() => false;
}
