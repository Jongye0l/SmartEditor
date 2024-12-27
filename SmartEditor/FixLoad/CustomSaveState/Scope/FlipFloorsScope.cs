using System.Collections.Generic;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class FlipFloorsScope : CustomSaveStateScope {
    public bool horizontal;
    public int[] floors;

    public FlipFloorsScope(bool horizontal) : base(false) {
        List<scrFloor> selectedFloors = scnEditor.instance.selectedFloors;
        floors = new int[selectedFloors.Count];
        for(int i = 0; i < selectedFloors.Count; i++) floors[i] = selectedFloors[i].seqID;
        this.horizontal = horizontal;
    }

    public FlipFloorsScope(scrFloor floor, bool horizontal) : base(false) {
        floors = [floor.seqID];
        this.horizontal = horizontal;
    }

    public override void Undo() {
        if(floors.Length == 0) return;
        scnEditor editor = scnEditor.instance;
        if(floors.Length == 1) editor.FlipFloor(editor.floors[floors[0]], horizontal);
        else {
            foreach(int i in floors) editor.FlipFloor(editor.floors[i], horizontal, false);
            FlipTileUpdate.UpdateTile(floors[0], floors.Length, horizontal);
            editor.MultiSelectFloors(editor.floors[floors[0]], editor.floors[floors[^1]]);
        }
    }

    public override void Redo() => Undo();
}