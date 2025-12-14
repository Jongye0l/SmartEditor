using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using UnityEngine;

namespace SmartEditor;

public static class Utility {
    public static double GetAngle(scrFloor prevFloor) {
        scrFloor curFloor = prevFloor.nextfloor;
        double prevAngle = prevFloor.floatDirection;
        if(prevAngle == 999) prevAngle = prevFloor.prevfloor.floatDirection + 180;
        double curAngle = curFloor.floatDirection;
        if(curAngle == 999) {
            if(!curFloor.nextfloor) return 0;
            curAngle = curFloor.nextfloor.floatDirection + 180;
        }
        return NormalizeAngle((180 + prevAngle - curAngle) * (curFloor.isCCW ? -1 : 1));
    }

    public static double NormalizeAngle(double angle) {
        angle = Math.Round(angle, 4);
        if(angle == 999) return angle;
        while(angle < 0) angle += 360;
        angle %= 360;
        return angle == 0 ? 360 : angle;
    }

    public static float FindCurrentSpeed(int floor) {
        List<(float, LevelEvent)> speedEvents = [];
        foreach(LevelEvent levelEvent in ADOBase.customLevel.events) {
            if(levelEvent.eventType != LevelEventType.SetSpeed || levelEvent.floor >= floor) continue;
            int start = 0;
            int end = speedEvents.Count - 1;
            float angleOffset = levelEvent.GetFloat("angleOffset");
            while(start <= end) {
                int mid = (start + end) / 2;
                int midFloor = speedEvents[mid].Item2.floor;
                if(midFloor < levelEvent.floor) start = mid + 1;
                else if(midFloor != levelEvent.floor) end = mid - 1;
                else {
                    float midAngleOffset = speedEvents[mid].Item1;
                    if(angleOffset < midAngleOffset) end = mid - 1;
                    else start = mid + 1;
                }
            }
            if((SpeedType) levelEvent["speedType"] == SpeedType.Bpm) {
                speedEvents.RemoveRange(0, start - 1);
                start = 0;
            }
            speedEvents.Insert(start, (angleOffset, levelEvent));
        }
        float speed = 1f;
        foreach((float _, LevelEvent levelEvent) in speedEvents) {
            if((SpeedType) levelEvent["speedType"] == SpeedType.Bpm)
                speed = levelEvent.GetFloat("beatsPerMinute") / ADOBase.customLevel.levelData.bpm;
            else speed *= levelEvent.GetFloat("bpmMultiplier");
        }
        return speed;
    }

    public static FloorIcon SpeedIconChangeState(scrFloor floor) {
        float num22 = floor.seqID == 0 ? 1f : floor.prevfloor.speed;
        float f = (floor.speed - num22) / num22;
        float num23 = Mathf.Abs(f);
        if(num23 > 0.05000000074505806f)
            return f > 0.0 ? num23 < 1.0499999523162842f ? FloorIcon.Rabbit : FloorIcon.DoubleRabbit : 1 - num23 > 0.44999998807907104f ? FloorIcon.Snail : FloorIcon.DoubleSnail;
        return FloorIcon.None;
    }

    public static void SetupIcon(scrFloor floor) {
        int num19 = 0;
        if(floor.prevfloor && floor.prevfloor.holdDistance > 0) num19 = floor.holdLength == 0 ? 1 : 2;
        FloorIcon floorIcon = FloorIcon.None;
        List<LevelEvent> source = scnGame.instance.events.Where(e => e.floor == floor.seqID || e.eventType == LevelEventType.SetSpeed && e.floor == floor.seqID - 1).ToList();
        floor.usedCustomFloorIcon = false;
        if(source.Count > 0) {
            floorIcon = FloorIcon.Vfx;
            floor.eventIcon = LevelEventType.None;
            bool flag14 = false;
            int num20 = 0;
            LevelEventType levelEventType = source[0].eventType;
            LevelEventType filteredEvent = GCS.filteredEvent;
            bool flag15 = filteredEvent != LevelEventType.None && scrController.instance.paused;
            bool flag16 = false;
            LevelEvent levelEvent1 = source.Find(e => e.eventType == LevelEventType.SetFloorIcon);
            if(levelEvent1 != null) {
                floor.usedCustomFloorIcon = true;
                floor.floorIcon = (FloorIcon) Enum.Parse(typeof(FloorIcon), ((CustomFloorIcon) levelEvent1.data["icon"]).ToString());
            } else {
                foreach(LevelEvent levelEvent2 in source) {
                    if(levelEvent2.active) {
                        LevelEventType eventType = levelEvent2.eventType;
                        if(eventType == filteredEvent & flag15)
                            flag16 = true;
                        if(eventType == LevelEventType.Checkpoint) {
                            if(num20 < 1) {
                                num20 = 1;
                                floorIcon = FloorIcon.Checkpoint;
                                flag14 = true;
                            } else continue;
                        }
                        if(eventType == LevelEventType.SetSpeed) {
                            float num22 = floor.seqID <= 0 ? 1f : floor.prevfloor.speed;
                            float f = (floor.speed - num22) / num22;
                            float num23 = Mathf.Abs(f);
                            if(num23 > 0.05000000074505806f)
                                floorIcon = f > 0                            ? num23 < 1.0499999523162842f ? FloorIcon.Rabbit : FloorIcon.DoubleRabbit : 
                                            1 - num23 > 0.44999998807907104f ? FloorIcon.Snail : FloorIcon.DoubleSnail;
                            else if(levelEvent2.floor == floor.seqID) {
                                floorIcon = FloorIcon.SameSpeed;
                                flag14 = true;
                                continue;
                            }
                            if(levelEvent2.floor == floor.seqID) flag14 = true;
                            break;
                        }
                        if(eventType == LevelEventType.Twirl) {
                            floorIcon = FloorIcon.Swirl;
                            flag14 = true;
                            break;
                        }
                        if(levelEvent2.eventType == LevelEventType.Hold) {
                            floorIcon = floor.holdLength != 0 ? FloorIcon.HoldArrowLong : FloorIcon.HoldArrowShort;
                            flag14 = true;
                        } else if(levelEvent2.eventType == LevelEventType.MultiPlanet) {
                            int num24 = floor.seqID <= 0 ? 1 : floor.prevfloor.numPlanets;
                            float numPlanets = floor.numPlanets;
                            if(numPlanets == 2.0) floorIcon = FloorIcon.MultiPlanetTwo;
                            else if(numPlanets > num24) floorIcon = FloorIcon.MultiPlanetThreeMore;
                            else if(numPlanets <= num24) floorIcon = FloorIcon.MultiPlanetThreeLess;
                            flag14 = true;
                        } else if(!flag14 && eventType != levelEventType && num20 == 0 && levelEvent2.floor == floor.seqID)
                            flag14 = true;
                    }
                }
            }
            if(!flag14) {
                if(source.Count > 0) {
                    floorIcon = FloorIcon.Vfx;
                    floor.eventIcon = levelEventType;
                } else if(floorIcon == FloorIcon.Vfx) floorIcon = FloorIcon.None;
            }
            if(flag15) {
                if(flag16) {
                    floorIcon = FloorIcon.Vfx;
                    floor.eventIcon = filteredEvent;
                } else floorIcon = FloorIcon.None;
            }
        }
        if(num19 > 0 && floorIcon is FloorIcon.Vfx or FloorIcon.None) 
            floorIcon = num19 == 1 ? FloorIcon.HoldReleaseShort : FloorIcon.HoldReleaseLong;
        if(!floor.usedCustomFloorIcon) floor.floorIcon = floorIcon;
        floor.UpdateIconSprite();
    }
}