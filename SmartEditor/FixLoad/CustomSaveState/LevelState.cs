using System.Collections.Generic;
using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState;

public class LevelState {
    public int[] selectedFloors;
    public int[] selectedDecorationIndices;
    public LevelEventType settingsEventType;
    public LevelEventType floorEventType;
    public int floorEventTypeIndex;
    public ChangedEventCache[] changedEvents;
    public ChangedFloorCache[] changedFloors;
    public Dictionary<SaveStatePatch.EventKey, SaveStatePatch.EventValue> changedEventValues;
}