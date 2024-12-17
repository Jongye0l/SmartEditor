using ADOFAI;

namespace SmartEditor.LevelEvent;

public class AngleEvent : CustomEvent {
    public float angleOffset;

    public AngleEvent(int newFloor, LevelEventType type, LevelEventInfo info) : base(newFloor, type, info) {
    }
}