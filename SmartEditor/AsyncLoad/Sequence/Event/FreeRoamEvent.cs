using System.Collections.Generic;
using ADOFAI;
using DG.Tweening;
using JALib.Tools;
using SmartEditor.AsyncLoad.MainThreadEvent;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence.Event;

public class FreeRoamEvent : LoadSequence {
    public SetupEvent setupEvent;

    public FreeRoamEvent(SetupEvent setupEvent) {
        this.setupEvent = setupEvent;
        JATask.Run(Main.Instance, ApplyEvent);
    }

    public void ApplyEvent() {
        List<LevelEvent>[] floorEvents = setupEvent.floorEvents;
        List<scrFloor> floors = scrLevelMaker.instance.listFloors;
        string text = Main.Instance.Localization["AsyncMapLoad.ApplyEvent4"];
        setupEvent.setupEventMainThread.AddEvent(new ResetFreeFoam());
        int freeroamRegion = 0;
        foreach(scrFloor floor in floors) {
            if(!floor.nextfloor) continue;
            SequenceText = string.Format(text, floor.seqID, floors.Count - 1);
            List<LevelEvent> levelEventList = floorEvents[floor.seqID];
            List<LevelEvent> freeraomEvents = null;
            if(levelEventList == null) continue;
            foreach(LevelEvent levelEvent in levelEventList) {
                if(levelEvent.eventType is < LevelEventType.FreeRoam or > LevelEventType.FreeRoamWarning) continue;
                if(levelEvent.eventType == LevelEventType.FreeRoam) {
                    if(levelEvent.GetInt("duration") < 2) continue;
                    floor.freeroamRegion = freeroamRegion++;
                    floor.freeroam = true;
                    floor.freeroamDimensions = (Vector2) levelEvent["size"];
                    floor.freeroamOffset = (Vector2) levelEvent["positionOffset"];
                    int duration = levelEvent.GetInt("duration");
                    int outTime = levelEvent.GetInt("outTime");
                    if(outTime > duration - 1) outTime = duration - 1;
                    floor.freeroamEndEarlyBeats = outTime;
                    floor.freeroamEndEase = (Ease) levelEvent.data["outEase"];
                    if(levelEvent.data.TryGetValue("hitsoundOnBeats", out object value)) floor.freeroamSoundOnBeat = (HitSound) value;
                    if(levelEvent.data.TryGetValue("hitsoundOffBeats", out value)) floor.freeroamSoundOffBeat = (HitSound) value;
                    floor.SetOpacity(scrController.instance.paused ? 0.1f : 0.0f);
                    floor.opacityVal = 0.0f;
                }
                freeraomEvents ??= [];
                freeraomEvents.Add(levelEvent);
            }
            if(freeraomEvents != null) setupEvent.setupEventMainThread.AddEvent(new ApplyFreeRoamEvent(floor, freeraomEvents));
        }
        Dispose();
    }

    public override void Dispose() {
        base.Dispose();
        setupEvent.freeRoamEvent = null;
        setupEvent.UpdateDispose();
    }
}