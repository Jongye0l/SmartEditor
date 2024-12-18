using JALib.Core;
using SmartEditor.FixLoad;
using UnityModManagerNet;

namespace SmartEditor;

public class Main : JAMod {
    public static Main Instance;

    public Main(UnityModManager.ModEntry modEntry) : base(modEntry, false) {
        AddFeature(new FixChartLoad());
    }

    protected override void OnEnable() {
    }

    protected override void OnDisable() {
    }
}