using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartEditor.FixLoad.CustomSaveState;

public class UndoTileUpdate {
    public static void UpdateTile(LevelState levelState, bool redo) {
        try {
            scnGame game = scnGame.instance;
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            levelMaker.leveldata = game.levelData.pathData;
            levelMaker.isOldLevel = game.levelData.isOldLevel;
            ApplyEvent applyEvent = new() { reloadEvents = levelState.changedEvents.Length > 0 };
            MakeLevel(levelState, applyEvent, redo); //game.levelMaker.MakeLevel();
            if(applyEvent.reloadEvents) game.ApplyEventsToFloors(levelMaker.listFloors);
            else foreach(scrFloor floor in applyEvent.onlyReloadFloors) ApplyEventsToFloors(floor);
            levelMaker.DrawHolds();
            levelMaker.DrawMultiPlanet();
            FixChartLoad.DrawEditor();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static void MakeLevel(LevelState levelState, ApplyEvent applyEvent, bool redo) {
        if(levelState.changedFloors.Length == 0) return;
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        if(levelMaker.isOldLevel) levelMaker.InstantiateStringFloors();
        else InstantiateFloatFloors(levelState, applyEvent, redo); //levelMaker.InstantiateFloatFloors();
        levelMaker.lm2 ??= levelMaker.GetComponent<scrLevelMaker2>();
        foreach(scrFloor floor in applyEvent.updatedFloors) {
            floor.startRot = floor.transform.rotation.eulerAngles;
            floor.tweenRot = floor.startRot;
            floor.offsetPos = Vector3.zero;
            CheckAndUpdateAngle(applyEvent, floor.prevfloor);
            CheckAndUpdateAngle(applyEvent, floor);
            CheckAndUpdateAngle(applyEvent, floor.nextfloor);
        }
        if(applyEvent.portalParticles) levelMaker.listFloors[^1].SpawnPortalParticles();
        if(!applyEvent.reloadEvents) foreach(scrFloor floor in applyEvent.reloadIcons) floor.UpdateIconSprite();
        if(applyEvent.reloadSeqId) for(int i = 0; i < levelMaker.listFloors.Count; i++) levelMaker.listFloors[i].SetSortingOrder((100 + levelMaker.listFloors.Count - i) * 5);
    }

    public static void CheckAndUpdateAngle(ApplyEvent applyEvent, scrFloor floor) {
        if(!applyEvent.onlyReloadFloors.Contains(floor)) floor.UpdateAngle();
    }

    public static void InstantiateFloatFloors(LevelState levelState, ApplyEvent applyEvent, bool redo) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        Transform floorsTransform = GameObject.Find("Floors").transform;
        ADOBase.conductor.onBeats.Clear();
        List<scrFloor> updatedFloors = [];
        ChangedFloorCache cache = redo ? levelState.changedFloors[0] : levelState.changedFloors[^1];
        for(int i = 1; cache != null;) {
            int act;
            ChangedFloorCache next = null;
            if(i != levelState.changedFloors.Length) {
                next = redo ? levelState.changedFloors[i++] : levelState.changedFloors[^++i];
                if(cache.action == ChangedFloorCache.Action.Add) act = next.action == ChangedFloorCache.Action.Remove && cache.index == next.index ? 1 : redo ? 2 : 3;
                else act = next.action == ChangedFloorCache.Action.Add && cache.index == next.index                                                ? 1 : redo ? 3 : 2;
                if(act == 1) {
                    updatedFloors.Add(levelMaker.listFloors[cache.index]);
                    if(i < levelState.changedFloors.Length) cache = redo ? levelState.changedFloors[i++] : levelState.changedFloors[^++i];
                    continue;
                }
            } else act = cache.action == ChangedFloorCache.Action.Add ? redo ? 2 : 3 : redo ? 3 : 2;
            if(act == 2) {
                GameObject newFloorObj = UnityEngine.Object.Instantiate(levelMaker.meshFloor, Vector3.zero, Quaternion.identity);
                newFloorObj.transform.parent = floorsTransform;
                scrFloor cur = newFloorObj.GetComponent<scrFloor>();
                cur.Reset();
                scrFloor prev = levelMaker.listFloors[cache.index];
                cur.nextfloor = prev.nextfloor;
                prev.nextfloor = cur;
                levelMaker.listFloors.Insert(cache.index + 1, cur);
                updatedFloors.Add(cur);
                updatedFloors.Add(cur.nextfloor);
                if(prev.midSpin) {
                    prev.midSpin = false;
                    cur.midSpin = true;
                }
                if(prev.isportal) {
                    prev.isportal = false;
                    foreach(scrPortalParticles component in prev.GetComponents<scrPortalParticles>()) UnityEngine.Object.DestroyImmediate(component.gameObject);
                    cur.isportal = true;
                    cur.levelnumber = Portal.EndOfLevel;
                    applyEvent.portalParticles = true;
                    if(!applyEvent.reloadEvents) {
                        applyEvent.reloadIcons.Add(prev);
                        applyEvent.reloadIcons.Add(cur);
                    }
                }
                cur.radiusScale = prev.radiusScale;
                applyEvent.onlyReloadFloors.Add(cur);
            } else {
                scrFloor prev = levelMaker.listFloors[cache.index];
                scrFloor cur = prev.nextfloor;
                prev.nextfloor = cur.nextfloor;
                UnityEngine.Object.DestroyImmediate(cur.gameObject);
                levelMaker.listFloors.RemoveAt(cache.index + 1);
                updatedFloors.Remove(cur);
                updatedFloors.Add(cur.nextfloor);
                if(cur.midSpin) prev.midSpin = true;
                if(cur.isportal) {
                    prev.isportal = true;
                    prev.levelnumber = Portal.EndOfLevel;
                    applyEvent.portalParticles = true;
                }
            }
            applyEvent.reloadSeqId = true;
            cache = next;
        }
        applyEvent.updatedFloors = updatedFloors.ToArray();
        List<float> floorAngles = scnGame.instance.levelData.angleData;
        if(applyEvent.reloadSeqId) for(int i = 0; i < levelMaker.listFloors.Count; i++) {
            scrFloor fl = levelMaker.listFloors[i];
            if(fl.seqID == i) continue;
            fl.seqID = i;
            fl.editorNumText.letterText.text = i.ToString();
        }
        foreach(scrFloor updatedFloor in updatedFloors) {
            scrFloor prevFloor = levelMaker.listFloors[updatedFloor.seqID - 1];
            updatedFloor.prevfloor = prevFloor;
            float prevFloorAngle = floorAngles[updatedFloor.seqID - 1];
            prevFloor.midSpin = prevFloorAngle == 999.0;
            double prevAngle = prevFloor.midSpin ? prevFloor.entryangle : (-prevFloorAngle + 90.0) * (Math.PI / 180.0);
            prevFloor.exitangle = prevAngle;
            updatedFloor.floatDirection = prevFloorAngle;
            updatedFloor.entryangle = (prevFloor.exitangle + 3.1415927410125732) % 6.2831854820251465;
            if(updatedFloor.isportal) updatedFloor.exitangle = updatedFloor.entryangle + 3.1415927410125732;
            else if(updatedFloor.midSpin) {
                updatedFloor.exitangle = updatedFloor.entryangle;
                updatedFloor.nextfloor.entryangle = prevAngle;
            }
        }
        Vector3 addedPos = Vector3.zero;
        Vector3 addedTransformPos = Vector3.zero;
        for(int i = 0; i < levelMaker.listFloors.Count; i++) {
            scrFloor fl = levelMaker.listFloors[i];
            if(updatedFloors.Contains(fl)) {
                scrFloor prev = levelMaker.listFloors[i - 1];
                addedPos = fl.startPos;
                Vector3 newPos = scrMisc.getVectorFromAngle(prev.exitangle, scrController.instance.startRadius);
                fl.startPos = prev.startPos + newPos;
                addedPos = fl.startPos - addedPos;
                if(applyEvent.reloadEvents) fl.transform.position = fl.startPos;
                else {
                    addedTransformPos = fl.transform.position;
                    fl.transform.position = prev.transform.position + newPos * fl.radiusScale;
                    addedTransformPos = fl.transform.position - addedTransformPos;
                }
            } else {
                fl.startPos += addedPos;
                if(applyEvent.reloadEvents) fl.transform.position = fl.startPos;
                else fl.transform.position += addedTransformPos;
            }
        }
    }

    public static void ApplyEventsToFloors(scrFloor curFloor) {
        scrFloor prevFloor = curFloor.prevfloor;
        curFloor.numPlanets = prevFloor.numPlanets;
        curFloor.isSafe = prevFloor.isSafe;
        curFloor.auto = prevFloor.auto;
        curFloor.showStatusText = prevFloor.showStatusText;
        curFloor.hideJudgment = prevFloor.hideJudgment;
        curFloor.hideIcon = prevFloor.hideIcon;
        curFloor.marginScale = prevFloor.marginScale;
        curFloor.lengthMult = prevFloor.lengthMult;
        curFloor.widthMult = prevFloor.widthMult;
        curFloor.planetEase = prevFloor.planetEase;
        curFloor.planetEaseParts = prevFloor.planetEaseParts;
        curFloor.planetEasePartBehavior = prevFloor.planetEasePartBehavior;
        curFloor.stickToFloor = prevFloor.stickToFloor;
        curFloor.customTexture = prevFloor.customTexture;
        curFloor.customTextureScale = prevFloor.customTextureScale;
        curFloor.outline = prevFloor.outline;
        curFloor.SetColor(prevFloor.floorRenderer.deselectedColor);
        curFloor.styleNum = prevFloor.styleNum;
        curFloor.UpdateAngle();
        curFloor.SetTrackStyle(prevFloor.initialTrackStyle, true);
        ffxChangeTrack prevFloorTrack = prevFloor.GetComponent<ffxChangeTrack>();
        if(prevFloorTrack) {
            ffxChangeTrack curFloorTrack = curFloor.GetOrAddComponent<ffxChangeTrack>();
            curFloorTrack.color1 = prevFloorTrack.color1;
            curFloorTrack.color2 = prevFloorTrack.color2;
            curFloorTrack.colorType = prevFloorTrack.colorType;
            curFloorTrack.colorAnimDuration = prevFloorTrack.colorAnimDuration;
            curFloorTrack.pulseType = prevFloorTrack.pulseType;
            curFloorTrack.pulseLength = prevFloorTrack.pulseLength;
            curFloorTrack.startOfColorChange = prevFloorTrack.startOfColorChange;
            curFloorTrack.texture = prevFloorTrack.texture;
            curFloorTrack.animationType = prevFloorTrack.animationType;
            curFloorTrack.animationType2 = prevFloorTrack.animationType2;
            curFloorTrack.tilesAhead = prevFloorTrack.tilesAhead;
            curFloorTrack.tilesBehind = prevFloorTrack.tilesBehind;
        }
        curFloor.glowMultiplier = prevFloor.glowMultiplier;
        curFloor.startScale = curFloor.transform.localScale = prevFloor.startScale;
        curFloor.SetOpacity(prevFloor.opacity);
        curFloor.opacityVal = prevFloor.opacityVal;
        curFloor.rotationOffset = prevFloor.rotationOffset;
        curFloor.SetRotation((curFloor.tweenRot - curFloor.startRot).z);
        curFloor.stickToFloor = prevFloor.stickToFloor;
    }

    public class ApplyEvent {
        public bool reloadEvents;
        public List<scrFloor> onlyReloadFloors = [];
        public List<scrFloor> reloadIcons = [];
        public scrFloor[] updatedFloors;
        public bool reloadSeqId;
        public bool portalParticles;
    }
}