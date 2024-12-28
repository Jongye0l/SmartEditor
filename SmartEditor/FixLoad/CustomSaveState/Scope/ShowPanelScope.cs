using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class ShowPanelScope : CustomSaveStateScope {
    public static LevelEventType lastPanel1;
    public static LevelEventType lastPanel2;
    public InspectorPanel panel;
    public LevelEventType eventType;
    public int index;

    public ShowPanelScope(InspectorPanel panel, LevelEventType type) : base(false) {
        this.panel = panel;
        if(panel == scnEditor.instance.settingsPanel) {
            eventType = lastPanel1;
            lastPanel1 = type;
        } else {
            eventType = lastPanel2;
            lastPanel2 = type;
        }
        if(eventType == type) SaveStatePatch.undoStates.Remove(this);
        index = panel.cacheEventIndex;
    }

    public override void Undo() {
        LevelEventType curType = eventType;
        ADOBase.editor.cacheSelectedEventIndex = index;
        eventType = panel.selectedEventType;
        index = panel.cacheEventIndex;
        if(curType == LevelEventType.None) panel.HideAllInspectorTabs();
        else panel.ShowPanel(curType);
    }

    public override void Redo() => Undo();
}