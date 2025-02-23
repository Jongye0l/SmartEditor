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
            if(instructionList[i].operand is not MethodInfo { Name: "SetActive" }) continue;
            instructionList.RemoveRange(i - 2, 3);
            instructionList[i - 3] = new CodeInstruction(OpCodes.Pop);
        }
        return instructionList;
    }

    [JAPatch(typeof(scrPlanet), nameof(MoveToNextFloor), PatchType.Transpiler, false)]
    [JAPatch(typeof(TaroCutsceneScript), "DisplayText", PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> MoveToNextFloor(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> instructionList = instructions.ToList();
        for(int i = 0; i < instructionList.Count; i++) {
            if(instructionList[i].operand is not MethodInfo { Name: "SetActive" }) continue;
            instructionList.RemoveAt(i);
            instructionList[i - 1] = new CodeInstruction(OpCodes.Pop);
        }
        return instructionList;
    }
}
