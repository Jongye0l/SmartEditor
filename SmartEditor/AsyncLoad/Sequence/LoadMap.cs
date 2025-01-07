using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ADOFAI;
using JALib.Tools;
using SFB;
using SmartEditor.FixLoad.CustomSaveState;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence;

public class LoadMap : LoadSequence {
    private static readonly FieldInfo StallFileDialog = typeof(scnEditor).Field("stallFileDialog");
    public string path;
    public string lastLevelPath;
    public string customLevelId;

    public LoadMap(string path) {
        this.path = path;
        SequenceText = Main.Instance.Localization["AsyncMapLoad.InitAsync"];
        scnEditor.instance.CloseAllPanels();
        Task.Run(Reset);
        LoadScreen.OnRemove += OnRemove;
    }

    public void Reset() {
        try {
            scnEditor editor = scnEditor.instance;
            if(StallFileDialog.GetValue<bool>(editor)) {
                Task.Yield().GetAwaiter().OnCompleted(Reset);
                return;
            }
            SequenceText = Main.Instance.Localization["AsyncMapLoad.MapReset"];
            //editor.Invoke("ClearAllFloorOffsets"); // Is this necessary?
            editor.redoStates.Clear();
            editor.undoStates.Clear();
            SaveStatePatch.redoStates.Clear();
            SaveStatePatch.undoStates.Clear();
            scnGame game = editor.customLevel;
            lastLevelPath = game.levelPath;
            if(path == null) {
                SequenceText = Main.Instance.Localization["AsyncMapLoad.SelectFile"];
                string[] levelPaths = StandaloneFileBrowser.OpenFilePanel(RDString.Get("editor.dialog.openFile"), Persistence.GetLastUsedFolder(), [
                    new ExtensionFilter(RDString.Get("editor.dialog.adofaiLevelDescription"), GCS.levelExtensions)
                ], false);
                if(levelPaths.Length == 0 || string.IsNullOrEmpty(levelPaths[0])) goto StopLoading;
                string str1 = Uri.UnescapeDataString(levelPaths[0].Replace("file:", ""));
                string lower = Path.GetExtension(str1).ToLower();
                string str2 = lower.Substring(1, lower.Length - 1);
                if(GCS.levelZipExtensions.Contains(str2)) {
                    string availableDirectoryName = RDUtils.GetAvailableDirectoryName(Path.Combine(Path.GetDirectoryName(str1), Path.GetFileNameWithoutExtension(str1)));
                    RDDirectory.CreateDirectory(availableDirectoryName);
                    try {
                        ZipUtils.Unzip(str1, availableDirectoryName);
                    } catch (Exception ex) {
                        editor.ShowNotificationPopup(RDString.Get("editor.notification.unzipFailed"));
                        Debug.LogError("Unzip failed: " + ex);
                        Directory.Delete(availableDirectoryName, true);
                    }
                    string levelOnDirectory = editor.Invoke<string>("FindAdofaiLevelOnDirectory", availableDirectoryName);
                    if(levelOnDirectory == null) {
                        editor.ShowNotificationPopup(RDString.Get("editor.notification.levelNotFound"));
                        Directory.Delete(availableDirectoryName, true);
                        goto StopLoading;
                    }
                    game.levelPath = levelOnDirectory;
                } else game.levelPath = str1;
            } else game.levelPath = path;
            SequenceText = Main.Instance.Localization["AsyncMapLoad.ResetData"];
            scrController.deaths = 0;
            customLevelId = GCS.customLevelId;
            GCS.customLevelId = null;
            Persistence.UpdateLastUsedFolder(ADOBase.levelPath);
            Persistence.UpdateLastOpenedLevel(ADOBase.levelPath);
            LoadLevel();
            return;
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }

StopLoading:
        MainThread.Run(Main.Instance, ForceEnd);
    }

    private void ForceEnd() {
        LoadScreen.Hide();
        AsyncMapLoad.isLoading = false;
    }

    public void LoadLevel() {
        LoadResult status = LoadResult.Error;
        scnEditor.instance.isLoading = true;
        try {
            LoadLevel(ADOBase.levelPath);
        } catch (Exception e) {
            string errorMessage = MakeExceptionMessage(e);
            Debug.Log(errorMessage);
            LoadAfter(false, status, errorMessage);
        }
    }

    public static string MakeExceptionMessage(Exception e) => "Error loading level file at " + ADOBase.levelPath + ": " + e.Message + ", Stacktrace:\n" + e.StackTrace;

    public void LoadLevel(string levelPath) {
        SequenceText = Main.Instance.Localization["AsyncMapLoad.CleanMemory"];
        scnEditor editor = scnEditor.instance;
        scnGame game = scnGame.instance;
        game.events.Clear();
        game.decorations.Clear();
        editor.GetValue<Dictionary<string, string>>("errorImageResult").Clear();
        editor.SetValue("isUnauthorizedAccess", false);
        game.FlushUnusedMemory();
        Resources.UnloadUnusedAssets();
        GC.Collect();
        new ReadLevel(this, levelPath).Read();
    }

    public void OnRemove() {
        if(LoadScreen.instance.Sequence.Count != 1) return;
        LoadAfter(true, LoadResult.Successful, null);
    }

    public void LoadAfter(bool complete, LoadResult status, string errorMessage) {
        if(!MainThread.IsMainThread()) {
            MainThread.Run(Main.Instance, new LoadAfterValues(this, complete, status, errorMessage).Run);
            return;
        }
        scnEditor editor = scnEditor.instance;
        if(complete) {
            scnGame.instance.imgHolder.Unload(true);
            editor.ShowNotification(RDString.Get("editor.notification.levelLoaded"));
            editor.SetValue("unsavedChanges", false);
        } else {
            editor.customLevel.levelPath = lastLevelPath;
            GCS.customLevelId = customLevelId;
            editor.ShowNotificationPopup(errorMessage, [
                new scnEditor.NotificationAction(RDString.Get("editor.notification.copyText"), () => {
                    scnEditor scnEditor = scnEditor.instance;
                    scnEditor.notificationPopupContent.text.CopyToClipboard();
                    scnEditor.ShowNotification(RDString.Get("editor.notification.copiedText"));
                }),
                new scnEditor.NotificationAction(RDString.Get("editor.ok"), () => {
                    scnEditor.instance.Invoke("CloseNotificationPopup");
                })
            ], RDString.Get($"editor.notification.loadingFailed.{status}"));
        }
        editor.isLoading = false;
        editor.CloseAllPanels();
        editor.Invoke("ShowImageLoadResult");
        ForceEnd();
        Dispose();
    }

    public override void Dispose() {
        base.Dispose();
        LoadScreen.OnRemove -= OnRemove;
    }

    private class LoadAfterValues(LoadMap loadMap, bool complete, LoadResult status, string errorMessage) {
        public void Run() => loadMap.LoadAfter(complete, status, errorMessage);
    }
}