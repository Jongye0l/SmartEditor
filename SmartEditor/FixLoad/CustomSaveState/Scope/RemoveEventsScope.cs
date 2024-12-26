using System.Collections.Generic;
using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class RemoveEventsScope : CustomSaveStateScope {
    public LevelEvent[] events;

    public RemoveEventsScope() : base(false) {
    }

    public RemoveEventsScope(List<LevelEvent> events) : this() => SetEvents(events);


    public void SetEvents(List<LevelEvent> events) {
        this.events = events.ToArray();
    }

    public override void Undo() {
        if(events == null || events.Length == 0) return;
        foreach(LevelEvent @event in events) {
            if(@event.IsDecoration) scnEditor.instance.decorations.Add(@event);
            else scnEditor.instance.events.Add(@event);
        }
        UpdateEvent();
    }

    public override void Redo() {
        if(events == null || events.Length == 0) return;
        foreach(LevelEvent @event in events) {
            if(@event.IsDecoration) scnEditor.instance.decorations.Remove(@event);
            else scnEditor.instance.events.Remove(@event);
        }
        UpdateEvent();
    }

    public void UpdateEvent() {
        scnEditor editor = scnEditor.instance;
        editor.ApplyEventsToFloors();
        int floor = events[0].floor;
        editor.levelEventsPanel.ShowTabsForFloor(floor);
        editor.ShowEventIndicators(editor.floors[floor]);
    }
}