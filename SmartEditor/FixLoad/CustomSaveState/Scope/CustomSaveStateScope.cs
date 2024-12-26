using System;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public abstract class CustomSaveStateScope : LevelState, IDisposable {

    public CustomSaveStateScope(bool skipSaving) {
        if(!skipSaving && scnEditor.instance.changingState == 0) {
            SaveStatePatch.undoStates.Add(this);
            SaveStatePatch.redoStates.Clear();
        }
        scnEditor.instance.changingState++;
    }

    public virtual void Dispose() => scnEditor.instance.changingState--;
}