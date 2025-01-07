using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using DG.Tweening;
using JALib.Core;
using SmartEditor.AsyncLoad.MainThreadEvent;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence;

public class SetupEvent : LoadSequence {
    public SetupEventMainThread setupEventMainThread = new();

    public void Setup() {
        JALocalization localization = Main.Instance.Localization;
        SequenceText = localization["AsyncMapLoad.ApplyEvent"];
        scrLevelMaker lm = scrLevelMaker.instance;
        List<scrFloor> floors = lm.listFloors;
        LevelData levelData = scnGame.instance.levelData;
        List<LevelEvent> events = levelData.levelEvents;
        scrConductor.instance.countdownTicks = levelData.countdownTicks;
        List<LevelEvent>[] floorEvents = new List<LevelEvent>[floors.Count];
        for(int index = 0; index < floorEvents.Length; ++index) floorEvents[index] = [];
        foreach(LevelEvent levelEvent in events) floorEvents[levelEvent.floor].Add(levelEvent);
        float pitch = scrConductor.instance.song.pitch * GCS.currentSpeedTrial;
        ApplyCoreEvent(floors, levelData, lm, floorEvents);
        SequenceText = localization["AsyncMapLoad.ApplyEvent"];
        lm.CalculateFloorEntryTimes();
        Ease planetEase = levelData.planetEase;
        int planetEaseParts = levelData.planetEaseParts;
        EasePartBehavior easePartBehavior = levelData.planetEasePartBehavior;
        bool stickToFloorAllTile = levelData.stickToFloors;
        Color color1 = levelData.trackColor;
        Color color2 = levelData.secondaryTrackColor;
        TrackColorType colorType = levelData.trackColorType;
        float colorAnimDur = levelData.trackColorAnimDuration;
        TrackColorPulse pulseType = levelData.trackColorPulse;
        Texture2D texture = levelData.trackTexture;
        float textureScale = levelData.trackTextureScale;
        int pulseLength = levelData.trackPulseLength;
        int startOfColorChange = floors[0].seqID;
        bool outline = levelData.floorIconOutlines;
        TrackAnimationType trackAnimation = levelData.trackAnimation;
        TrackAnimationType2 disappearAnimation = levelData.trackDisappearAnimation;
        bool trackAnimationDisabled = true;
        TrackStyle style = levelData.trackStyle;
        float trackBeatsAhead = levelData.trackBeatsAhead;
        float trackBeatsBehind = levelData.trackBeatsBehind;
        float glowMult = levelData.trackGlowIntensity / 100f;
        float num1 = 1f;
        float num2 = 1f;
        int planets = 2;
        bool flag3 = false;
        bool flag4 = false;
        bool flag5 = false;
        float num3 = 1f;
        Vector2 tilePositionAllTile = Vector2.zero;
        Vector2 tilePositionThisTile = Vector2.zero;
        bool isHold = false;
        Vector2 holdDistance = Vector2.zero;
        Vector2 zero = Vector2.zero;
        float opacityAllTile = 1f;
        float scaleAllTile = 1f;
        float rotationAllTile = 0.0f;
        float opacityThisTile = 1f;
        float scaleThisTile = 1f;
        float rotationThisTile = 0.0f;
        bool stickToFloorThisTile = stickToFloorAllTile;
        Color trackColorThisTile = color1;
        Color secondaryTrackColorThisTile = color2;
        TrackColorType trackColorTypeThisTile = colorType;
        float trackColorAnimationThisTile = colorAnimDur;
        TrackColorPulse trackColorPulseThisTile = pulseType;
        Texture2D tempTexture = texture;
        float textureScaleThisTile = textureScale;
        int trackPulseLengthThisTile = pulseLength;
        int colorStartThisTile = startOfColorChange;
        bool outlineThisTile = outline;
        TrackStyle trackStyleThisTile = style;
        float trackGlowThisTile = glowMult;
        bool flag8 = false;
        bool flag9 = false;
        float highestBpm = 0.0f;
        int freeroamRegion = 0;
        float num10 = 1f;
        float num11 = 1f;
        foreach(scrFloor thisFloor in floors) {
            SequenceText = string.Format(localization["AsyncMapLoad.ApplyEvent3"], thisFloor.seqID + 1, floors.Count);
            if(isHold) {
                tilePositionAllTile += holdDistance;
                tilePositionThisTile += holdDistance;
                isHold = false;
            }
            float speed = thisFloor.speed;
            bool isCcw = thisFloor.isCCW;
            object value;
            foreach(LevelEvent levelEvent in floorEvents[thisFloor.seqID]) {
                if(!levelEvent.active) continue;
                bool justThisTile = levelEvent.data.TryGetValue("justThisTile", out value) && (bool) value;
                switch(levelEvent.eventType) {
                    case LevelEventType.ChangeTrack:
                        color1 = Convert.ToString(levelEvent.data["trackColor"]).HexToColor();
                        color2 = Convert.ToString(levelEvent.data["secondaryTrackColor"]).HexToColor();
                        colorAnimDur = levelEvent.GetFloat("trackColorAnimDuration");
                        colorType = (TrackColorType) levelEvent.data["trackColorType"];
                        pulseType = (TrackColorPulse) levelEvent.data["trackColorPulse"];
                        pulseLength = (int) levelEvent["trackPulseLength"];
                        startOfColorChange = thisFloor.seqID;
                        trackAnimation = (TrackAnimationType) levelEvent.data["trackAnimation"];
                        disappearAnimation = (TrackAnimationType2) levelEvent.data["trackDisappearAnimation"];
                        style = (TrackStyle) levelEvent.data["trackStyle"];
                        trackBeatsAhead = levelEvent.GetFloat("beatsAhead");
                        trackBeatsBehind = levelEvent.GetFloat("beatsBehind");
                        continue;
                    case LevelEventType.ColorTrack:
                        trackColorThisTile = ((string) levelEvent.data["trackColor"]).HexToColor();
                        secondaryTrackColorThisTile = Convert.ToString(levelEvent.data["secondaryTrackColor"]).HexToColor();
                        trackColorAnimationThisTile = levelEvent.GetFloat("trackColorAnimDuration");
                        trackColorTypeThisTile = (TrackColorType) levelEvent.data["trackColorType"];
                        trackColorPulseThisTile = (TrackColorPulse) levelEvent.data["trackColorPulse"];
                        trackPulseLengthThisTile = (int) levelEvent["trackPulseLength"];
                        colorStartThisTile = thisFloor.seqID;
                        if(!levelEvent.disabled["floorIconOutlines"]) outlineThisTile = levelEvent.GetBool("floorIconOutlines");
                        string str = levelEvent.data["trackTexture"] as string;
                        tempTexture = !string.IsNullOrEmpty(str) && scnGame.instance.imgHolder.customTextures.TryGetValue(str, out scrExtImgHolder.CustomTexture customTexture)
                                          ? customTexture.GetTexture(scrExtImgHolder.ImageOptions.None)
                                          : null;
                        textureScaleThisTile = levelEvent.GetFloat("trackTextureScale");
                        trackStyleThisTile = (TrackStyle) levelEvent.data["trackStyle"];
                        if(levelEvent.data.TryGetValue("trackGlowIntensity", out value)) trackGlowThisTile = (float) value / 100f;
                        if(!justThisTile) {
                            color1 = trackColorThisTile;
                            color2 = secondaryTrackColorThisTile;
                            colorAnimDur = trackColorAnimationThisTile;
                            colorType = trackColorTypeThisTile;
                            pulseType = trackColorPulseThisTile;
                            pulseLength = trackPulseLengthThisTile;
                            startOfColorChange = colorStartThisTile;
                            outline = outlineThisTile;
                            texture = tempTexture;
                            textureScale = textureScaleThisTile;
                            style = trackStyleThisTile;
                            glowMult = trackGlowThisTile;
                        }
                        continue;
                    case LevelEventType.AnimateTrack:
                        trackAnimationDisabled = !levelEvent.disabled["trackAnimation"];
                        if(trackAnimationDisabled) {
                            trackAnimation = (TrackAnimationType) levelEvent.data["trackAnimation"];
                            trackBeatsAhead = levelEvent.GetFloat("beatsAhead");
                        }
                        if(!levelEvent.disabled["trackDisappearAnimation"]) {
                            disappearAnimation = (TrackAnimationType2) levelEvent.data["trackDisappearAnimation"];
                            trackBeatsBehind = levelEvent.GetFloat("beatsBehind");
                        }
                        num1 = num2;
                        num2 = speed;
                        continue;
                    case LevelEventType.SetPlanetRotation:
                        planetEase = (Ease) levelEvent.data["ease"];
                        planetEaseParts = (int) levelEvent.data["easeParts"];
                        easePartBehavior = (EasePartBehavior) levelEvent.data["easePartBehavior"];
                        continue;
                    case LevelEventType.PositionTrack:
                        bool editorOnly = levelEvent.data.TryGetValue("editorOnly", out value) && (bool) value;
                        if(editorOnly && scrController.instance.paused || !editorOnly) {
                            if(!levelEvent.disabled["positionOffset"]) {
                                int floor = thisFloor.seqID;
                                if(levelEvent.data.TryGetValue("relativeTo", out value))
                                    floor = scnGame.IDFromTile(value as Tuple<int, TileRelativeTo>, thisFloor.seqID, floors);
                                if(floor != thisFloor.seqID) {
                                    scrFloor fl = floors[floor];
                                    tilePositionThisTile += (Vector2) levelEvent.data["positionOffset"] * 1.5f + (Vector2) fl.startPos + (Vector2) fl.offsetPos - ((Vector2) thisFloor.startPos + tilePositionAllTile);
                                } else tilePositionThisTile += (Vector2) levelEvent.data["positionOffset"] * 1.5f;
                                if(!justThisTile) tilePositionAllTile = tilePositionThisTile;
                            }
                            if(!levelEvent.disabled["scale"] && levelEvent.data.TryGetValue("scale", out value)) {
                                scaleThisTile = (float) value / 100f;
                                if(!justThisTile) scaleAllTile = scaleThisTile;
                            }
                            if(!levelEvent.disabled["opacity"] && levelEvent.data.TryGetValue("opacity", out value)) {
                                opacityThisTile = (float) value / 100f;
                                if(!justThisTile) opacityAllTile = opacityThisTile;
                            }
                            if(!levelEvent.disabled["rotation"] && levelEvent.data.TryGetValue("rotation", out value)) {
                                rotationThisTile = (float) value;
                                if(!justThisTile) rotationAllTile = rotationThisTile;
                            }
                            if(!levelEvent.disabled["stickToFloors"] && levelEvent.data.TryGetValue("stickToFloors", out value)) {
                                stickToFloorThisTile = (bool) value;
                                if(!justThisTile) stickToFloorAllTile = stickToFloorThisTile;
                            }
                        }
                        continue;
                    case LevelEventType.Hold:
                        if((int) levelEvent.data["duration"] >= 0) {
                            bool flag12 = Math.Abs(scrMisc.GetAngleMoved(thisFloor.entryangle, thisFloor.exitangle, !isCcw) - 0.0) < 1E-05 ||
                                          Math.Abs(scrMisc.GetAngleMoved(thisFloor.entryangle, thisFloor.exitangle, !isCcw) - 2.0 * Math.PI) < 1E-05;
                            float num13 = 1f;
                            if(levelEvent.data.TryGetValue("distanceMultiplier", out value))
                                num13 = (int) value / 100f;
                            thisFloor.holdDistance = flag12 ? 0.0f : ((int) levelEvent.data["duration"] * 2 + 1) * num13;
                            thisFloor.holdLength = (int) levelEvent.data["duration"];
                            holdDistance = flag12 ? Vector2.zero
                                               : new Vector2(Mathf.Cos(Convert.ToSingle(thisFloor.exitangle) - 1.5707964f), Mathf.Sin(Convert.ToSingle(thisFloor.exitangle) + 1.5707964f)) *
                                                 (thisFloor.holdDistance * 1.5f);
                            isHold = true;
                            thisFloor.showHoldTiming = levelEvent.data.TryGetValue("landingAnimation", out value) && (bool) value;
                            continue;
                        }
                        thisFloor.holdLength = -1;
                        continue;
                    case LevelEventType.MultiPlanet:
                        planets = (int) levelEvent.data["planets"];
                        if(planets < 2) planets = 2;
                        planets = Math.Min(planets, 3);
                        if(thisFloor.prevfloor && thisFloor.prevfloor.midSpin) thisFloor.prevfloor.numPlanets = planets;
                        continue;
                    case LevelEventType.FreeRoam:
                        if(levelEvent.GetInt("duration") >= 2) opacityAllTile = 0.0f;
                        continue;
                    case LevelEventType.AutoPlayTiles:
                        flag3 = levelEvent.GetBool("enabled");
                        flag4 = levelEvent.GetBool("showStatusText");
                        if(levelEvent.data.TryGetValue("safetyTiles", out value)) flag5 = (bool) value;
                        continue;
                    case LevelEventType.Hide:
                        flag8 = levelEvent.GetBool("hideJudgment");
                        flag9 = levelEvent.GetBool("hideTileIcon");
                        continue;
                    case LevelEventType.ScaleMargin:
                        num3 = levelEvent.GetFloat("scale") / 100f;
                        continue;
                    case LevelEventType.TileDimensions:
                        num10 = levelEvent.GetFloat("length") / 100f;
                        num11 = levelEvent.GetFloat("width") / 100f;
                        continue;
                    default:
                        continue;
                }
            }
            ApplyManyData applyManyData = new(thisFloor);
            thisFloor.numPlanets = planets;
            thisFloor.isSafe = flag5;
            thisFloor.auto = flag3;
            thisFloor.showStatusText = flag4;
            thisFloor.hideJudgment = flag8;
            thisFloor.hideIcon = flag9;
            thisFloor.marginScale = num3;
            thisFloor.lengthMult = num10;
            thisFloor.widthMult = num11;
            thisFloor.planetEase = planetEase;
            thisFloor.planetEaseParts = planetEaseParts;
            thisFloor.planetEasePartBehavior = easePartBehavior;
            zero += new Vector2(Mathf.Cos(Convert.ToSingle(thisFloor.entryangle) - 1.5707964f), Mathf.Sin(Convert.ToSingle(thisFloor.entryangle) + 1.5707964f)) *
                    (float) (-(thisFloor.radiusScale - 1.0) * 1.5);
            applyManyData.position = thisFloor.startPos + new Vector3(tilePositionThisTile.x, tilePositionThisTile.y, 0.0f) + new Vector3(zero.x, zero.y, 0.0f);
            thisFloor.offsetPos = new Vector3(tilePositionThisTile.x, tilePositionThisTile.y, 0.0f) + new Vector3(zero.x, zero.y, 0.0f);
            tilePositionThisTile = tilePositionAllTile;
            thisFloor.customTexture = tempTexture;
            thisFloor.customTextureScale = textureScaleThisTile;
            thisFloor.outline = outlineThisTile;
            switch(trackColorTypeThisTile) {
                case TrackColorType.Single:
                case TrackColorType.Glow:
                case TrackColorType.Blink:
                case TrackColorType.Switch:
                case TrackColorType.Volume:
                    applyManyData.color = trackColorThisTile;
                    break;
                case TrackColorType.Stripes:
                    applyManyData.color = (thisFloor.seqID - startOfColorChange) % 2 == 0 ? trackColorThisTile : secondaryTrackColorThisTile;
                    break;
                case TrackColorType.Rainbow:
                    applyManyData.color = Color.white;
                    break;
            }
            thisFloor.styleNum = (int) trackStyleThisTile;
            applyManyData.trackStyle = trackStyleThisTile;
            ffxChangeTrack changeTrack = thisFloor.GetComponent<ffxChangeTrack>();
            changeTrack.color1 = trackColorThisTile;
            changeTrack.color2 = secondaryTrackColorThisTile;
            changeTrack.colorType = trackColorTypeThisTile;
            changeTrack.colorAnimDuration = trackColorAnimationThisTile;
            changeTrack.pulseType = trackColorPulseThisTile;
            changeTrack.pulseLength = trackPulseLengthThisTile;
            changeTrack.startOfColorChange = colorStartThisTile;
            changeTrack.texture = tempTexture;
            float num14 = speed / num2;
            float num15 = speed / num1;
            changeTrack.animationType = trackAnimation;
            changeTrack.animationType2 = disappearAnimation;
            changeTrack.tilesAhead = trackBeatsAhead * (trackAnimationDisabled ? num14 : num15);
            changeTrack.tilesBehind = trackBeatsBehind * (trackAnimationDisabled ? num14 : num15);
            thisFloor.glowMultiplier = trackGlowThisTile;
            // if(!Mathf.Approximately(Mathf.Round((float) scrMisc.GetAngleMoved(thisFloor.entryangle, thisFloor.exitangle, !thisFloor.isCCW) * 57.29578f), 0.0f) || thisFloor.midSpin) {
            //     foreach(LevelEvent levelEvent in levelEventList) {
            //         if(levelEvent.data.TryGetValue("angleOffset", out value)) {
            //             float num16 = (float) value;
            //             levelEvent.data["angleOffset"] = num16;
            //         }
            //     }
            // }
            thisFloor.startScale = applyManyData.scale = new Vector3(scaleThisTile, scaleThisTile, 0.0f);
            thisFloor.SetOpacity(scrController.instance.paused ? Mathf.Max(opacityThisTile, 0.1f) : opacityThisTile);
            thisFloor.opacityVal = opacityThisTile;
            thisFloor.rotationOffset = rotationThisTile;
            applyManyData.rotation = rotationThisTile;
            thisFloor.stickToFloor = stickToFloorThisTile;
            scaleThisTile = scaleAllTile;
            opacityThisTile = opacityAllTile;
            rotationThisTile = rotationAllTile;
            stickToFloorThisTile = stickToFloorAllTile;
            highestBpm = Mathf.Max(highestBpm, speed * levelData.bpm * pitch);
            trackColorThisTile = color1;
            secondaryTrackColorThisTile = color2;
            trackColorTypeThisTile = colorType;
            trackColorAnimationThisTile = colorAnimDur;
            trackColorPulseThisTile = pulseType;
            tempTexture = texture;
            textureScaleThisTile = textureScale;
            trackPulseLengthThisTile = pulseLength;
            colorStartThisTile = startOfColorChange;
            outlineThisTile = outline;
            trackStyleThisTile = style;
            trackGlowThisTile = glowMult;
            setupEventMainThread.AddEvent(applyManyData);
        }
        ADOBase.customLevel.highestBPM = highestBpm;
        setupEventMainThread.AddEvent(new ResetFreeFoam());
        foreach(scrFloor floor in floors) {
            if(!floor.nextfloor) continue;
            SequenceText = string.Format(localization["AsyncMapLoad.ApplyEvent4"], floor.seqID + 1, floors.Count);
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
                    if(levelEvent.data.TryGetValue("hitsoundOnBeats", out object value))
                        floor.freeroamSoundOnBeat = (HitSound) value;
                    if(levelEvent.data.TryGetValue("hitsoundOffBeats", out object value1))
                        floor.freeroamSoundOffBeat = (HitSound) value1;
                    floor.SetOpacity(scrController.instance.paused ? 0.1f : 0.0f);
                    floor.opacityVal = 0.0f;
                }
                freeraomEvents ??= [];
                freeraomEvents.Add(levelEvent);
            }
            if(freeraomEvents != null) setupEventMainThread.AddEvent(new ApplyFreeRoamEvent(floor, freeraomEvents));
        }
        ffxFlashPlus.legacyFlash = levelData.legacyFlash;
        global::ffxCameraPlus.legacyRelativeTo = levelData.legacyCamRelativeTo;
        scrVfx.instance.currentColourScheme.colourText = levelData.defaultTextColor;
        scrVfx.instance.currentColourScheme.colourTextShadow = levelData.defaultTextShadowColor;
        int holdLength = 0;
        foreach(scrFloor floor in floors) {
            SequenceText = string.Format(localization["AsyncMapLoad.ApplyEvent5"], floor.seqID + 1, floors.Count);
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
            setupEventMainThread.AddRequestSprite(comment);
        }
        if(scrController.instance.paused) return;
        Dictionary<int, Dictionary<string, Tuple<int, float, bool>>> repeatEventData = new();
        foreach(LevelEvent levelEvent in events) {
            if(!levelEvent.active || levelEvent.info.type != LevelEventType.RepeatEvents) continue;
            bool isBeat = (RepeatType) levelEvent.data["repeatType"] == RepeatType.Beat;
            int repeat = isBeat ? Convert.ToInt32(levelEvent.data["repetitions"]) : Convert.ToInt32(levelEvent.data["floorCount"]);
            float interval = isBeat ? levelEvent.GetFloat("interval") : -1f;
            string[] tag = (levelEvent.GetString("tag") ?? "").Split(" ");
            bool executeOnCurrentFloor = levelEvent.GetBool("executeOnCurrentFloor");
            if(!repeatEventData.ContainsKey(levelEvent.floor))
                repeatEventData[levelEvent.floor] = new Dictionary<string, Tuple<int, float, bool>>();
            foreach(string key in tag)
                repeatEventData[levelEvent.floor][key] = new Tuple<int, float, bool>(repeat, interval, executeOnCurrentFloor);
        }
        setupEventMainThread.repeatEventData = repeatEventData;
        Dictionary<int, string[]> conditionalEventData = new();
        foreach(LevelEvent levelEvent in events.FindAll(x => x.info.type == LevelEventType.SetConditionalEvents)) {
            if(!levelEvent.active) continue;
            string[] strArray = [
                levelEvent.GetString("perfectTag"),
                levelEvent.GetString("earlyPerfectTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("hitTag"),
                levelEvent.GetString("latePerfectTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("hitTag"),
                levelEvent.GetString("veryEarlyTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("barelyTag"),
                levelEvent.GetString("veryLateTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("barelyTag"),
                levelEvent.GetString("tooEarlyTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("missTag"),
                levelEvent.GetString("tooLateTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("missTag"),
                levelEvent.GetString("lossTag"),
                levelEvent.GetString("onCheckpointTag")
            ];
            conditionalEventData.Add(levelEvent.floor, strArray);
            floors[levelEvent.floor].hasConditionalChange = true;
        }
        setupEventMainThread.conditionalEventData = conditionalEventData;
        setupEventMainThread.ApplyEvent();
        ffxCameraPlus ffxCameraPlus = floors[0].GetComponent<ffxCameraPlus>();
        floors[0].plusEffects.Insert(0, ffxCameraPlus);
        ffxCameraPlus.startTime = 0.0;
        ffxCameraPlus.duration = 0.0f;
        ffxCameraPlus.targetPos = levelData.camPosition * 1.5f;
        ffxCameraPlus.targetRot = levelData.camRotation;
        ffxCameraPlus.targetZoom = levelData.camZoom / 100f;
        ffxCameraPlus.ease = Ease.Linear;
        ffxCameraPlus.movementType = levelData.camRelativeTo;
        ffxCameraPlus.dontDisable = levelData.camEnabledOnLowVFX;
        Dispose();
    }

    public void ApplyCoreEvent(List<scrFloor> floors, LevelData levelData, scrLevelMaker lm, List<LevelEvent>[] floorEvents) {
        JALocalization localization = Main.Instance.Localization;
        float speed = 1f;
        float speedThisTile = 1f;
        bool speedOnlyThisTile = false;
        bool isCCW = false;
        float bpm = levelData.bpm;
        float scale = 1f;
        int planets = 2;
        scrController.instance.planetsUsed = 2;
        float tileWidth = 1f;
        float tileLength = 1f;
        foreach(scrFloor floor in floors) {
            SequenceText = string.Format(localization["AsyncMapLoad.ApplyEvent2"], floor.seqID + 1, floors.Count);
            floor.extraBeats = 0.0f;
            List<LevelEvent> floorEvent = floorEvents[floor.seqID];
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
                            if(floor.seqID + tileOffset < 0) tileOffset = -floor.seqID;
                            if(floor.seqID + tileOffset > lm.listFloors.Count - 2)
                                tileOffset = lm.listFloors.Count - 2 - floor.seqID;
                        }
                        setupEventMainThread.AddEvent(new AddCheckpoint(floor, tileOffset));
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
                            if(!floor.midSpin && Math.Abs(scrMisc.GetAngleMoved(floor.entryangle, floor.exitangle, !floor.isCCW) - 6.2831854820251465) < 0.0001 && Application.isPlaying)
                                ++floor.extraBeats;
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
            if(speedAngleList.Count > 0) {
                float num8 = scrMisc.ApproximatelyFloor(floor.entryangle, floor.exitangle) ? 360f : (float) (scrMisc.GetAngleMoved(floor.entryangle, floor.exitangle, !isCCW) * 57.295780181884766);
                float num10 = speedAngleList[0] / 180f * 60f / (bpm * speed);
                foreach(float num11 in speedAngleList) {
                    float angleOffset = num11;
                    LevelEvent last = setSpeedEvent.FindLast(e => e.GetFloat("angleOffset") == angleOffset);
                    float num12 = speedAngleList.Find(e => e > angleOffset);
                    if(num12 == 0.0) num12 = num8;
                    float setupedBPM;
                    if((SpeedType) last.data["speedType"] == SpeedType.Bpm) setupedBPM = last.GetFloat("beatsPerMinute");
                    else {
                        setupedBPM = bpm * speed;
                        foreach(LevelEvent levelEvent in setSpeedEvent) {
                            if(levelEvent.GetFloat("angleOffset") != angleOffset) continue;
                            if((SpeedType) levelEvent.data["speedType"] == SpeedType.Bpm) setupedBPM = levelEvent.GetFloat("beatsPerMinute");
                            setupedBPM *= levelEvent.GetFloat("bpmMultiplier");
                        }
                    }
                    num10 += (float) ((num12 - angleOffset) / 180.0) * 60f / setupedBPM;
                    if(angleOffset > 0.0 && angleOffset <= num8) speedOnlyThisTile = true;
                    speed = setupedBPM / bpm;
                }
                if(speedOnlyThisTile) speedThisTile = (float) (60.0 / num10 * (num8 / 180.0)) / bpm;
            }
            floor.radiusScale = scale;
            if(speedOnlyThisTile) {
                floor.speed = speedThisTile;
                speedOnlyThisTile = false;
            } else floor.speed = speed;
            floor.isCCW = isCCW;
            floor.numPlanets = planets;
            floor.lengthMult = tileLength;
            floor.widthMult = tileWidth;
            if(planets > scrController.instance.planetsUsed)
                scrController.instance.planetsUsed = planets;
        }
    }

    private static bool CheckActive(LevelEvent e) => e.active;
}