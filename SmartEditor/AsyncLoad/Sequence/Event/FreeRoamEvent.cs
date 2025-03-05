using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADOFAI;
using DG.Tweening;
using JALib.Tools;
using SmartEditor.AsyncLoad.MainThreadEvent;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence.Event;

public class FreeRoamEvent : LoadSequence {
    public SetupEvent setupEvent;
    public int cur;
    public int freeroamRegion;

    public FreeRoamEvent(SetupEvent setupEvent) {
        this.setupEvent = setupEvent;
        ResetFreeFoam resetFreeFoam = new();
        setupEvent.setupEventMainThread.AddEvent(resetFreeFoam);
        resetFreeFoam.tcs.Task.GetAwaiter().UnsafeOnCompleted(ApplyEvent);
    }

    public void ApplyEvent() {
        try {
            List<LevelEvent>[] floorEvents = setupEvent.floorEvents;
            List<scrFloor> floors = scrLevelMaker.instance.listFloors;
            List<float> floorAngles = scnGame.instance.levelData.angleData;
            string text = Main.Instance.Localization["AsyncMapLoad.ApplyEvent4"];
            for(; cur < floors.Count && cur != floorAngles.Count; cur++) {
                SequenceText = string.Format(text, cur, floorAngles.Count);
                scrFloor floor = floors[cur];
                List<LevelEvent> levelEventList = floorEvents[cur];
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
            if(cur >= floorAngles.Count) Dispose();
            else {
                SequenceText = string.Format(text, cur, floorAngles.Count);
                Task.Yield().GetAwaiter().UnsafeOnCompleted(ApplyEvent);
            }
        } catch (Exception e) {
            Main.Instance.LogReportException(e);
        }
    }

    public override void Dispose() {
        base.Dispose();
        setupEvent.freeRoamEvent = null;
        setupEvent.UpdateDispose();
    }
}