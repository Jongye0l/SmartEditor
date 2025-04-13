using System;
using ADOFAI;
using ADOFAI.Editor;
using ADOFAI.Editor.Actions;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using UnityEngine;

namespace SmartEditor.Rotate;

public class RotateScreen : Feature {
    public static RotateData data;
    public static RotateSettings settings;
    public static string angleText;
    public static KeyModifier[] keyModifiers = [
        KeyModifier.Shift,
        KeyModifier.Control,
        KeyModifier.Alt,
        KeyModifier.BackQuote
    ];
    public static byte changedKey;

    public RotateScreen() : base(Main.Instance, nameof(RotateScreen), true, typeof(RotateScreen), typeof(RotateSettings)) {
        settings = (RotateSettings) Setting;
    }

    protected override void OnEnable() {
        if(scnEditor.instance) Awake(scnEditor.instance);
    }

    protected override void OnDisable() {
        if(scnEditor.instance && scrController.instance.paused) scnEditor.instance.RemakePath();
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.settingGUI;
        JALocalization localization = Main.Instance.Localization;
        settingGUI.AddSettingFloat(ref settings.rotateAngle, 30, ref angleText, localization["RotateScreen.Angle"], 0, 360);
        GUILayout.Label(localization["RotateScreen.KeyModifier"]);
        foreach(KeyModifier modifier in keyModifiers) {
            bool value = settings.addedKey.HasFlag(modifier);
            bool temp = value;
            settingGUI.AddSettingToggle(ref value, modifier.ToString());
            if(temp != value) {
                if(value) settings.addedKey |= KeyModifier.Shift;
                else settings.addedKey &= ~KeyModifier.Shift;
            }
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label(localization["RotateScreen.MinusKey"]);
        if(GUILayout.Button(Bold(settings.minusKey.ToString(), changedKey == 1))) changedKey = (byte) (changedKey == 1 ? 0 : 1);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label(localization["RotateScreen.PlusKey"]);
        if(GUILayout.Button(Bold(settings.plusKey.ToString(), changedKey == 2))) changedKey = (byte) (changedKey == 2 ? 0 : 2);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(changedKey == 0 || !Input.anyKeyDown) return;
        foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
            if(!Input.GetKeyDown(keyCode)) continue;
            if(changedKey == 1) {
                settings.minusKey = keyCode;
                changedKey = 0;
            } else if(changedKey == 2) {
                settings.plusKey = keyCode;
                changedKey = 0;
            }
            Main.Instance.SaveSetting();
            break;
        }
    }

    private static string Bold(string text, bool bold) => bold ? $"<b>{text}</b>" : text;

    [JAPatch(typeof(scnEditor), nameof(Awake), PatchType.Postfix, false)]
    public static void Awake(scnEditor __instance) {
        data = __instance.GetOrAddComponent<RotateData>();
        data.angle = 0;
    }

    [JAPatch(typeof(scnEditor), nameof(SwitchToEditMode), PatchType.Postfix, false)]
    public static void SwitchToEditMode() {
        Vector3 vector3 = new(0, 0, -data.angle);
        scrCamera.instance.transform.eulerAngles = vector3;
        Transform transform = data.shortcutsContainer.parent;
        for(int i = 0; i < transform.childCount; i++) transform.GetChild(i).eulerAngles = vector3;
    }

    [JAPatch(typeof(CreateFloorWithCharOrAngleEditorAction), nameof(CreateFloorWithCharOrAngleEditorAction.Execute), PatchType.Prefix, false)]
    public static void CreateFloorPrefix(ref float ___angle) {
        ___angle = (___angle - data.angle + 360) % 360;
    }

    [JAPatch(typeof(CreateFloorWithCharOrAngleEditorAction), nameof(CreateFloorWithCharOrAngleEditorAction.Execute), PatchType.Postfix, false)]
    public static void CreateFloorPostfix(ref float ___angle) {
        ___angle = (___angle + data.angle) % 360;
    }

    [JAPatch(typeof(scrLevelMaker), nameof(GetAngleFromFloorCharDirectionWithCheck), PatchType.Postfix, false)]
    public static void GetAngleFromFloorCharDirectionWithCheck(bool exists, ref float __result) {
        if(exists) __result = (__result - data.angle + 360) % 360;
    }

    [JAPatch(typeof(FloorDirectionButton), nameof(Init), PatchType.Postfix, false)]
    public static void Init(FloorDirectionButton __instance) {
        Vector3 vector3 = __instance.text.transform.eulerAngles;
        vector3.z -= data.angle;
        __instance.text.transform.eulerAngles = vector3;
        __instance.textShifted.transform.eulerAngles = vector3;
        __instance.deleteIcon.transform.eulerAngles = vector3;
    }

    [JAPatch(typeof(scnEditor), nameof(UpdateDirectionButton), PatchType.Prefix, false)]
    public static void UpdateDirectionButton(ref float oppositeAngle) {
        oppositeAngle = (oppositeAngle + data.angle) % 360;
    }
}