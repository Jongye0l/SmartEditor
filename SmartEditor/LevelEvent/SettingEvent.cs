using ADOFAI;

namespace SmartEditor.LevelEvent;

public class SettingEvent : CustomEvent {
    public SettingEvent(int newFloor, LevelEventType type, LevelEventInfo info) : base(newFloor, type, info) {
    }
}