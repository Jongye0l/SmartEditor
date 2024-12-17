using ADOFAI;
using UnityEngine;

namespace SmartEditor.LevelEvent;

public class DecorationEvent : CustomEvent {
    public Vector2 position;
    public DecPlacementType relativeTo;
    public Vector2 pivotOffset;
    public float rotation;
    public Vector2 scale = new(100, 100);
    public int depth = -1;
    public Vector2 parallax;
    public Vector2 parallaxOffset;
    public string tag = "";

    public DecorationEvent(int newFloor, LevelEventType type, LevelEventInfo info) : base(newFloor, type, info) {

    }
}