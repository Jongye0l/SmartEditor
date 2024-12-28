namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class SelectFloorScope(bool skipSaving) : CustomSaveStateScope(skipSaving, false) {
    public int[] selectedFloors = GetSelectedFloors();

    public static int[] GetSelectedFloors() {
        scnEditor editor = scnEditor.instance;
        if(editor.SelectionIsEmpty()) return null;
        if(editor.SelectionIsSingle()) return [editor.selectedFloors[0].seqID];
        int[] floors = new int[editor.selectedFloors.Count + 1];
        floors[0] = editor.multiSelectPoint?.seqID ?? -1;
        for(int i = 0; i < editor.selectedFloors.Count; i++) floors[i + 1] = editor.selectedFloors[i].seqID;
        return floors;
    }

    public static void SelectFloors(int[] floors) {
        scnEditor editor = scnEditor.instance;
        if(floors == null) editor.DeselectFloors();
        else if(floors.Length == 1) editor.SelectFloor(editor.floors[floors[0]]);
        else {
            editor.MultiSelectFloors(editor.floors[floors[1]], editor.floors[floors[^1]]);
            if(floors[0] != -1) editor.multiSelectPoint = editor.floors[floors[0]];
        }
    }

    public override void Undo() {
        int[] selected = selectedFloors;
        selectedFloors = GetSelectedFloors();
        SelectFloors(selected);
    }

    public override void Redo() => Undo();
}