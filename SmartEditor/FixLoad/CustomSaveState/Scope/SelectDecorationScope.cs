using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class SelectDecorationScope() : CustomSaveStateScope(false) {
    public LevelEvent[] selectedDecorations = scnEditor.instance.selectedDecorations.ToArray();

    public override void Undo() {
        LevelEvent[] selected = scnEditor.instance.selectedDecorations.ToArray();
        scnEditor.instance.DeselectAllDecorations();
        foreach(LevelEvent decoration in selectedDecorations) scnEditor.instance.SelectDecoration(decoration, false, true, true, true);
        selectedDecorations = selected;
    }

    public override void Redo() => Undo();
}