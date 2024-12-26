using System.Collections.Generic;
using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class EventsChangeScope : CustomSaveStateScope {
    public LevelEvent[] events;
    public int startEventCount;
    public int startDecorationCount;
    public bool delete;

    public EventsChangeScope() : base(false) {
        startEventCount = scnEditor.instance.events.Count;
        startDecorationCount = scnEditor.instance.decorations.Count;
    }

    public EventsChangeScope(LevelEvent @event, bool delete) : this() {
        events = [@event];
        this.delete = delete;
    }

    public EventsChangeScope(List<LevelEvent> events) : this() => SetEvents(events);

    public void SetEvents(List<LevelEvent> events) {
        this.events = events.ToArray();
        delete = true;
    }

    public void Add() {
        foreach(LevelEvent @event in events) {
            if(@event.IsDecoration) scnEditor.instance.decorations.Add(@event);
            else scnEditor.instance.events.Add(@event);
        }
    }

    public void Remove() {
        foreach(LevelEvent @event in events) {
            if(@event.IsDecoration) scnEditor.instance.decorations.Remove(@event);
            else scnEditor.instance.events.Remove(@event);
        }
    }

    public override void Undo() {
        if(events == null || events.Length == 0) return;
        if(delete) Add();
        else Remove();
        UpdateEvent();
    }

    public override void Redo() {
        if(events == null || events.Length == 0) return;
        if(delete) Remove();
        else Add();
        UpdateEvent();
    }

    public void UpdateEvent() {
        scnEditor editor = scnEditor.instance;
        editor.ApplyEventsToFloors();
        int floor = events[0].floor;
        editor.levelEventsPanel.ShowTabsForFloor(floor);
        editor.ShowEventIndicators(editor.floors[floor]);
    }

    public void DeleteEvents(LevelEvent @event) {
        events = [ @event ];
        delete = true;
    }

    public override void Dispose() {
        base.Dispose();
        if(events != null) return;
        events = new LevelEvent[scnEditor.instance.events.Count - startEventCount + scnEditor.instance.decorations.Count - startDecorationCount];
        int index = 0;
        for(int i = startEventCount; i < scnEditor.instance.events.Count; i++) events[index++] = scnEditor.instance.events[i];
        for(int i = startDecorationCount; i < scnEditor.instance.decorations.Count; i++) events[index++] = scnEditor.instance.decorations[i];
    }
}