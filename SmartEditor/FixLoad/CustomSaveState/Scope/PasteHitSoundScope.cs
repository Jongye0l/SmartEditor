using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class PasteHitSoundScope : CustomSaveStateScope {
    public int seqId;
    public LevelEvent copiedHitsound;
    public LevelEvent removedHitsound;
    public LevelEvent previousHitsound;
    public bool previousNeed;

    public PasteHitSoundScope(int seqId) : base(false, true) {
        this.seqId = seqId;
        copiedHitsound = FixPrivateMethod.copiedHitsound;
        previousNeed = seqId < scnEditor.instance.floors.Count - 2;
        foreach(LevelEvent @event in scnEditor.instance.events) {
            if(@event.eventType != LevelEventType.SetHitsound) continue;
            if(@event.floor == seqId) removedHitsound = @event;
            else if(@event.floor == seqId + 1) previousNeed = false;
            if(removedHitsound != null && !previousNeed) break;
        }
    }

    public override void Undo() {
        scnEditor editor = scnEditor.instance;
        for(int i = 0; i < editor.events.Count; i++) {
            LevelEvent @event = editor.events[i];
            if(@event.floor == seqId && @event.eventType == LevelEventType.SetHitsound) {
                editor.events.RemoveAt(i);
                break;
            }
        }
        if(removedHitsound != null) editor.events.Add(removedHitsound);
        if(previousNeed) editor.events.Remove(previousHitsound);
        editor.ApplyEventsToFloors();
        editor.levelEventsPanel.ShowTabsForFloor(seqId);
        editor.ShowEventIndicators(editor.floors[seqId]);
    }

    public override void Redo() {
        scnEditor editor = scnEditor.instance;
        editor.events.Add(FixPrivateMethod.CopyEvent(copiedHitsound, seqId));
        if(removedHitsound != null) editor.events.Remove(removedHitsound);
        if(previousNeed) editor.events.Add(previousHitsound);
        editor.ApplyEventsToFloors();
        scrFloor floor = editor.floors[seqId];
        editor.SelectFloor(floor);
        editor.levelEventsPanel.ShowTabsForFloor(seqId);
        editor.levelEventsPanel.selectedEventType = LevelEventType.SetHitsound;
        editor.levelEventsPanel.ShowPanel(LevelEventType.SetHitsound);
        editor.ShowEventIndicators(floor);
    }

    public override void Dispose() {
        base.Dispose();
        if(previousNeed)
            foreach(LevelEvent @event in scnEditor.instance.events)
                if(@event.floor == seqId + 1 && @event.eventType == LevelEventType.SetHitsound) {
                    previousHitsound = @event;
                    break;
                }
    }
}