using System.Collections.Generic;
using JALib.Tools;

namespace SmartEditor.AsyncLoad.Sequence;

public class FloorShapeUpdate : LoadSequence {
    public int updatedFloor;
    public int updateRequestFloor;
    public bool updating;
    public bool finish;

    public void AddUpdateRequest() {
        updateRequestFloor++;
        lock(this) {
            if(updating || updatedFloor >= updateRequestFloor) return;
            updating = true;
        }
        MainThread.Run(Main.Instance, UpdateFloorShape);
    }

    public void AddLastRequest() {
        AddUpdateRequest();
        finish = true;
    }

    public void UpdateFloorShape() {
        List<scrFloor> listFloors = scrLevelMaker.instance.listFloors;
Restart:
        for(;updatedFloor < updateRequestFloor; updatedFloor++) {
            scrFloor floor = listFloors[updatedFloor];
            floor.UpdateAngle();
            floor.GetOrAddComponent<ffxChangeTrack>();
        }
        bool end;
        lock(this) {
            if(updatedFloor < updateRequestFloor) goto Restart;
            updating = false;
            end = finish;
        }
        SequenceText = string.Format(Main.Instance.Localization["AsyncMapLoad.UpdateShape"], updatedFloor + 1, listFloors.Count);
        if(end) Dispose();
    }
}