using JALib.Core;
using SmartEditor.AsyncLoad;
using SmartEditor.FixLoad;

namespace SmartEditor;

public class Main : JAMod {
    public static Main Instance;

    protected override void OnSetup() {
        AddFeature(new FixChartLoad(), new BGAMod(), new SpeedPauseConverter(), new BpmBeatCalculator(), new AsyncMapLoad());
    }

    protected override void OnEnable() {
    }

    protected override void OnDisable() {
    }
}