﻿using System;
using System.Collections.Generic;
using JALib.Tools;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence;

public class SetupTileData : LoadSequence {
    public MakePath makePath;
    public int updatedTile;
    public bool initialized;
    public bool currentSetup;

    public SetupTileData(MakePath makePath) {
        this.makePath = makePath;
        SequenceText = Main.Instance.Localization["AsyncMapLoad.ResetTile"];
        MainThread.Run(Main.Instance, Init);
    }

    public void AddTileCount() {
        lock(this) {
            if(!initialized || currentSetup || updatedTile > scrLevelMaker.instance.listFloors.Count) return;
            currentSetup = true;
        }
        JATask.Run(Main.Instance, SetupTile);
    }

    public void Init() {
        bool isFirst = true;
        foreach(scrFloor floor in scrLevelMaker.instance.listFloors) {
            foreach (ffxPlusBase component in floor.GetComponents<ffxPlusBase>()) UnityEngine.Object.DestroyImmediate(component);
            foreach (ffxBase component in floor.GetComponents<ffxBase>()) if(component is not ffxChangeTrack) UnityEngine.Object.DestroyImmediate(component);
            floor.plusEffects.Clear();
            if(Application.isPlaying) floor.floorRenderer.material.CopyPropertiesFromMaterial(RDConstants.data.floorMeshDefault);
            floor.SetTileColor(scrLevelMaker.instance.lm2.tilecolor);
            floor.Reset();
            if(!isFirst) continue;
            floor.hasLit = true;
            floor.entryangle = 4.71238899230957;
            floor.name = "0/Floor 0";
            floor.styleNum = 0;
            floor.tweenRot = floor.startRot = floor.transform.rotation.eulerAngles;
            floor.startPos = floor.offsetPos = Vector3.zero;
            floor.gameObject.AddComponent<ffxCameraPlus>();
            isFirst = false;
        }
        initialized = true;
        AddTileCount();
        scnEditor.instance.SetValue("lastSelectedFloor", null);
        scnEditor.instance.SelectFirstFloor();
        SequenceText = null;
    }

    public void SetupTile() {
        List<scrFloor> listFloors = scrLevelMaker.instance.listFloors;
        List<float> angleData = scnGame.instance.levelData.angleData;
        scrFloor prevFloor = listFloors[0];
        Vector3 zero = prevFloor.transform.position;
Restart:
        for(;updatedTile < Math.Min(angleData.Count, listFloors.Count - 1); updatedTile++) {
            SequenceText = string.Format(Main.Instance.Localization["AsyncMapLoad.CalcTile"], updatedTile, angleData.Count + (makePath.angleDataEnd ? "" : "+"));
            double startRadius = scrController.instance.startRadius;
            float floorAngle = angleData[updatedTile];
            double angle = floorAngle == 999.0 ? prevFloor.entryangle : (-floorAngle + 90) * (Math.PI / 180);
            prevFloor.exitangle = angle;
            prevFloor.UpdateAngle();
            Vector3 vectorFromAngle = scrMisc.getVectorFromAngle(angle, startRadius);
            zero += vectorFromAngle;
            scrFloor curFloor = listFloors[updatedTile + 1];
            prevFloor.nextfloor = curFloor;
            curFloor.prevfloor = prevFloor;
            curFloor.floatDirection = floorAngle;
            curFloor.seqID = updatedTile + 1;
            curFloor.entryangle = (angle + 3.1415927410125732) % 6.2831854820251465;
            curFloor.isCCW = false;
            curFloor.speed = 1f;
            if(floorAngle == 999.0) prevFloor.midSpin = true;
            curFloor.styleNum = 0;
            curFloor.startPos = zero;
            curFloor.tweenRot = curFloor.startRot = curFloor.transform.rotation.eulerAngles;
            curFloor.offsetPos = Vector3.zero;
            prevFloor = curFloor;
            makePath.setupEvent.AddSetupTile(updatedTile);
        }
        SequenceText = string.Format(Main.Instance.Localization["AsyncMapLoad.CalcTile"], updatedTile, angleData.Count + (makePath.angleDataEnd ? "" : "+"));
        bool end = false;
        lock(this) {
            if(makePath.angleDataEnd && angleData.Count == updatedTile) end = true;
            if(updatedTile >= Math.Min(angleData.Count, listFloors.Count - 1)) currentSetup = false;
            else goto Restart;
        }
        if(!end) return;
        makePath.setupEvent.AddSetupTile(updatedTile);
        SequenceText = Main.Instance.Localization["AsyncMapLoad.SetupLastTile"];
        prevFloor.isportal = true;
        prevFloor.levelnumber = Portal.EndOfLevel;
        prevFloor.exitangle = prevFloor.entryangle + 3.1415927410125732;
        prevFloor.UpdateAngle();
        MainThread.Run(Main.Instance, SetupLastTile);
    }

    public void SetupLastTile() {
        List<scrFloor> listFloors = scrLevelMaker.instance.listFloors;
        for(int i = 0; i < listFloors.Count; i++) listFloors[i].SetSortingOrder((100 + listFloors.Count - i) * 5);
        listFloors[^1].SpawnPortalParticles();
        SequenceText = null;
    }
}