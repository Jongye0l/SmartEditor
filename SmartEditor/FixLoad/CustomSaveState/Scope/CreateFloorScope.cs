namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class CreateFloorScope(float angle) : CustomSaveStateScope {
    public int index = scnEditor.instance.selectedFloors[0].seqID;
    public bool deleted;

    public override void Undo() {
        scnEditor editor = scnEditor.instance;
        if(!deleted) FixPrivateMethod.DeleteFloor(index + 1);
        else if(index <= 1) return;
        else {
            FixPrivateMethod.OffsetFloorIDsInEvents(index - 1, 1);
            editor.InsertFloatFloor(index - 1, angle + 180 % 360);
        }
        scrFloor floor = editor.floors[index];
        editor.SelectFloor(floor);
        FixPrivateMethod.MoveCameraToFloor(floor);
    }

    public override void Redo() {
        scnEditor editor = scnEditor.instance;
        scrFloor floor;
        if(!deleted) {
            FixPrivateMethod.OffsetFloorIDsInEvents(index, 1);
            editor.InsertFloatFloor(index, angle);
            floor = editor.floors[index + 1];
        } else if(index <= 1) return;
        else {
            FixPrivateMethod.DeleteFloor(index);
            floor = editor.floors[index - 1];
        }
        editor.SelectFloor(floor);
        FixPrivateMethod.MoveCameraToFloor(floor);
    }
}