using ADOFAI;
using UnityEngine;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class DecorationLocationChangeScope : CustomSaveStateScope {
    public DecorationCache[] decorations;

    public DecorationLocationChangeScope() : base(false) {
        decorations = new DecorationCache[scnEditor.instance.selectedDecorations.Count];
        for(int i = 0; i < decorations.Length; i++) {
            LevelEvent decoration = scnEditor.instance.selectedDecorations[i];
            decorations[i] = new DecorationCache(decoration, (Vector2) decoration["position"]);
        }
    }

    public override void Undo() {
        foreach(DecorationCache cache in decorations) {
            Vector2 temp = cache.location;
            cache.location = (Vector2) cache.decoration["position"];
            cache.decoration["position"] = temp;
            scrDecoration decoration = scrDecorationManager.GetDecoration(cache.decoration);
            decoration.UpdatePosition();
        }
    }

    public override void Redo() => Undo();

    public class DecorationCache(LevelEvent decoration, Vector2 location) {
        public LevelEvent decoration = decoration;
        public Vector2 location = location;
    }
}