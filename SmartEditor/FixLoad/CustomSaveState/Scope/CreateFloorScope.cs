namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class CreateFloorScope : CustomSaveStateScope {
    public static CreateFloorScope instance;
    public int index = scnEditor.instance.selectedFloors[0].seqID;
    public float angle;
    public DeleteFloorScope deleted;

    public CreateFloorScope(float angle) : base(false, true) {
        this.angle = angle;
        if(scnEditor.instance.changingState == 1) instance = this;
    }

    public override void Undo() {
        scnEditor editor = scnEditor.instance;
        if(deleted == null) {
            FixPrivateMethod.DeleteFloor(index + 1);
            scrFloor floor = editor.floors[index];
            editor.SelectFloor(floor);
            FixPrivateMethod.MoveCameraToFloor(floor);
        } else deleted.Undo();
    }

    public override void Redo() {
        scnEditor editor = scnEditor.instance;
        if(deleted == null) {
            FixPrivateMethod.OffsetFloorIDsInEvents(index, 1);
            editor.InsertFloatFloor(index, angle);
            scrFloor floor = editor.floors[index + 1];
            editor.SelectFloor(floor);
            FixPrivateMethod.MoveCameraToFloor(floor);
        } else deleted.Redo();
    }

    public override void Dispose() {
        base.Dispose();
        if(instance == this) instance = null;
    }
}