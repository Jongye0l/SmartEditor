using System;
using System.Collections.Generic;
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
            game.ApplyEventsToFloors(levelMaker.listFloors);
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
            cur.SpawnPortalParticles();
            cur.GetComponent<scrPortalParticles>().Invoke("Start");
            levelMaker.listFloors[floor - 1].UpdateIconSprite();
            levelMaker.listFloors[floor].UpdateIconSprite();
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
            foreach(scrPortalParticles component in prevFloor.GetComponents<scrPortalParticles>()) UnityEngine.Object.DestroyImmediate(component.gameObject);
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
}