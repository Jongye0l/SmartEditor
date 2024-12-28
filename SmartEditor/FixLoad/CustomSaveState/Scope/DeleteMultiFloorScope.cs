using System.Collections.Generic;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class DeleteMultiFloorScope : CustomSaveStateScope {
    public static DeleteMultiFloorScope instance;
    public List<DeleteFloorScope> deleteFloorScopes = [];
    public int[] beforeSelected;
    public int[] afterSelected;

    public DeleteMultiFloorScope() : base(false, true) {
        instance = this;
        beforeSelected = SelectFloorScope.GetSelectedFloors();
    }

    public override void Undo() {
        bool reloadEvents = false;
        for(int i = deleteFloorScopes.Count - 1; i >= 0; i--) {
            DeleteFloorScope deleteFloorScope = deleteFloorScopes[i];
            deleteFloorScope.Undo();
            if(deleteFloorScope.events.Length > 0) reloadEvents = true;
        }
        if(reloadEvents) scnGame.instance.ApplyEventsToFloors(scrLevelMaker.instance.listFloors);
        SelectFloorScope.SelectFloors(beforeSelected);
    }

    public override void Redo() {
        foreach(DeleteFloorScope scope in deleteFloorScopes) scope.Redo();
        SelectFloorScope.SelectFloors(afterSelected);
    }

    public override void Dispose() {
        base.Dispose();
        instance = null;
        afterSelected = SelectFloorScope.GetSelectedFloors();
    }
}