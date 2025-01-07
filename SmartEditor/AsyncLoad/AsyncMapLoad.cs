using System.IO;
using System.Reflection;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using SmartEditor.AsyncLoad.Sequence;

namespace SmartEditor.AsyncLoad;

public class AsyncMapLoad : Feature {
    public static bool isLoading;
    public static MethodInfo pauseIfUnpaused = typeof(scnEditor).Method("PauseIfUnpaused");

    public AsyncMapLoad() : base(Main.Instance, nameof(AsyncMapLoad), true, typeof(AsyncMapLoad)) {
    }

    [JAPatch(typeof(scnEditor), nameof(LateUpdate), PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrFloor), "Update", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrDecorationManager), nameof(LateUpdate), PatchType.Prefix, false, TryingCatch = false)]
    public static bool LateUpdate() => !isLoading;

    [JAPatch(typeof(scrFloor), nameof(Start), PatchType.Prefix, false)]
    public static bool Start(ref bool ___didRunStart) {
        if(!isLoading) return true;
        ___didRunStart = true;
        return false;
    }

    [JAPatch(typeof(scnEditor), nameof(OpenLevel), PatchType.Replace, false, ArgumentTypesType = [])]
    public static void OpenLevel(scnEditor __instance) {
        __instance.CheckUnsavedChanges(OpenLevelContinue);
    }

    private static void OpenLevelContinue() {
        InitOpen(null);
        pauseIfUnpaused.Invoke(scnEditor.instance);
    }

    [JAPatch(typeof(scnEditor), nameof(OpenLevel), PatchType.Replace, false, ArgumentTypesType = [typeof(string)])]
    public static void OpenLevel(scnEditor __instance, string filePath) {
        InitOpen(filePath);
        pauseIfUnpaused.Invoke(__instance);
        __instance.ShowPopup(false);
    }

    [JAPatch(typeof(scnEditor), nameof(OpenRecent), PatchType.Replace, false)]
    public static void OpenRecent(scnEditor __instance, bool checkCtrl) {
        string recentLevel = Persistence.GetLastOpenedLevel();
        if(!File.Exists(recentLevel) || checkCtrl && __instance.Invoke<bool>("OpenDirectory", recentLevel)) return;
        __instance.CheckUnsavedChanges(OpenRecentContinue);
    }

    private static void OpenRecentContinue() => InitOpen(Persistence.GetLastOpenedLevel());

    private static void InitOpen(string path) {
        isLoading = true;
        LoadScreen.Show();
        _ = new LoadMap(path);
    }
}