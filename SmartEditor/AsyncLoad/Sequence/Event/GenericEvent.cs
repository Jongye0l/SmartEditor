using System;
using System.Collections.Generic;
using System.Reflection;
using ADOFAI;
using DG.Tweening;
using JALib.Tools;
using SmartEditor.AsyncLoad.MainThreadEvent;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence.Event;

public class GenericEvent : LoadSequence {
    public static MethodInfo DrawFloorOffsetLines = typeof(scnEditor).Method("DrawFloorOffsetLines");
    public static MethodInfo DrawFloorNums = typeof(scnEditor).Method("DrawFloorNums");
    public SetupEvent setupEvent;
    public int cur;
    public bool running;
    public Ease planetEase;
    public int planetEaseParts;
    public EasePartBehavior easePartBehavior;
    public bool stickToFloor;
    public Color trackColor;
    public Color secondaryTrackColor;
    public TrackColorType colorType;
    public float colorAnimDur;
    public TrackColorPulse pulseType;
    public Texture2D texture;
    public float textureScale;
    public int pulseLength;
    public int startOfColorChange;
    public bool outline;
    public TrackAnimationType trackAnimation;
    public TrackAnimationType2 disappearAnimation;
    public bool trackAnimationDisabled;
    public TrackStyle style;
    public float trackBeatsAhead;
    public float trackBeatsBehind;
    public float glowMult;
    public float num1;
    public float num2;
    public int planets;
    public bool auto;
    public bool showStatusText;
    public bool safetyTile;
    public float marginScale;
    public Vector2 tilePosition;
    public bool isHold;
    public Vector2 holdDistance;
    public Vector2 zero;
    public float opacity;
    public float scale;
    public float rotation;
    public bool hideJudgement;
    public bool hideTileIcon;
    public float highestBpm;

    public GenericEvent(SetupEvent setupEvent) {
        this.setupEvent = setupEvent;
        LevelData levelData = scnGame.instance.levelData;
        planetEase = levelData.planetEase;
        planetEaseParts = levelData.planetEaseParts;
        easePartBehavior = levelData.planetEasePartBehavior;
        stickToFloor = levelData.stickToFloors;
        trackColor = levelData.trackColor;
        secondaryTrackColor = levelData.secondaryTrackColor;
        colorType = levelData.trackColorType;
        colorAnimDur = levelData.trackColorAnimDuration;
        pulseType = levelData.trackColorPulse;
        texture = levelData.trackTexture;
        textureScale = levelData.trackTextureScale;
        pulseLength = levelData.trackPulseLength;
        startOfColorChange = 0;
        outline = levelData.floorIconOutlines;
        trackAnimation = levelData.trackAnimation;
        disappearAnimation = levelData.trackDisappearAnimation;
        trackAnimationDisabled = true;
        style = levelData.trackStyle;
        trackBeatsAhead = levelData.trackBeatsAhead;
        trackBeatsBehind = levelData.trackBeatsBehind;
        glowMult = levelData.trackGlowIntensity / 100;
        num1 = num2 = 1;
        planets = 2;
        auto = false;
        showStatusText = false;
        safetyTile = false;
        marginScale = 1;
        tilePosition = Vector2.zero;
        isHold = false;
        holdDistance = Vector2.zero;
        zero = Vector2.zero;
        opacity = 1;
        scale = 1;
        rotation = 0;
        hideJudgement = false;
        hideTileIcon = false;
        highestBpm = 0;
        LoadEvent();
    }

    public void LoadEvent() {
        lock(this) {
            if(running || cur >= setupEvent.coreEvent.cur) return;
            running = true;
        }
        JATask.Run(Main.Instance, ApplyEvent);
    }

    public void ApplyEvent() {
        List<LevelEvent>[] floorEvents = setupEvent.floorEvents;
        List<scrFloor> floors = scrLevelMaker.instance.listFloors;
        string text = Main.Instance.Localization["AsyncMapLoad.ApplyEvent3"];
        float bpmPitch = scnGame.instance.levelData.bpm * scrConductor.instance.song.pitch * GCS.currentSpeedTrial;
Restart:
        for(; cur < (setupEvent.coreEvent?.cur ?? floors.Count); cur++) {
            scrFloor thisFloor = floors[cur];
            SequenceText = string.Format(text, thisFloor.seqID, floors.Count);
            if(isHold) {
                tilePosition += holdDistance;
                isHold = false;
            }
            float speed = thisFloor.speed;
            bool isCcw = thisFloor.isCCW;
            bool stickToFloorThisTile = stickToFloor;
            Vector2 tilePositionThisTile = tilePosition;
            float opacityThisTile = opacity;
            float scaleThisTile = scale;
            float rotationThisTile = rotation;
            Color trackColorThisTile = trackColor;
            Color secondaryTrackColorThisTile = secondaryTrackColor;
            TrackColorType trackColorTypeThisTile = colorType;
            float trackColorAnimationThisTile = colorAnimDur;
            TrackColorPulse trackColorPulseThisTile = pulseType;
            Texture2D textureThisTile = texture;
            float textureScaleThisTile = textureScale;
            int trackPulseLengthThisTile = pulseLength;
            int colorStartThisTile = startOfColorChange;
            bool outlineThisTile = outline;
            TrackStyle trackStyleThisTile = style;
            float trackGlowThisTile = glowMult;
            foreach(LevelEvent levelEvent in floorEvents[thisFloor.seqID]) {
                if(!levelEvent.active) continue;
                bool justThisTile = levelEvent.data.TryGetValue("justThisTile", out object value) && (bool) value;
                switch(levelEvent.eventType) {
                    case LevelEventType.ChangeTrack:
                        trackColor = Convert.ToString(levelEvent.data["trackColor"]).HexToColor();
                        secondaryTrackColor = Convert.ToString(levelEvent.data["secondaryTrackColor"]).HexToColor();
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
                        textureThisTile = !string.IsNullOrEmpty(str) && scnGame.instance.imgHolder.customTextures.TryGetValue(str, out scrExtImgHolder.CustomTexture customTexture)
                                          ? customTexture.GetTexture(scrExtImgHolder.ImageOptions.None)
                                          : null;
                        textureScaleThisTile = levelEvent.GetFloat("trackTextureScale");
                        trackStyleThisTile = (TrackStyle) levelEvent.data["trackStyle"];
                        if(levelEvent.data.TryGetValue("trackGlowIntensity", out value)) trackGlowThisTile = (float) value / 100f;
                        if(!justThisTile) {
                            trackColor = trackColorThisTile;
                            secondaryTrackColor = secondaryTrackColorThisTile;
                            colorAnimDur = trackColorAnimationThisTile;
                            colorType = trackColorTypeThisTile;
                            pulseType = trackColorPulseThisTile;
                            pulseLength = trackPulseLengthThisTile;
                            startOfColorChange = colorStartThisTile;
                            outline = outlineThisTile;
                            texture = textureThisTile;
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
                                    tilePositionThisTile += (Vector2) levelEvent.data["positionOffset"] * 1.5f + (Vector2) fl.startPos + (Vector2) fl.offsetPos - ((Vector2) thisFloor.startPos + tilePosition);
                                } else tilePositionThisTile += (Vector2) levelEvent.data["positionOffset"] * 1.5f;
                                if(!justThisTile) tilePosition = tilePositionThisTile;
                            }
                            if(!levelEvent.disabled["scale"] && levelEvent.data.TryGetValue("scale", out value)) {
                                scaleThisTile = (float) value / 100f;
                                if(!justThisTile) scale = scaleThisTile;
                            }
                            if(!levelEvent.disabled["opacity"] && levelEvent.data.TryGetValue("opacity", out value)) {
                                opacityThisTile = (float) value / 100f;
                                if(!justThisTile) opacity = opacityThisTile;
                            }
                            if(!levelEvent.disabled["rotation"] && levelEvent.data.TryGetValue("rotation", out value)) {
                                rotationThisTile = (float) value;
                                if(!justThisTile) rotation = rotationThisTile;
                            }
                            if(!levelEvent.disabled["stickToFloors"] && levelEvent.data.TryGetValue("stickToFloors", out value)) {
                                stickToFloorThisTile = (bool) value;
                                if(!justThisTile) stickToFloor = stickToFloorThisTile;
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
                        if(levelEvent.GetInt("duration") >= 2) opacity = 0.0f;
                        continue;
                    case LevelEventType.AutoPlayTiles:
                        auto = levelEvent.GetBool("enabled");
                        showStatusText = levelEvent.GetBool("showStatusText");
                        if(levelEvent.data.TryGetValue("safetyTiles", out value)) safetyTile = (bool) value;
                        continue;
                    case LevelEventType.Hide:
                        hideJudgement = levelEvent.GetBool("hideJudgment");
                        hideTileIcon = levelEvent.GetBool("hideTileIcon");
                        continue;
                    case LevelEventType.ScaleMargin:
                        marginScale = levelEvent.GetFloat("scale") / 100f;
                        continue;
                    default:
                        continue;
                }
            }
            ApplyManyData applyManyData = new(thisFloor);
            thisFloor.numPlanets = planets;
            thisFloor.isSafe = safetyTile;
            thisFloor.auto = auto;
            thisFloor.showStatusText = showStatusText;
            thisFloor.hideJudgment = hideJudgement;
            thisFloor.hideIcon = hideTileIcon;
            thisFloor.marginScale = marginScale;
            thisFloor.planetEase = planetEase;
            thisFloor.planetEaseParts = planetEaseParts;
            thisFloor.planetEasePartBehavior = easePartBehavior;
            zero += new Vector2(Mathf.Cos(Convert.ToSingle(thisFloor.entryangle) - 1.5707964f), Mathf.Sin(Convert.ToSingle(thisFloor.entryangle) + 1.5707964f)) *
                    (float) (-(thisFloor.radiusScale - 1.0) * 1.5);
            applyManyData.position = thisFloor.startPos + new Vector3(tilePositionThisTile.x, tilePositionThisTile.y, 0.0f) + new Vector3(zero.x, zero.y, 0.0f);
            thisFloor.offsetPos = new Vector3(tilePositionThisTile.x, tilePositionThisTile.y, 0.0f) + new Vector3(zero.x, zero.y, 0.0f);
            thisFloor.customTexture = textureThisTile;
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
            changeTrack.texture = textureThisTile;
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
            highestBpm = Mathf.Max(highestBpm, speed * bpmPitch);
            setupEvent.setupEventMainThread.AddEvent(applyManyData);
        }
        ADOBase.customLevel.highestBPM = highestBpm;
        lock(this) {
            if(cur < (setupEvent.coreEvent?.cur ?? floors.Count)) goto Restart;
            running = false;
        }
        if(cur + 1 == floors.Count) {
            SequenceText = Main.Instance.Localization["AsyncMapLoad.ApplyEvent9"];
            MainThread.Run(Main.Instance, ApplyLast);
        } else SequenceText = string.Format(text, cur, floors.Count);
    }

    public void ApplyLast() {
        scnEditor editor = scnEditor.instance;
        DrawFloorOffsetLines.Invoke(editor);
        scrLevelMaker.instance.DrawHolds();
        DrawFloorNums.Invoke(editor);
        scrLevelMaker.instance.DrawMultiPlanet();
        Dispose();
    }

    public override void Dispose() {
        base.Dispose();
        setupEvent.genericEvent = null;
        setupEvent.UpdateDispose();
    }
}