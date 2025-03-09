using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartEditor.AsyncLoad.Sequence.Event;

public class TileEntryTime : LoadSequence {
    public SetupEvent setupEvent;
    public int cur;
    public int max;
    public double entryTime;
    public double entryBeat;

    public TileEntryTime(SetupEvent setupEvent) {
        this.setupEvent = setupEvent;
        scrLevelMaker.instance.highestBPM = 0;
        Task.Run(ApplyEvent);
    }

    public void LoadEvent(int floor) {
        max = floor;
    }

    public void ApplyEvent() {
        try {
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            List<float> floorAngles = scnGame.instance.levelData.angleData;
            List<scrFloor> floors = levelMaker.listFloors;
            string text = Main.Instance.Localization["AsyncMapLoad.TileEntryTime"];
            scrConductor conductor = ADOBase.conductor;
            float pitch = conductor.song.pitch;
            scrFloor floor = floors[cur];
            if(cur == 0) {
                SequenceText = string.Format(text, cur, floorAngles.Count);
                entryTime = conductor.crotchetAtStart * (conductor.adjustedCountdownTicks - 1) + scrMisc.GetTimeBetweenAngles(floor.entryangle, floor.exitangle, floor.speed, conductor.bpm, !floor.isCCW);
                floor.entryTime = 0;
                floor.entryBeat = -1;
                floor = floors[++cur];
                floor.entryTime = entryTime;
                floor.entryTimePitchAdj = entryTime / pitch;
            }
            Restart:
            for(; cur < max; cur++) {
                SequenceText = string.Format(text, cur, floorAngles.Count);
                scrFloor nextFloor = floors[cur + 1];
                double num4 = scrMisc.GetInverseAnglePerBeatMultiplanet(floor.numPlanets) * (floor.isCCW ? -1.0 : 1.0);
                if(floor.midSpin) num4 = 0.0;
                if(floor.prevfloor.midSpin && floor.numPlanets > 2)
                    num4 -= (6.2831854820251465 + scrMisc.GetInverseAnglePerBeatMultiplanet(floor.numPlanets)) * (floor.isCCW ? -1.0 : 1.0);
                double angles = scrMisc.GetTimeBetweenAngles(floor.entryangle + num4, floor.exitangle + (floor.midSpin ? num4 : 0.0), floor.speed, conductor.bpm, !floor.isCCW);
                bool flag = angles <= 1E-06 || angles >= 2.0 * conductor.crotchetAtStart / floor.speed - 1E-06;
                double extra = scrMisc.GetTimeBetweenAngles(0.0, 3.1415927410125732, floor.speed, conductor.bpm, false);
                if(flag) angles = floor.midSpin ? 0.0 : 2.0 * extra;
                double num6 = entryTime + angles;
                if(floor.holdLength > 0) num6 += floor.holdLength * 2 * extra;
                float extraBeats = floor.extraBeats;
                if(extraBeats > 0.0 & flag) --extraBeats;
                nextFloor.entryTime = entryTime = num6 + extraBeats * extra;
                nextFloor.entryTimePitchAdj = entryTime / pitch;
                float curBpm = floor.speed * conductor.bpm;
                if(curBpm > levelMaker.highestBPM) levelMaker.highestBPM = curBpm;
                floor.entryBeat = entryBeat;
                double floorAngleLength = levelMaker.CalculateSingleFloorAngleLength(floor);
                entryBeat += floorAngleLength / Math.PI + floor.extraBeats;
                floor = nextFloor;
            }
            if(cur < max) goto Restart;
            if(max == floorAngles.Count) Dispose();
            else {
                SequenceText = string.Format(text, cur, floorAngles.Count);
                Task.Yield().GetAwaiter().OnCompleted(ApplyEvent);
            }
        } catch (Exception e) {
            Main.Instance.LogReportException("Work ApplyEvent Fail", e);
        }
    }

    public override void Dispose() {
        base.Dispose();
        setupEvent.tileEntryTime = null;
        setupEvent.UpdateDispose();
    }
}