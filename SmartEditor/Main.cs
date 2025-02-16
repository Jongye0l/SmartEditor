using JALib.Core;
using SmartEditor.FixLoad;

namespace SmartEditor;

public class Main : JAMod {
    public static Main Instance;

    protected override void OnSetup() {
        AddFeature(new FixChartLoad(), new BGAMod());
    }

    protected override void OnEnable() {
    }

    protected override void OnDisable() {
    }
}