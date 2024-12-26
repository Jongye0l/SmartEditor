namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class SelectFloorScope(bool skipSaving) : CustomSaveStateScope(skipSaving) {
    public int[] selectedFloors = GetSelectedFloors();

    public static int[] GetSelectedFloors() {
        scnEditor editor = scnEditor.instance;
        if(editor.SelectionIsEmpty()) return null;
        if(editor.SelectionIsSingle()) return [editor.selectedFloors[0].seqID];
        int[] floors = new int[editor.selectedFloors.Count];
        for(int i = 0; i < editor.selectedFloors.Count; i++) floors[i] = editor.selectedFloors[i].seqID;
        return floors;
    }

    public static void SelectFloors(int[] floors) {
        scnEditor editor = scnEditor.instance;
        if(floors == null) editor.DeselectFloors();
        else if(floors.Length == 1) editor.SelectFloor(editor.floors[floors[0]]);
        else editor.MultiSelectFloors(editor.floors[floors[0]], editor.floors[floors[^1]]);
    }

    public override void Undo() {
        int[] selected = selectedFloors;
        selectedFloors = GetSelectedFloors();
        SelectFloors(selected);
    }

    public override void Redo() => Undo();
}