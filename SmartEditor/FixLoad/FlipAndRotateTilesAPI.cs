using System;
using FlipAndRotateTiles;

namespace SmartEditor.FixLoad;

public class FlipAndRotateTilesAPI {
    private static bool initialized;

    public static bool CheckMod() {
        try {
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