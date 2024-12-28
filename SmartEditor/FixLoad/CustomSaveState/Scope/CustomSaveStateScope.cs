using System;
using UnityEngine;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public abstract class CustomSaveStateScope : LevelState, IDisposable {

    public CustomSaveStateScope(bool skipSaving, bool dataHasChanged) {
        scnEditor editor = scnEditor.instance;
        if(!skipSaving && editor.initialized && editor.changingState == 0) {
            SaveStatePatch.undoStates.Add(this);
            SaveStatePatch.redoStates.Clear();
            SaveStatePatch.saveStateLastFrame.SetValue(editor, Time.frameCount);
            if(dataHasChanged) SaveStatePatch.unsavedChanges.Invoke(editor, [true]);
        }
        editor.changingState++;
    }

    public virtual void Dispose() => scnEditor.instance.changingState--;
}