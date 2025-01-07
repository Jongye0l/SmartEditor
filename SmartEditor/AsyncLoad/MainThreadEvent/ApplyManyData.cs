using UnityEngine;

namespace SmartEditor.AsyncLoad.MainThreadEvent;

public class ApplyManyData : ApplyMainThread {
    public scrFloor floor;
    public Vector3 position;
    public Color color;
    public TrackStyle trackStyle;
    public Vector3 scale;
    public float rotation;

    public ApplyManyData(scrFloor floor) {
        this.floor = floor;
    }

    public override void Run() {
        floor.transform.position = position;
        floor.SetColor(color);
        floor.SetTrackStyle(trackStyle, true);
        floor.transform.localScale = scale;
        floor.SetRotation(rotation);
    }
}