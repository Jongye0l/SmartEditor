namespace SmartEditor.FixLoad.CustomSaveState;

public class ChangedFloorCache {
    public Action action;
    public float angle;
    public int index;

    public ChangedFloorCache(Action action, float angle, int index) {
        this.action = action;
        this.angle = angle;
        this.index = index;
    }

    public enum Action {
        Add,
        Remove
    }
}