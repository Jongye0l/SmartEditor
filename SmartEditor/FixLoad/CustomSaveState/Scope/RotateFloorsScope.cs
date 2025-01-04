using System.Collections.Generic;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class RotateFloorsScope : CustomSaveStateScope {
    public byte act;
    public int seqID;
    public int size;
    public bool adjustOnRotate;
    public float? customAngle;

    public RotateFloorsScope(int act) : base(false, true) {
        List<scrFloor> selectedFloors = scnEditor.instance.selectedFloors;
        if(selectedFloors.Count > 0) {
            seqID = selectedFloors[0].seqID;
            size = selectedFloors.Count;
        }
        this.act = (byte) act;
        if(FlipAndRotateTilesAPI.CheckMod()) {
            adjustOnRotate = FlipAndRotateTilesAPI.AdjustOnRotate;
            customAngle = FlipAndRotateTilesAPI.CustomAngle;
        }
    }

    public RotateFloorsScope(scrFloor floor, int act) : base(false, true) {
        seqID = floor.seqID;
        size = 1;
        this.act = (byte) act;
        if(FlipAndRotateTilesAPI.CheckMod()) {
            adjustOnRotate = FlipAndRotateTilesAPI.AdjustOnRotate;
            customAngle = FlipAndRotateTilesAPI.CustomAngle;
        }
    }

    public override void Undo() {
        if(size == 0) return;
        scnEditor editor = scnEditor.instance;
        bool modInit = FlipAndRotateTilesAPI.CheckMod();
        bool originalAdjust = false;
        float? originalAngle = null;
        if(modInit) {
            originalAdjust = FlipAndRotateTilesAPI.AdjustOnRotate;
            originalAngle = FlipAndRotateTilesAPI.CustomAngle;
            FlipAndRotateTilesAPI.AdjustOnRotate = adjustOnRotate;
            FlipAndRotateTilesAPI.CustomAngle = act == 2 ? -customAngle : customAngle;
        }
        try {
            if(size == 1) {
                if(act == 2) editor.RotateFloor180(editor.floors[seqID]);
                else editor.RotateFloor(editor.floors[seqID], act == 0);
            } else {
                for(int i = 0; i < size; i++) {
                    if(act == 2) editor.RotateFloor180(editor.floors[seqID + i], false);
                    else editor.RotateFloor(editor.floors[seqID + i], act == 0, false);
                }
                RotateTileUpdate.UpdateTile(seqID, size, act == 0, act == 2);
                editor.MultiSelectFloors(editor.floors[seqID], editor.floors[seqID + size - 1]);
            }
        } finally {
            if(modInit) {
                FlipAndRotateTilesAPI.AdjustOnRotate = originalAdjust;
                FlipAndRotateTilesAPI.CustomAngle = originalAngle;
            }
        }
    }

    public override void Redo() {
        if(size == 0) return;
        scnEditor editor = scnEditor.instance;
        bool modInit = FlipAndRotateTilesAPI.CheckMod();
        bool originalAdjust = false;
        float? originalAngle = null;
        if(modInit) {
            originalAdjust = FlipAndRotateTilesAPI.AdjustOnRotate;
            originalAngle = FlipAndRotateTilesAPI.CustomAngle;
            FlipAndRotateTilesAPI.AdjustOnRotate = adjustOnRotate;
            FlipAndRotateTilesAPI.CustomAngle = customAngle;
        }
        try {
            if(size == 1) {
                if(act == 2) editor.RotateFloor180(editor.floors[seqID]);
                else editor.RotateFloor(editor.floors[seqID], act == 1);
            } else {
                for(int i = 0; i < size; i++) {
                    if(act == 2) editor.RotateFloor180(editor.floors[seqID + i], false);
                    else editor.RotateFloor(editor.floors[seqID + i], act == 1, false);
                }
                RotateTileUpdate.UpdateTile(seqID, size, act == 1, act == 2);
                editor.MultiSelectFloors(editor.floors[seqID], editor.floors[seqID + size - 1]);
            }
        } finally {
            if(modInit) {
                FlipAndRotateTilesAPI.AdjustOnRotate = originalAdjust;
                FlipAndRotateTilesAPI.CustomAngle = originalAngle;
            }
        }
    }
}