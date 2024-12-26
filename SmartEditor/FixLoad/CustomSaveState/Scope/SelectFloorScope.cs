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

    public override void Undo() {
        int[] selected = selectedFloors;
        selectedFloors = GetSelectedFloors();
        scnEditor editor = scnEditor.instance;
        if(selected == null) editor.DeselectFloors();
        else if(selected.Length == 1) editor.SelectFloor(editor.floors[selected[0]]);
        else editor.MultiSelectFloors(editor.floors[selected[0]], editor.floors[selected[^1]]);
    }

    public override void Redo() => Undo();
}