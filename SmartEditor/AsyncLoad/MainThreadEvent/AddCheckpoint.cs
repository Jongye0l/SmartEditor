namespace SmartEditor.AsyncLoad.MainThreadEvent;

public class AddCheckpoint : ApplyMainThread {
    public scrFloor floor;
    public int offset;

    public AddCheckpoint(scrFloor floor, int offset) {
        this.floor = floor;
        this.offset = offset;
    }

    public override void Run() {
        floor.gameObject.AddComponent<ffxCheckpoint>().checkpointTileOffset = offset;
    }
}