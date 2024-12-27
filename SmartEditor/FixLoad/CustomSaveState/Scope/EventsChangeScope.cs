using System.Collections.Generic;
using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class EventsChangeScope : CustomSaveStateScope {
    public static EventsChangeScope instance;
    public LevelEvent[] events;
    public List<DecorationCache> decorations;
    public int startEventCount;
    public bool delete;

    public EventsChangeScope() : base(false) {
        startEventCount = scnEditor.instance.events.Count;
        instance ??= this;
        decorations = [];
    }

    public EventsChangeScope(LevelEvent @event, int index) : base(false) {
        events = [];
        decorations = [ new DecorationCache(@event, index) ];
    }

    public EventsChangeScope(LevelEvent @event) : this(@event, scnEditor.instance.decorations.IndexOf(@event)) {
        delete = true;
    }

    public EventsChangeScope(List<LevelEvent> events) : base(false) => SetEvents(events);

    public void SetEvents(List<LevelEvent> events) {
        this.events = events.ToArray();
        delete = true;
    }

    public void Add() {
        foreach(LevelEvent @event in events) {
            if(@event.IsDecoration) scnEditor.instance.AddDecoration(@event);
            else scnEditor.instance.events.Add(@event);
        }
        foreach(DecorationCache cache in decorations) scnEditor.instance.AddDecoration(cache.decoration, cache.index);
    }

    public void Remove() {
        LevelEvent decoration = null;
        foreach(LevelEvent @event in events) {
            scnEditor.instance.RemoveEvent(@event, true);
            if(@event.IsDecoration) decoration ??= @event;
        }
        if(decoration == null && decorations.Count == 0) return;
        foreach(DecorationCache cache in decorations) scnEditor.instance.RemoveEvent(cache.decoration, true);
        scnEditor.instance.RemoveEvent(decoration ?? decorations[0].decoration);
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

    public static void AddDecoration(LevelEvent @event, int index) {
        if(instance is not { events: null }) return;
        instance.decorations.Add(new DecorationCache(@event, index));
    }

    public override void Dispose() {
        base.Dispose();
        if(instance == this) instance = null;
        if(events != null) return;
        int count = scnEditor.instance.events.Count - startEventCount;
        events = new LevelEvent[count];
        scnEditor.instance.events.CopyTo(startEventCount, events, 0, count);
    }

    public class DecorationCache(LevelEvent decoration, int index) {
        public LevelEvent decoration = decoration;
        public int index = index;
    }
}