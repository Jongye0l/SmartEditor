using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADOFAI;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence.Event;

public class EventIcon : LoadSequence {
    public SetupEvent setupEvent;
    public CoreEvent coreEvent;
    public int cur;
    public int holdLength;

    public EventIcon(SetupEvent setupEvent) {
        this.setupEvent = setupEvent;
        coreEvent = setupEvent.coreEvent;
        Task.Run(ApplyEvent);
    }

    public void ApplyEvent() {
        try {
            List<LevelEvent>[] floorEvents = setupEvent.floorEvents;
            List<scrFloor> floors = scrLevelMaker.instance.listFloors;
            List<float> floorAngles = scnGame.instance.levelData.angleData;
            string text = Main.Instance.Localization["AsyncMapLoad.ApplyEvent5"];
Restart:
            for(; cur < coreEvent.cur; cur++) {
                SequenceText = string.Format(text, cur, floorAngles.Count);
                scrFloor floor = floors[cur];
                FloorIcon floorIcon = FloorIcon.None;
                List<LevelEvent> collection = floorEvents[floor.seqID];
                List<LevelEvent> source = collection.Where(CheckActive).ToList();
                if(floor.seqID > 0) source.AddRange(floorEvents[floor.seqID - 1].Where(e => e.active && e.eventType == LevelEventType.SetSpeed));
                bool comment = false;
                floor.usedCustomFloorIcon = false;
                if(source.Count > 0) {
                    comment = source.Any(e => e.eventType == LevelEventType.EditorComment);
                    floorIcon = FloorIcon.Vfx;
                    floor.eventIcon = LevelEventType.None;
                    bool flag14 = false;
                    int priority = 0;
                    LevelEventType levelEventType = collection.Any(CheckActive) ? source[0].eventType : LevelEventType.AddDecoration;
                    LevelEventType filteredEvent = GCS.filteredEvent;
                    bool flag15 = filteredEvent != LevelEventType.None && scrController.instance.paused;
                    bool filterIcon = false;
                    LevelEvent levelEvent1 = collection.Find(e => e.active && e.eventType == LevelEventType.SetFloorIcon);
                    if(levelEvent1 != null) {
                        floor.usedCustomFloorIcon = true;
                        floor.floorIcon = (FloorIcon) Enum.Parse(typeof(FloorIcon), ((CustomFloorIcon) levelEvent1.data["icon"]).ToString());
                    } else {
                        foreach(LevelEvent levelEvent2 in source) {
                            if(!levelEvent2.active) continue;
                            LevelEventType eventType = levelEvent2.eventType;
                            if(eventType == filteredEvent & flag15) filterIcon = true;
                            if(eventType == LevelEventType.Checkpoint) {
                                if(priority < 1) {
                                    priority = 1;
                                    floorIcon = FloorIcon.Checkpoint;
                                    flag14 = true;
                                } else continue;
                            }
                            if(eventType == LevelEventType.SetSpeed) {
                                if(priority < 2) {
                                    priority = 2;
                                    float num22 = floor.seqID <= 0 ? 1f : floors[floor.seqID - 1].speed;
                                    float f = (floor.speed - num22) / num22;
                                    float num23 = Mathf.Abs(f);
                                    if(num23 > 0.05000000074505806)
                                        floorIcon = f > 0.0                           ? num23 < 1.0499999523162842 ? FloorIcon.Rabbit : FloorIcon.DoubleRabbit :
                                                    1.0 - num23 > 0.44999998807907104 ? FloorIcon.Snail : FloorIcon.DoubleSnail;
                                    else if(levelEvent2.floor == floor.seqID) {
                                        floorIcon = FloorIcon.SameSpeed;
                                        priority = 0;
                                    }
                                    if(levelEvent2.floor == floor.seqID) flag14 = true;
                                } else continue;
                            } else if(eventType == LevelEventType.Twirl) {
                                if(priority < 2) {
                                    priority = 2;
                                    floorIcon = FloorIcon.Swirl;
                                    flag14 = true;
                                } else continue;
                            } else if(levelEvent2.eventType == LevelEventType.Hold) {
                                if(priority < 2) {
                                    floorIcon = floor.holdLength != 0 ? FloorIcon.HoldArrowLong : FloorIcon.HoldArrowShort;
                                    flag14 = true;
                                } else continue;
                            } else if(levelEvent2.eventType == LevelEventType.MultiPlanet) {
                                if(priority < 2) {
                                    int num24 = floor.seqID <= 0 ? 1 : floors[floor.seqID - 1].numPlanets;
                                    float numPlanets = floor.numPlanets;
                                    if(numPlanets == 2.0) floorIcon = FloorIcon.MultiPlanetTwo;
                                    else if(numPlanets > num24) floorIcon = FloorIcon.MultiPlanetThreeMore;
                                    else if(numPlanets <= num24) floorIcon = FloorIcon.MultiPlanetThreeLess;
                                    flag14 = true;
                                } else continue;
                            } else if(!flag14 && eventType != levelEventType && priority == 0 && levelEvent2.floor == floor.seqID)
                                flag14 = true;
                            if(priority == 2) break;
                        }
                    }
                    if(!flag14) {
                        if(collection.Any(CheckActive)) {
                            floorIcon = FloorIcon.Vfx;
                            floor.eventIcon = levelEventType;
                        } else if(floorIcon == FloorIcon.Vfx) floorIcon = FloorIcon.None;
                    }
                    if(flag15) {
                        if(filterIcon) {
                            floorIcon = FloorIcon.Vfx;
                            floor.eventIcon = filteredEvent;
                        } else floorIcon = FloorIcon.None;
                    }
                }
                if(holdLength > 0 && floorIcon is FloorIcon.Vfx or FloorIcon.None)
                    floorIcon = holdLength == 1 ? FloorIcon.HoldReleaseShort : FloorIcon.HoldReleaseLong;
                holdLength = 0;
                if(source.Exists(x => x.eventType == LevelEventType.Hold))
                    holdLength = floor.holdLength == 0 ? 1 : 2;
                if(!floor.usedCustomFloorIcon) floor.floorIcon = floorIcon;
                setupEvent.setupEventMainThread.AddRequestSprite(comment);
            }
            if(cur <= coreEvent.cur) goto Restart;
            if(cur >= floorAngles.Count) Dispose();
            else {
                SequenceText = string.Format(text, cur, floorAngles.Count);
                Task.Yield().GetAwaiter().OnCompleted(ApplyEvent);
            }
        } catch (Exception e) {
            Main.Instance.LogReportException("EventIcon Setup Fail", e);
        }
    }

    private static bool CheckActive(LevelEvent e) => e.active;

    public override void Dispose() {
        base.Dispose();
        setupEvent.eventIcon = null;
        setupEvent.UpdateDispose();
    }
}