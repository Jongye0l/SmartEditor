using JALib.Core;
using JALib.Tools;
using SmartEditor.FixLoad;
using SmartEditor.Rotate;

namespace SmartEditor;

public class Main : JAMod {
    public static Main Instance;
    public static SettingGUI settingGUI;

    protected override void OnSetup() {
        AddFeature(new FixChartLoad(), new BGAMod(), new SpeedPauseConverter(), new BpmBeatCalculator(), new RotateScreen());
        settingGUI = new SettingGUI(this);
    }

    protected override void OnEnable() {
    }

    protected override void OnDisable() {
    }
}