using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class ShowPanelScope : CustomSaveStateScope {
    public static LevelEventType lastPanel1;
    public static LevelEventType lastPanel2;
    public InspectorPanel panel;
    public LevelEventType eventType;

    public ShowPanelScope(InspectorPanel panel, LevelEventType type) : base(false) {
        this.panel = panel;
        if(panel == scnEditor.instance.settingsPanel) {
            eventType = lastPanel1;
            lastPanel1 = type;
        } else {
            eventType = lastPanel2;
            lastPanel2 = type;
        }
    }

    public override void Undo() {
        LevelEventType curType = eventType;
        eventType = panel.selectedEventType;
        if(curType == LevelEventType.None) panel.HideAllInspectorTabs();
        else panel.ShowPanel(curType);
    }

    public override void Redo() => Undo();
}