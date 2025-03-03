using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using JALib.Tools;
using SmartEditor.AsyncLoad.MainThreadEvent;

namespace SmartEditor.AsyncLoad.Sequence.Event;

public class CoreEvent : LoadSequence {
    public SetupEvent setupEvent;
    public int cur;
    public bool running;
    public float speed;
    public bool isCCW;
    public float scale;
    public int planets;
    public float tileWidth;
    public float tileLength;

    public CoreEvent(SetupEvent setupEvent) {
        this.setupEvent = setupEvent;
        SequenceText = Main.Instance.Localization["AsyncMapLoad.ApplyEvent"];
        LevelData levelData = scnGame.instance.levelData;
        scrConductor.instance.countdownTicks = levelData.countdownTicks;
        speed = 1f;
        isCCW = false;
        scale = 1f;
        planets = ADOBase.controller.planetarySystem.planetsUsed = 2;
        tileWidth = tileLength = 1f;
        LoadEvent();
    }

    public void LoadEvent() {
        lock(this) {
            if(running || cur >= setupEvent.updatedTile) return;
            running = true;
        }
        JATask.Run(Main.Instance, ApplyEvent);
    }

    public void ApplyEvent() {
        List<LevelEvent>[] floorEvents = setupEvent.floorEvents;
        List<scrFloor> floors = scrLevelMaker.instance.listFloors;
        string text = Main.Instance.Localization["AsyncMapLoad.ApplyEvent2"];
        float bpm = scnGame.instance.levelData.bpm;
Restart:
        for(; cur < setupEvent.updatedTile; cur++) {
            scrFloor floor = floors[cur];
            SequenceText = string.Format(text, cur, floors.Count);
            floor.extraBeats = 0.0f;
            List<LevelEvent> floorEvent = floorEvents[cur];
            List<LevelEvent> setSpeedEvent = floorEvent.FindAll(e => e.active && e.eventType == LevelEventType.SetSpeed);
            List<float> speedAngleList = setSpeedEvent.ConvertAll(e => e.GetFloat("angleOffset")).Distinct().ToList();
            speedAngleList.Sort();
            foreach(LevelEvent levelEvent in floorEvent) {
                if(!levelEvent.active) continue;
                object value;
                switch(levelEvent.eventType) {
                    case LevelEventType.Twirl:
                        isCCW = !isCCW;
                        floor.isSwirl = true;
                        continue;
                    case LevelEventType.Checkpoint:
                        int tileOffset = 0;
                        if(levelEvent.data.TryGetValue("tileOffset", out value)) {
                            tileOffset = (int) value;
                            if(cur + tileOffset < 0) tileOffset = -cur;
                            if(cur + tileOffset > floors.Count - 2)
                                tileOffset = floors.Count - 2 - cur;
                        }
                        setupEvent.setupEventMainThread.AddEvent(new AddCheckpoint(floor, tileOffset));
                        continue;
                    case LevelEventType.Hold:
                        int duration = (int) levelEvent.data["duration"];
                        floor.holdLength = !floor.nextfloor || duration < 0 ? -1 : duration;
                        continue;
                    case LevelEventType.MultiPlanet:
                        planets = (int) levelEvent.data["planets"];
                        if(planets < 2) planets = 2;
                        planets = Math.Min(planets, 3);
                        if(floor.prevfloor && floor.prevfloor.midSpin) floor.prevfloor.numPlanets = planets;
                        continue;
                    case LevelEventType.FreeRoam:
                        int durationFreeRoam = levelEvent.GetInt("duration");
                        if(floor.nextfloor && durationFreeRoam >= 2) {
                            floor.extraBeats += durationFreeRoam - 1;
                            floor.countdownTicks = levelEvent.GetInt("countdownTicks");
                            if(levelEvent.data.TryGetValue("angleCorrectionDir", out value)) floor.angleCorrectionType = (int) value;
                            if(levelEvent.data.TryGetValue("hitsoundOnBeats", out value)) floor.freeroamSoundOnBeat = (HitSound) value;
                            if(levelEvent.data.TryGetValue("hitsoundOffBeats", out value)) floor.freeroamSoundOffBeat = (HitSound) value;
                        }
                        continue;
                    case LevelEventType.Pause:
                        if(floor.nextfloor && !floor.midSpin) {
                            floor.extraBeats += levelEvent.GetFloat("duration");
                            floor.countdownTicks = levelEvent.GetInt("countdownTicks");
                            if(!floor.midSpin && Math.Abs(scrMisc.GetAngleMoved(floor.entryangle, floor.exitangle, !floor.isCCW) - 6.2831854820251465) < 0.0001) floor.extraBeats++;
                            if(levelEvent.data.TryGetValue("angleCorrectionDir", out value)) floor.angleCorrectionType = (int) value;
                        }
                        continue;
                    case LevelEventType.ScaleRadius:
                        scale = levelEvent.GetFloat("scale") / 100f;
                        continue;
                    case LevelEventType.Multitap:
                        floor.tapsNeeded = (int) levelEvent.data["taps"];
                        continue;
                    case LevelEventType.TileDimensions:
                        tileLength = levelEvent.GetFloat("length") / 100f;
                        tileWidth = levelEvent.GetFloat("width") / 100f;
                        continue;
                    default:
                        continue;
                }
            }
            bool speedOnlyThisTile = false;
            if(speedAngleList.Count > 0) {
                float num8 = scrMisc.ApproximatelyFloor(floor.entryangle, floor.exitangle) ? 360f : (float) (scrMisc.GetAngleMoved(floor.entryangle, floor.exitangle, !isCCW) * 57.295780181884766);
                float num10 = speedAngleList[0] / 180f * 60f / (bpm * speed);
                foreach(float angle in speedAngleList) {
                    float angleOffset = angle;
                    LevelEvent last = setSpeedEvent.FindLast(e => e.GetFloat("angleOffset") == angleOffset);
                    float num12 = speedAngleList.Find(e => e > angleOffset);
                    if(num12 == 0.0) num12 = num8;
                    float setupBpm;
                    if((SpeedType) last.data["speedType"] == SpeedType.Bpm) setupBpm = last.GetFloat("beatsPerMinute");
                    else {
                        setupBpm = bpm * speed;
                        foreach(LevelEvent levelEvent in setSpeedEvent) {
                            if(levelEvent.GetFloat("angleOffset") != angleOffset) continue;
                            if((SpeedType) levelEvent.data["speedType"] == SpeedType.Bpm) setupBpm = levelEvent.GetFloat("beatsPerMinute");
                            setupBpm *= levelEvent.GetFloat("bpmMultiplier");
                        }
                    }
                    num10 += (float) ((num12 - angleOffset) / 180.0) * 60f / setupBpm;
                    if(angleOffset > 0.0 && angleOffset <= num8) speedOnlyThisTile = true;
                    speed = setupBpm / bpm;
                }
                if(speedOnlyThisTile) floor.speed = (float) (60.0 / num10 * (num8 / 180.0)) / bpm;
            }
            floor.radiusScale = scale;
            if(!speedOnlyThisTile) floor.speed = speed;
            floor.isCCW = isCCW;
            floor.numPlanets = planets;
            floor.lengthMult = tileLength;
            floor.widthMult = tileWidth;
            if(planets > ADOBase.controller.planetarySystem.planetsUsed) ADOBase.controller.planetarySystem.planetsUsed = planets;
            setupEvent.OnCoreEventUpdate();
        }
        lock(this) {
            if(cur < setupEvent.updatedTile) goto Restart;
            running = false;
        }
        if(cur + 1 == floors.Count) Dispose();
        else SequenceText = string.Format(text, cur, floors.Count);
    }

    public override void Dispose() {
        base.Dispose();
        setupEvent.coreEvent = null;
        setupEvent.UpdateDispose();
    }
}