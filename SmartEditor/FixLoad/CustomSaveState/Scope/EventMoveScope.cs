using System.Collections.Generic;
using ADOFAI;
using EditorTweaks.Patch.Timeline;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class EventMoveScope : CustomSaveStateScope {
    public EventCache[] eventCaches;

    public EventMoveScope(TimelinePanel timelinePanel) : base(false, true) {
        HashSet<LevelEvent> events = timelinePanel.selectedEvents;
        eventCaches = new EventCache[events.Count];
        int i = 0;
        foreach(LevelEvent levelEvent in events) eventCaches[i++] = new EventCache(levelEvent);
    }

    public override void Undo() {
        if(eventCaches == null) return;
        foreach(EventCache eventCache in eventCaches) eventCache.Change();
    }

    public override void Redo() => Undo();

    public class EventCache(LevelEvent @event) {
        public readonly LevelEvent @event = @event;
        public int floor = @event.floor;
        public float duration = (float) @event["duration"];
        public float angleOffset = (float) @event["angleOffset"];

        public void Change() {
            (floor, @event.floor) = (@event.floor, floor);
            (@event["duration"], duration) = (duration, (float) @event["duration"]);
            (@event["angleOffset"], angleOffset) = (angleOffset, (float) @event["angleOffset"]);
        }
    }
}