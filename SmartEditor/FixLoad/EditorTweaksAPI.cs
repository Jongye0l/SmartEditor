using System;

namespace SmartEditor.FixLoad;

public static class EditorTweaksAPI {
    private static bool initialized;

    public static bool CheckMod() {
        try {
            bool enabled = CheckEnabled();
            if(!initialized) {
                Main.Instance.Log("EditorTweaks Mod Founded! Patching...");
                FixChartLoad.patcher.AddPatch(typeof(EditorTweaksPatch));
                initialized = true;
            }
            return enabled;
        } catch (Exception) {
            return false;
        }
    }

    private static bool CheckEnabled() => EditorTweaks.Main.Entry.Enabled;
}