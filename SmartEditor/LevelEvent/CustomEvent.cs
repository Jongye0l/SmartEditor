using ADOFAI;

namespace SmartEditor.LevelEvent;

public abstract class CustomEvent : ADOFAI.LevelEvent {

    protected CustomEvent(int newFloor, LevelEventType type, LevelEventInfo info) : base(newFloor, type, info) {
        data = new EventData(this);
    }
}