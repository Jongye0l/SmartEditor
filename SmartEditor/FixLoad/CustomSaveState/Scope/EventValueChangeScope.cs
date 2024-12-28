using ADOFAI;
using ADOFAI.LevelEditor.Controls;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class EventValueChangeScope : CustomSaveStateScope {
    public LevelEvent @event;
    public string key;
    public object value;

    public EventValueChangeScope(PropertyControl control) : base(false) {
        @event = control.propertiesPanel.inspectorPanel.selectedEvent;
        key = control.propertyInfo.name;
        value = @event[key];
    }

    public override void Undo() {
        (@event[key], value) = (value, @event[key]);
        scnEditor.instance.ApplyEventsToFloors();
    }

    public override void Redo() => Undo();
}