using System;
using System.Collections.Generic;
using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState;

public abstract class LevelState {
    public abstract void Undo();
    public abstract void Redo();
}

public class DefaultLevelState : LevelState {
    public int[] selectedFloors;
    public int[] selectedDecorationIndices;
    public LevelEventType settingsEventType;
    public LevelEventType floorEventType;
    public int floorEventTypeIndex;
    public ChangedEventCache[] changedEvents;
    public ChangedFloorCache[] changedFloors;
    public Dictionary<SaveStatePatch.EventKey, SaveStatePatch.EventValue> changedEventValues;

    public override void Undo() => throw new NotSupportedException();
    public override void Redo() => throw new NotSupportedException();
}