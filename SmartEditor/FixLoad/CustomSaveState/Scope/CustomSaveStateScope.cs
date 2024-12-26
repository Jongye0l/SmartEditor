﻿using System;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public abstract class CustomSaveStateScope : LevelState, IDisposable {

    public CustomSaveStateScope() {
        if(scnEditor.instance.changingState == 0) SaveStatePatch.undoStates.Add(this);
        scnEditor.instance.changingState++;
    }

    public virtual void Dispose() => scnEditor.instance.changingState--;
}