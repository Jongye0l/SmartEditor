using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ADOFAI;
using JALib.Tools;
using UnityEngine;

namespace SmartEditor.FixLoad;

public static class InsertTileUpdate {

    public static void UpdateTile(int floor) {
        try {
            scnGame game = scnGame.instance;
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            levelMaker.leveldata = game.levelData.pathData;
            levelMaker.isOldLevel = game.levelData.isOldLevel;
            MakeLevel(floor); //game.levelMaker.MakeLevel();
            ApplyEventsToFloors(floor); //game.ApplyEventsToFloors(levelMaker.listFloors);
            levelMaker.DrawHolds();
            levelMaker.DrawMultiPlanet();
            FixChartLoad.DrawEditor();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static void MakeLevel(int floor) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        if(levelMaker.isOldLevel) levelMaker.InstantiateStringFloors();
        else InstantiateFloatFloors(floor); //levelMaker.InstantiateFloatFloors();
        levelMaker.lm2 ??= levelMaker.GetComponent<scrLevelMaker2>();
        levelMaker.listFloors[floor].UpdateAngle();
        scrFloor cur = levelMaker.listFloors[floor + 1];
        cur.styleNum = 0;
        cur.SetTileColor(levelMaker.lm2.tilecolor);
        if(cur.isportal) {
            levelMaker.listFloors[floor].UpdateIconSprite();
            cur.UpdateIconSprite();
        }
        for(int index = 0; index < levelMaker.listFloors.Count; ++index) {
            scrFloor listFloor = levelMaker.listFloors[index];
            listFloor.SetSortingOrder((100 + levelMaker.listFloors.Count - index) * 5);
            if(index < floor) continue;
            listFloor.startRot = listFloor.transform.rotation.eulerAngles;
            listFloor.tweenRot = listFloor.startRot;
            listFloor.offsetPos = Vector3.zero;
        }
    }

    public static void InstantiateFloatFloors(int floor) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        Transform floorsTransform = GameObject.Find("Floors").transform;
        List<float> floorAngles = scnGame.instance.levelData.angleData;
        ADOBase.conductor.onBeats.Clear();
        scrFloor prevFloor = levelMaker.listFloors[floor];
        float prevFloorAngle = floorAngles[floor];
        double prevAngle = prevFloorAngle == 999.0 ? prevFloor.entryangle : (-prevFloorAngle + 90.0) * (Math.PI / 180.0);
        prevFloor.exitangle = prevAngle;
        Vector3 startAddedPos = scrMisc.getVectorFromAngle(prevAngle, FixChartLoad.GetRadius());
        Vector3 addedPos = startAddedPos * prevFloor.radiusScale;
        GameObject newFloorObj = UnityEngine.Object.Instantiate(levelMaker.meshFloor, prevFloor.transform.position + addedPos, Quaternion.identity);
        newFloorObj.transform.parent = floorsTransform;
        scrFloor curFloor = newFloorObj.GetComponent<scrFloor>();
        levelMaker.listFloors.Insert(floor + 1, curFloor);
        prevFloor.nextfloor = curFloor;
        curFloor.Reset();
        curFloor.seqID = floor + 1;
        curFloor.floatDirection = prevFloorAngle;
        curFloor.entryangle = (prevAngle + 3.1415927410125732) % 6.2831854820251465;
        curFloor.startPos = prevFloor.startPos + startAddedPos;
        if(prevFloor.midSpin) {
            prevFloor.midSpin = false;
            curFloor.midSpin = true;
        }
        if(prevFloorAngle == 999.0) prevFloor.midSpin = true;
        if(floor + 1 == floorAngles.Count) {
            prevFloor.isportal = false;
            foreach(scrPortalParticles component in prevFloor.GetComponentsInChildren<scrPortalParticles>()) {
                component.transform.parent = curFloor.transform;
                component.Invoke("Start");
            }
            curFloor.isportal = true;
            curFloor.levelnumber = Portal.EndOfLevel;
            curFloor.exitangle = curFloor.entryangle + 3.1415927410125732;
        } else {
            float nextFloorAngle = floorAngles[floor + 1];
            double nextAngle = curFloor.midSpin ? curFloor.entryangle : (-nextFloorAngle + 90.0) * (Math.PI / 180.0);
            curFloor.exitangle = nextAngle;
            scrFloor nextFloor = levelMaker.listFloors[floor + 2];
            curFloor.nextfloor = nextFloor;
            if(curFloor.midSpin) nextFloor.entryangle = (nextAngle + 3.1415927410125732) % 6.2831854820251465;
            else {
                addedPos = curFloor.transform.position - prevFloor.transform.position;
                startAddedPos = curFloor.startPos - prevFloor.startPos;
            }
            for(int i = floor + 2; i < levelMaker.listFloors.Count; i++) {
                scrFloor fl = levelMaker.listFloors[i];
                fl.seqID = i;
                fl.editorNumText.letterText.text = i.ToString();
                fl.transform.position += addedPos;
                fl.startPos += startAddedPos;
            }
        }
    }

    public static void ApplyEventsToFloors(int floor) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        scrFloor prevFloor = levelMaker.listFloors[floor];
        scrFloor curFloor = levelMaker.listFloors[floor + 1];

        // ApplyCoreEventsToFloors
        curFloor.isCCW = prevFloor.isCCW;
        curFloor.numPlanets = prevFloor.numPlanets;
        // curFloor.lengthMult = 1; // pro events
        // curFloor.widthMult = 1; // pro events
        curFloor.radiusScale = prevFloor.radiusScale;
        bool isJustThisTile = false;
        LevelEvent prevColorEvent = null;
        LevelEvent prevOnlyColorEvent = null;
        LevelEvent nextColorEvent = null;
        List<(float, LevelEvent)> prevSpeedEvents = [];
        List<(float, LevelEvent)> speedEvents = [];

        foreach(LevelEvent levelEvent in ADOBase.customLevel.events) {
            switch(levelEvent.eventType) {
                case LevelEventType.SetSpeed:
                    if(levelEvent.floor - floor > 1) continue;
                    List<(float, LevelEvent)> curSpeedEvents = levelEvent.floor == floor ? speedEvents : prevSpeedEvents;
                    int start = 0;
                    int end = curSpeedEvents.Count - 1;
                    float angleOffset = levelEvent.GetFloat("angleOffset");
                    while(start <= end) {
                        int mid = (start + end) / 2;
                        float midAngleOffset = curSpeedEvents[mid].Item1;
                        if(angleOffset < midAngleOffset) end = mid - 1;
                        else start = mid + 1;
                    }
                    curSpeedEvents.Insert(start, (angleOffset, levelEvent));
                    break;
                case LevelEventType.ColorTrack:
                    if(levelEvent.data.Keys.Contains("justThisTile") && levelEvent.GetBool("justThisTile")) {
                        if(levelEvent.floor == floor) isJustThisTile = true;
                        break;
                    }
                    if(levelEvent.floor <= floor)
                        if(prevOnlyColorEvent == null || levelEvent.floor <= floor && levelEvent.floor >= prevOnlyColorEvent.floor)
                            prevOnlyColorEvent = levelEvent;
                    goto case LevelEventType.ChangeTrack;
                case LevelEventType.ChangeTrack:
                    if(levelEvent.floor <= floor) {
                        if(prevColorEvent == null || levelEvent.floor <= floor && levelEvent.floor >= prevColorEvent.floor)
                            prevColorEvent = levelEvent;
                    } else {
                        if(nextColorEvent == null || levelEvent.floor >= floor && levelEvent.floor <= nextColorEvent.floor)
                            nextColorEvent = levelEvent;
                    }
                    break;
            }
        }
        bool needUpdateIcon = false;
        LevelData levelData = ADOBase.customLevel.levelData;

        if(speedEvents.TrueForAll(ev => ev.Item1 == 0)) curFloor.speed = prevFloor.speed;
        else {
            float prevSpeed;
            bool flag1 = false;
            float bpm = levelData.bpm;
            float totalAngle = scrMisc.ApproximatelyFloor(prevFloor.entryangle, prevFloor.exitangle) ? 360f : (float) (scrMisc.GetAngleMoved(prevFloor.entryangle, prevFloor.exitangle, prevFloor.isCCW) * 57.295780181884766);
            if(floor == 0) prevSpeed = 1;
            else if(prevSpeedEvents.TrueForAll(ev => ev.Item1 == 0)) prevSpeed = levelMaker.listFloors[floor - 1].speed;
            else if(prevSpeedEvents.Any(ev => (SpeedType) ev.Item2["speedType"] == SpeedType.Bpm)) {
                float curBpm = -1;
                foreach((float _, LevelEvent ev) in prevSpeedEvents) {
                    if((SpeedType) ev["speedType"] == SpeedType.Bpm) curBpm = ev.GetFloat("beatsPerMinute");
                    else curBpm *= ev.GetFloat("bpmMultiplier");
                }
                prevSpeed = curBpm / bpm;
            } else prevSpeed = Utility.FindCurrentSpeed(floor);
            float currentSpeed = speedEvents[0].Item1 == 0 ? 0 : speedEvents[0].Item1 / 180f * 60f / (bpm * prevSpeed);
            float currentBpm = -1;
            for(int i = 0; i < speedEvents.Count; i++) {
                (float angleOffset, LevelEvent levelEvent) = speedEvents[i];
                float num10 = i + 1 < speedEvents.Count ? speedEvents[i + 1].Item1 : totalAngle;
                if((SpeedType) levelEvent["speedType"] == SpeedType.Bpm) currentBpm = levelEvent.GetFloat("beatsPerMinute");
                else if(currentBpm == -1) currentBpm = bpm * prevSpeed * levelEvent.GetFloat("bpmMultiplier");
                else currentBpm *= levelEvent.GetFloat("bpmMultiplier");
                if(num10 == 0) continue;
                currentSpeed += (num10 - angleOffset) / 180 * (60 / currentBpm);
                if(angleOffset > 0 && angleOffset <= totalAngle) flag1 = true;
                prevSpeed = currentBpm / bpm;
            }
            curFloor.speed = flag1 ? (float) (60.0 / currentSpeed * (totalAngle / 180.0)) / bpm : prevSpeed;
            levelMaker.listFloors[floor + 2].UpdateIconSprite();
            needUpdateIcon = true;
        }

        // ApplyEventsToFloors
        //scrLevelMaker.instance.CalculateFloorEntryTimes();
        CalculateFloorEntryTimes(floor + 1);
        curFloor.numPlanets = prevFloor.numPlanets;
        curFloor.isSafe = prevFloor.isSafe;
        curFloor.auto = prevFloor.auto;
        curFloor.showStatusText = prevFloor.showStatusText;
        curFloor.hideJudgment = prevFloor.hideJudgment;
        curFloor.hideIcon = prevFloor.hideIcon;
        curFloor.marginScale = prevFloor.marginScale;
        curFloor.planetEase = prevFloor.planetEase;
        curFloor.planetEaseParts = prevFloor.planetEaseParts;
        curFloor.planetEasePartBehavior = prevFloor.planetEasePartBehavior;
        curFloor.stickToFloor = prevFloor.stickToFloor;
        curFloor.customTexture = prevFloor.customTexture;
        curFloor.customTextureScale = prevFloor.customTextureScale;

        ffxChangeTrack prevFloorTrack = prevFloor.GetComponent<ffxChangeTrack>();
        ffxChangeTrack curFloorTrack = curFloor.GetOrAddComponent<ffxChangeTrack>();
        if(isJustThisTile) {
            if(prevColorEvent != null) {
                curFloorTrack.color1 = ((string) prevColorEvent["trackColor"]).HexToColor();
                curFloorTrack.color2 = Convert.ToString(prevColorEvent["secondaryTrackColor"]).HexToColor();
                curFloorTrack.colorAnimDuration = prevColorEvent.GetFloat("trackColorAnimDuration");
                curFloorTrack.colorType = (TrackColorType) prevColorEvent["trackColorType"];
                curFloorTrack.pulseType = (TrackColorPulse) prevColorEvent["trackColorPulse"];
                curFloorTrack.pulseLength = (int) prevColorEvent["trackPulseLength"];
                curFloorTrack.startOfColorChange = prevColorEvent.floor;
                curFloor.outline = levelData.floorIconOutlines;
                if(prevColorEvent.eventType == LevelEventType.ColorTrack) {
                    if(!prevColorEvent.disabled["floorIconOutlines"]) curFloor.outline = prevColorEvent.GetBool("floorIconOutlines");
                    string str = prevColorEvent["trackTexture"] as string;
                    if(!string.IsNullOrEmpty(str)) {
                        string filePath = Path.Combine(Path.GetDirectoryName(ADOBase.customLevel.levelPath), str);
                        Texture2D tempTexture = scnGame.instance.imgHolder.AddTexture(str, out LoadResult _, filePath)?.GetTexture(TextureManager.ImageOptions.None);
                        if(tempTexture) tempTexture.wrapMode = TextureWrapMode.Repeat;
                        curFloorTrack.texture = tempTexture;
                    }
                    curFloor.customTextureScale = prevColorEvent.GetFloat("trackTextureScale");

                    curFloor.styleNum = (int) prevColorEvent["trackStyle"];
                    curFloor.glowMultiplier = prevColorEvent.GetFloat("trackGlowIntensity") / 100f;
                } else if(prevOnlyColorEvent != null) {
                    if(!prevOnlyColorEvent.disabled["floorIconOutlines"]) curFloor.outline = prevOnlyColorEvent.GetBool("floorIconOutlines");
                    string str = prevOnlyColorEvent["trackTexture"] as string;
                    if(!string.IsNullOrEmpty(str)) {
                        string filePath = Path.Combine(Path.GetDirectoryName(ADOBase.customLevel.levelPath), str);
                        Texture2D tempTexture = scnGame.instance.imgHolder.AddTexture(str, out LoadResult _, filePath)?.GetTexture(TextureManager.ImageOptions.None);
                        if(tempTexture) tempTexture.wrapMode = TextureWrapMode.Repeat;
                        curFloorTrack.texture = tempTexture;
                    }
                    curFloor.customTextureScale = prevOnlyColorEvent.GetFloat("trackTextureScale");

                    curFloor.styleNum = (int) prevOnlyColorEvent["trackStyle"];
                    curFloor.glowMultiplier = prevOnlyColorEvent.GetFloat("trackGlowIntensity") / 100f;
                } else {
                    curFloorTrack.texture = levelData.trackTexture;
                    curFloor.customTextureScale = levelData.trackTextureScale;
                    curFloor.styleNum = (int) levelData.trackStyle;
                    curFloor.glowMultiplier = levelData.trackGlowIntensity / 100f;
                }
            } else {
                curFloorTrack.color1 = levelData.trackColor;
                curFloorTrack.color2 = levelData.secondaryTrackColor;
                curFloorTrack.colorAnimDuration = levelData.trackColorAnimDuration;
                curFloorTrack.colorType = levelData.trackColorType;
                curFloorTrack.pulseType = levelData.trackColorPulse;
                curFloorTrack.pulseLength = levelData.trackPulseLength;
                curFloorTrack.startOfColorChange = 0;
                curFloor.outline = levelData.floorIconOutlines;
                curFloorTrack.texture = levelData.trackTexture;
                curFloor.customTextureScale = levelData.trackTextureScale;
                curFloor.styleNum = (int) levelData.trackStyle;
                curFloor.glowMultiplier = levelData.trackGlowIntensity / 100f;
            }
        } else {
            curFloorTrack.color1 = prevFloorTrack.color1;
            curFloorTrack.color2 = prevFloorTrack.color2;
            curFloorTrack.colorType = prevFloorTrack.colorType;
            curFloorTrack.colorAnimDuration = prevFloorTrack.colorAnimDuration;
            curFloorTrack.pulseType = prevFloorTrack.pulseType;
            curFloorTrack.pulseLength = prevFloorTrack.pulseLength;
            curFloorTrack.startOfColorChange = prevFloorTrack.startOfColorChange;
            curFloorTrack.texture = prevFloorTrack.texture;
            curFloor.outline = prevFloor.outline;
            curFloor.styleNum = prevFloor.styleNum;
            curFloor.glowMultiplier = prevFloor.glowMultiplier;
        }
        curFloorTrack.animationType = prevFloorTrack.animationType;
        curFloorTrack.animationType2 = prevFloorTrack.animationType2;
        curFloorTrack.tilesAhead = prevFloorTrack.tilesAhead;
        curFloorTrack.tilesBehind = prevFloorTrack.tilesBehind;

        curFloor.UpdateAngle();
        curFloor.SetTrackStyle((TrackStyle) curFloor.styleNum, true);

        curFloor.startScale = curFloor.transform.localScale = prevFloor.startScale;
        curFloor.SetOpacity(prevFloor.opacity);
        curFloor.opacityVal = prevFloor.opacityVal;
        curFloor.rotationOffset = prevFloor.rotationOffset;
        curFloor.SetRotation((curFloor.tweenRot - curFloor.startRot).z);
        curFloor.stickToFloor = prevFloor.stickToFloor;

        if(needUpdateIcon) {
            Utility.SetupIcon(curFloor);
            Utility.SetupIcon(curFloor.nextfloor);
        }

        if(curFloorTrack.colorType == TrackColorType.Stripes) {
            Color color1 = curFloorTrack.color1;
            Color color2 = curFloorTrack.color2;
            int startFloor = prevColorEvent?.floor ?? 0;
            int endFloor = nextColorEvent?.floor ?? levelMaker.listFloors.Count;
            for(int i = floor + 1; i < endFloor; i++) {
                scrFloor stripeFloor = levelMaker.listFloors[i];
                if(curFloor.GetComponent<ffxChangeTrack>().startOfColorChange != startFloor) continue;
                stripeFloor.SetColor((stripeFloor.seqID - startFloor) % 2 == 0 ? color1 : color2);
            }
        } else curFloor.SetColor(isJustThisTile ? curFloorTrack.color1 : prevFloor.floorRenderer.deselectedColor);
        
        if(prevFloor.floorIcon is FloorIcon.Swirl or FloorIcon.SwirlCW) prevFloor.UpdateIconSprite();
    }
    
    public static void CalculateFloorEntryTimes(int floor) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        scrConductor conductor = ADOBase.conductor;
        float pitch = conductor.song.pitch;
        scrFloor curFloor = levelMaker.listFloors[floor];
        scrFloor prevFloor = curFloor.prevfloor = levelMaker.listFloors[floor - 1];
        scrFloor nextFloor = null;
        if(floor + 1 < levelMaker.listFloors.Count) (nextFloor = levelMaker.listFloors[floor + 1]).prevfloor = curFloor;
        if(floor == 1) {
            // 4 line to 1 line (optimize IL)
            // float num2 = conductor.adjustedCountdownTicks - 1f;
            // double num3 = conductor.crotchetAtStart * num2 + scrMisc.GetTimeBetweenAngles(listFloor1.entryangle, listFloor1.exitangle, listFloor1.speed, conductor.bpm, !listFloor1.isCCW);
            // levelMaker.listFloors[1].entryTime = num3;
            // levelMaker.listFloors[1].entryTimePitchAdj = num3 / pitch;
            levelMaker.listFloors[1].entryTimePitchAdj =
                (levelMaker.listFloors[1].entryTime = conductor.crotchetAtStart * (conductor.adjustedCountdownTicks - 1f) +
                                                      scrMisc.GetTimeBetweenAngles(prevFloor.entryangle, prevFloor.exitangle, prevFloor.speed, conductor.bpm, !prevFloor.isCCW)) 
                / pitch;
        } else {
            if(!prevFloor.prevfloor) prevFloor.prevfloor = levelMaker.listFloors[floor - 2];
            double oldEntryTime = nextFloor?.entryTime ?? -1;
            double oldEntryBeat = nextFloor?.entryBeat ?? -1;
            for(int i = 0; i < 2; i++) {
                double num4 = prevFloor.midSpin ? 0 : scrMisc.GetInverseAnglePerBeatMultiplanet(prevFloor.numPlanets) * (prevFloor.isCCW ? -1.0 : 1.0);
                if(prevFloor.prevfloor.midSpin && prevFloor.numPlanets > 2)
                    num4 -= (6.2831854820251465 + scrMisc.GetInverseAnglePerBeatMultiplanet(prevFloor.numPlanets)) * (prevFloor.isCCW ? -1.0 : 1.0);
                double num5 = scrMisc.GetTimeBetweenAngles(prevFloor.entryangle + num4, prevFloor.exitangle + (prevFloor.midSpin ? num4 : 0.0), prevFloor.speed, conductor.bpm, !prevFloor.isCCW);
                bool flag = num5 <= 1E-06 || num5 >= 2.0 * conductor.crotchetAtStart / prevFloor.speed - 1E-06;
                if(flag) num5 = prevFloor.midSpin ? 0.0 : 2.0 * scrMisc.GetTimeBetweenAngles(0.0, 3.1415927410125732, prevFloor.speed, conductor.bpm, false);
                double num6 = prevFloor.prevfloor.entryTime + num5;
                if(prevFloor.holdLength > 0) num6 += prevFloor.holdLength * 2 * scrMisc.GetTimeBetweenAngles(0.0, 3.1415927410125732, prevFloor.speed, conductor.bpm, false);
                float extraBeats = prevFloor.extraBeats;
                if(extraBeats > 0.0 & flag) --extraBeats;
                curFloor.entryTimePitchAdj = 
                    (curFloor.entryTime = num6 + extraBeats * scrMisc.GetTimeBetweenAngles(0.0, 3.1415927410125732, prevFloor.speed, conductor.bpm, false)) 
                    / pitch;
                curFloor.entryBeat = prevFloor.entryBeat + levelMaker.CalculateSingleFloorAngleLength(prevFloor) / Math.PI + prevFloor.extraBeats;
                prevFloor = curFloor;
                if((curFloor = curFloor.nextfloor) == null) break;
            }
            if(nextFloor) {
                oldEntryTime = nextFloor.entryTime - oldEntryTime;
                double pitchedAdj = oldEntryTime / pitch;
                oldEntryBeat = nextFloor.entryBeat - oldEntryBeat;
                nextFloor = nextFloor.nextfloor;
                while(nextFloor) {
                    nextFloor.entryTime += oldEntryTime;
                    nextFloor.entryTimePitchAdj += pitchedAdj;
                    nextFloor.entryBeat += oldEntryBeat;
                    nextFloor = nextFloor.nextfloor;
                }
            }
        }
    }
}