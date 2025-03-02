using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class SpeedPauseConvertScope(LevelEvent oldEvent) : CustomSaveStateScope(false, true) {
    public LevelEvent oldEvent = oldEvent;
    public LevelEvent levelEvent;

    public override void Undo() {
        if(levelEvent == null) return;
        scnEditor.instance.events.Remove(levelEvent);
        scnEditor.instance.events.Add(oldEvent);
        (oldEvent, levelEvent) = (levelEvent, oldEvent);
        scnEditor editor = scnEditor.instance;
        editor.levelEventsPanel.selectedEventType = levelEvent.eventType;
        editor.DecideInspectorTabsAtSelected();
        editor.levelEventsPanel.ShowPanel(levelEvent.eventType);
        editor.ApplyEventsToFloors();
        editor.ShowEventIndicators(editor.selectedFloors[0]);
    }

    public override void Redo() => Undo();
}