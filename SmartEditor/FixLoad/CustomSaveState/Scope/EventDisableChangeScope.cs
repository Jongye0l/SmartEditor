using ADOFAI;
using ADOFAI.LevelEditor.Controls;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class EventDisableChangeScope : CustomSaveStateScope {
    public LevelEvent @event;
    public string key;
    public bool disable;

    public EventDisableChangeScope(PropertyControl control) : base(false) {
        @event = control.propertiesPanel.inspectorPanel.selectedEvent;
        key = control.propertyInfo.name;
        disable = @event.disabled[key];
    }

    public override void Undo() {
        (@event.disabled[key], disable) = (disable, @event.disabled[key]);
        scnEditor.instance.ApplyEventsToFloors();
        scnEditor.instance.levelEventsPanel.ShowPanel(@event.eventType);
    }

    public override void Redo() => Undo();
}