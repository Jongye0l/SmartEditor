using System.Collections.Generic;
using System.Reflection;
using ADOFAI;
using ADOFAI.LevelEditor.Controls;
using JALib.Tools;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class DecoDragScope : CustomSaveStateScope {
    public static FieldInfo cachedDecorations = typeof(PropertyControl_DecorationsList).Field("cachedDecorations");
    public LevelEvent[] decoration;
    public int index;

    public DecoDragScope(PropertyControl_List list) : base(false, true) {
        if(list is PropertyControl_DecorationsList decoList) {
            List<scrDecoration> decorations = cachedDecorations.GetValue<List<scrDecoration>>(decoList);
            decoration = new LevelEvent[decorations.Count];
            if(decoration.Length == 0) return;
            for(int i = 0; i < decoration.Length; i++) decoration[i] = decorations[i].sourceLevelEvent;
            index = scnEditor.instance.decorations.IndexOf(decoration[0]);
        }
    }

    public override void Undo() {
        if(decoration == null || decoration.Length == 0) return;
        DecorationsArray<LevelEvent> decorations = scnEditor.instance.decorations;
        int currentIndex = decorations.IndexOf(decoration[0]);
        scrDecoration[] scrDecorations = new scrDecoration[decoration.Length];
        List<scrDecoration> allDecorations = scrDecorationManager.instance.allDecorations;
        int i = currentIndex;
        foreach(LevelEvent levelEvent in decoration) {
            decorations.Remove(levelEvent);
            scrDecorations[i - currentIndex] = allDecorations[i];
            allDecorations.RemoveAt(i++);
        }
        decorations.InsertRange(index, decoration);
        allDecorations.InsertRange(index, scrDecorations);
        scnEditor.instance.propertyControlDecorationsList.lastSelectedIndex = index;
        index = currentIndex;
        scnEditor.instance.propertyControlDecorationsList.OnDecorationUpdate();
    }

    public override void Redo() => Undo();
}