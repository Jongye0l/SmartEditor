using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class PasteHitSoundScope : CustomSaveStateScope {
    public int seqId;
    public LevelEvent copiedHitsound;
    public LevelEvent removedHitsound;
    public LevelEvent previousHitsound;

    public PasteHitSoundScope(int seqId) : base(false) {
        this.seqId = seqId;
        copiedHitsound = FixPrivateMethod.copiedHitsound;
        foreach(LevelEvent @event in scnEditor.instance.events) {
            if(@event.floor == seqId && @event.eventType == LevelEventType.SetHitsound) {
                removedHitsound = @event;
                break;
            }
        }
    }

    public override void Undo() {
        scnEditor editor = scnEditor.instance;
        editor.events.Remove(copiedHitsound);
        editor.events.Add(removedHitsound);
        if(previousHitsound != null) editor.events.Remove(previousHitsound);
        editor.ApplyEventsToFloors();
        editor.levelEventsPanel.ShowTabsForFloor(seqId);
        editor.ShowEventIndicators(editor.floors[seqId]);
    }

    public override void Redo() {
        scnEditor editor = scnEditor.instance;
        editor.events.Add(copiedHitsound);
        editor.events.Remove(removedHitsound);
        if(previousHitsound != null) editor.events.Add(previousHitsound);
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
        previousHitsound = FixPrivateMethod.previousHitsound;
        if(!scnEditor.instance.events.Contains(previousHitsound)) previousHitsound = null;
    }
}