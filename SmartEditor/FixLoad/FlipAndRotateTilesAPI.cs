using System;
using FlipAndRotateTiles;
using UnityModManagerNet;

namespace SmartEditor.FixLoad;

public static class FlipAndRotateTilesAPI {
    private static bool initialized;

    public static bool CheckMod() {
        try {
            if(!CheckEnabledUmm()) return false;
            bool enabled = CheckEnabled();
            if(!initialized) {
                Main.Instance.Log("FlipAndRotateTiles Mod Founded! Patching...");
                FixChartLoad.patcher.AddPatch(typeof(FlipAndRotateTilesPatch));
                initialized = true;
            }
            return enabled;
        } catch (Exception) {
            return false;
        }
    }

    private static bool CheckEnabledUmm() {
        foreach(UnityModManager.ModEntry modEntry in UnityModManager.modEntries) 
            if(modEntry.Info.Id == "FlipAndRotateTiles") return modEntry.Enabled;
        return false;
    }

    private static bool CheckEnabled() => FlipAndRotateTiles.Main.ModEntry.Enabled;

    public static bool AdjustOnFlip {
        get => FlipAndRotateTiles.Main.Settings.adjustOnFlip;
        set => FlipAndRotateTiles.Main.Settings.adjustOnFlip = value;
    }

    public static bool AdjustOnRotate {
        get => FlipAndRotateTiles.Main.Settings.adjustOnRotate;
        set => FlipAndRotateTiles.Main.Settings.adjustOnRotate = value;
    }

    public static float? CustomAngle {
        get {
            Settings settings = FlipAndRotateTiles.Main.Settings;
            return settings.customAngleRotation ? settings.customAngle : null;
        }
        set {
            Settings settings = FlipAndRotateTiles.Main.Settings;
            settings.customAngleRotation = value.HasValue;
            if(value.HasValue) settings.customAngle = value.Value;
        }
    }
}