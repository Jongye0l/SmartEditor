using System.Collections.Generic;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class FlipFloorsScope : CustomSaveStateScope {
    public bool horizontal;
    public int seqID;
    public int size;

    public FlipFloorsScope(bool horizontal) : base(false, true) {
        List<scrFloor> selectedFloors = scnEditor.instance.selectedFloors;
        if(selectedFloors.Count > 0) {
            seqID = selectedFloors[0].seqID;
            size = selectedFloors.Count;
        }
        this.horizontal = horizontal;
    }

    public FlipFloorsScope(scrFloor floor, bool horizontal) : base(false, true) {
        seqID = floor.seqID;
        size = 1;
        this.horizontal = horizontal;
    }

    public override void Undo() {
        if(size == 0) return;
        scnEditor editor = scnEditor.instance;
        if(size == 1) editor.FlipFloor(editor.floors[seqID], horizontal);
        else {
            for(int i = 0; i < size; i++) editor.FlipFloor(editor.floors[seqID + i], horizontal, false);
            FlipTileUpdate.UpdateTile(seqID, size, horizontal);
            editor.MultiSelectFloors(editor.floors[seqID], editor.floors[seqID + size - 1]);
        }
    }

    public override void Redo() => Undo();
}