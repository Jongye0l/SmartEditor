using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartEditor.FixLoad;

public static class PasteTileUpdate {
    public static void UpdateTile() {
        try {
            scnEditor editor = scnEditor.instance;
            scnGame game = scnGame.instance;
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            levelMaker.leveldata = game.levelData.pathData;
            levelMaker.isOldLevel = game.levelData.isOldLevel;
            MakeLevel(editor.selectedFloors[0].seqID, editor.clipboard.Count); //game.levelMaker.MakeLevel();
            game.ApplyEventsToFloors(levelMaker.listFloors);
            levelMaker.DrawHolds();
            levelMaker.DrawMultiPlanet();
            FixChartLoad.DrawEditor();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static void MakeLevel(int floor, int size) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        if(levelMaker.isOldLevel) levelMaker.InstantiateStringFloors();
        else InstantiateFloatFloors(floor, size); //levelMaker.InstantiateFloatFloors();
        levelMaker.lm2 ??= levelMaker.GetComponent<scrLevelMaker2>();
        levelMaker.listFloors[floor].UpdateAngle();
        for(int i = 0; i < size; i++) {
            scrFloor cur = levelMaker.listFloors[floor + 1];
            cur.styleNum = 0;
            cur.SetTileColor(levelMaker.lm2.tilecolor);
            if(cur.isportal) {
                cur.SpawnPortalParticles();
                levelMaker.listFloors[floor].UpdateIconSprite();
            }
        }
        for(int index = 0; index < levelMaker.listFloors.Count; ++index) {
            scrFloor listFloor = levelMaker.listFloors[index];
            listFloor.SetSortingOrder((100 + levelMaker.listFloors.Count - index) * 5);
            if(index < floor) continue;
            listFloor.startPos = listFloor.transform.position;
            listFloor.startRot = listFloor.transform.rotation.eulerAngles;
            listFloor.tweenRot = listFloor.startRot;
            listFloor.offsetPos = Vector3.zero;
        }
    }

    public static void InstantiateFloatFloors(int floor, int size) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        Transform floorsTransform = GameObject.Find("Floors").transform;
        List<float> floorAngles = scnGame.instance.levelData.angleData;
        ADOBase.conductor.onBeats.Clear();
        scrFloor prevFloor = levelMaker.listFloors[floor];
        prevFloor.transform.position = prevFloor.startPos;
        Vector3 addedPos = prevFloor.startPos;
        scrFloor curFloor;
        bool needPortal = false;
        if(prevFloor.isportal) {
            prevFloor.isportal = false;
            foreach(scrPortalParticles component in prevFloor.GetComponents<scrPortalParticles>()) UnityEngine.Object.DestroyImmediate(component.gameObject);
            needPortal = true;
        }
        scrFloor[] floors = new scrFloor[size];
        for(int i = 0; i < size; i++) {
            float prevFloorAngle = floorAngles[floor + i];
            double prevAngle = prevFloorAngle == 999.0 ? prevFloor.entryangle : (-prevFloorAngle + 90.0) * (Math.PI / 180.0);
            prevFloor.exitangle = prevAngle;
            GameObject newFloorObj = UnityEngine.Object.Instantiate(levelMaker.meshFloor, prevFloor.startPos + scrMisc.getVectorFromAngle(prevAngle, FixChartLoad.GetRadius()), Quaternion.identity);
            newFloorObj.transform.parent = floorsTransform;
            curFloor = newFloorObj.GetComponent<scrFloor>();
            floors[i] = curFloor;
            prevFloor.nextfloor = curFloor;
            curFloor.Reset();
            curFloor.seqID = floor + i + 1;
            curFloor.floatDirection = prevFloorAngle;
            curFloor.entryangle = (prevAngle + 3.1415927410125732) % 6.2831854820251465;
            if(prevFloorAngle == 999.0) prevFloor.midSpin = true;
            prevFloor = curFloor;
        }
        levelMaker.listFloors.InsertRange(floor + 1, floors);
        if(needPortal) {
            prevFloor.isportal = true;
            prevFloor.levelnumber = Portal.EndOfLevel;
            prevFloor.exitangle = prevFloor.entryangle + 3.1415927410125732;
        } else {
            addedPos = prevFloor.startPos - addedPos;
            float nextFloorAngle = floorAngles[floor + size];
            double nextAngle = prevFloor.midSpin ? prevFloor.entryangle : (-nextFloorAngle + 90.0) * (Math.PI / 180.0);
            prevFloor.exitangle = nextAngle;
            curFloor = levelMaker.listFloors[floor + size + 1];
            prevFloor.nextfloor = curFloor;
            if(curFloor.midSpin) curFloor.entryangle = (nextAngle + 3.1415927410125732) % 6.2831854820251465;
            for(int i = floor + size + 1; i < levelMaker.listFloors.Count; i++) {
                scrFloor fl = levelMaker.listFloors[i];
                fl.seqID = i;
                fl.editorNumText.letterText.text = i.ToString();
                fl.transform.position = fl.startPos + addedPos;
            }
        }
    }
}