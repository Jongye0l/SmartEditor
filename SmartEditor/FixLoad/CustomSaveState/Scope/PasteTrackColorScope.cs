using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class PasteTrackColorScope : CustomSaveStateScope {
    public int seqId;
    public LevelEvent copiedTrackColor;
    public LevelEvent removedTrackColor;
    public LevelEvent previousTrackColor;
    public bool previousNeed;

    public PasteTrackColorScope(int seqId, bool previousNeed) : base(false) {
        this.seqId = seqId;
        copiedTrackColor = FixPrivateMethod.copiedTrackColor;
        previousNeed = seqId < scnEditor.instance.floors.Count - 2 && previousNeed;
        foreach(LevelEvent @event in scnEditor.instance.events) {
            if(@event.eventType != LevelEventType.ColorTrack) continue;
            if(@event.floor == seqId) removedTrackColor = @event;
            else if(@event.floor == seqId + 1) previousNeed = false;
            if(removedTrackColor != null && !previousNeed) break;
        }
    }

    public override void Undo() {
        scnEditor editor = scnEditor.instance;
        for(int i = 0; i < editor.events.Count; i++) {
            LevelEvent @event = editor.events[i];
            if(@event.floor == seqId && @event.eventType == LevelEventType.ColorTrack) {
                editor.events.RemoveAt(i);
                break;
            }
        }
        if(removedTrackColor != null) editor.events.Add(removedTrackColor);
        if(previousNeed) editor.events.Remove(previousTrackColor);
        editor.ApplyEventsToFloors();
        editor.levelEventsPanel.ShowTabsForFloor(seqId);
        editor.ShowEventIndicators(editor.floors[seqId]);
    }

    public override void Redo() {
        scnEditor editor = scnEditor.instance;
        editor.events.Add(FixPrivateMethod.CopyEvent(copiedTrackColor, seqId));
        if(removedTrackColor != null) editor.events.Remove(removedTrackColor);
        if(previousNeed) editor.events.Add(previousTrackColor);
        editor.ApplyEventsToFloors();
        scrFloor floor = editor.floors[seqId];
        editor.SelectFloor(floor);
        editor.levelEventsPanel.ShowTabsForFloor(seqId);
        editor.levelEventsPanel.selectedEventType = LevelEventType.ColorTrack;
        editor.levelEventsPanel.ShowPanel(LevelEventType.ColorTrack);
        editor.ShowEventIndicators(floor);
    }

    public override void Dispose() {
        base.Dispose();
        if(previousNeed) 
            foreach(LevelEvent @event in scnEditor.instance.events) 
                if(@event.floor == seqId + 1 && @event.eventType == LevelEventType.ColorTrack) {
                    previousTrackColor = @event;
                    break;
                }
    }
}