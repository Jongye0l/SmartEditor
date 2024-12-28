using System.Collections.Generic;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class PasteFloorsScope : CustomSaveStateScope {
    public scnEditor.FloorData[] clipboard;
    public int seqID;
    public bool alsoDecorations;

    public PasteFloorsScope(bool alsoDecorations) : base(false, true) {
        List<object> list = scnEditor.instance.clipboard;
        clipboard = new scnEditor.FloorData[list.Count];
        for(int i = 0; i < list.Count; i++) clipboard[i] = (scnEditor.FloorData) list[i];
        seqID = scnEditor.instance.selectedFloors[0].seqID;
        this.alsoDecorations = alsoDecorations;
    }

    public override void Undo() {
        int seqID = this.seqID + 1;
        for(int i = 0; i < clipboard.Length; i++) FixPrivateMethod.DeleteFloor(seqID, false);
        DeleteTileUpdate.UpdateTile(seqID, clipboard.Length);
        scnEditor editor = scnEditor.instance;
        editor.SelectFloor(editor.floors[this.seqID]);
        FixPrivateMethod.MoveCameraToFloor(editor.floors[this.seqID]);
    }

    public override void Redo() {
        scnEditor editor = scnEditor.instance;
        List<object> oldClipboard = editor.clipboard;
        editor.clipboard = new List<object>(clipboard.Length);
        editor.selectedFloors = [editor.floors[seqID]];
        foreach(scnEditor.FloorData data in clipboard) editor.clipboard.Add(data);
        editor.PasteFloors(alsoDecorations);
        editor.clipboard = oldClipboard;
    }
}