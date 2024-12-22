using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState;

public class ChangedEventCache {
    public Action action;
    public LevelEvent @event;

    public ChangedEventCache(Action action, LevelEvent @event) {
        this.action = action;
        this.@event = @event;
    }

    public enum Action {
        Add,
        Remove
    }
}