using System;
using System.Collections.Generic;
using JALib.Tools;
using UnityEngine;

namespace SmartEditor.FixLoad;

public class InsertTileUpdate {
    public static List<float> floorAngles;

    public static void UpdateTile(int floor) {
        try {
            scnEditor editor = scnEditor.instance;
            scnGame game = scnGame.instance;
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            levelMaker.leveldata = game.levelData.pathData;
            floorAngles = game.levelData.angleData;
            levelMaker.isOldLevel = game.levelData.isOldLevel;
            MakeLevel(floor); //game.levelMaker.MakeLevel();
            game.ApplyEventsToFloors(game.GetValue<List<scrFloor>>("floors"));
            levelMaker.DrawHolds();
            levelMaker.DrawMultiPlanet();
            editor.Invoke("DrawFloorOffsetLines");
            editor.Invoke("DrawFloorNums");
            editor.Invoke("DrawMultiPlanet");
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static void MakeLevel(int floor) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        if(levelMaker.isOldLevel) levelMaker.InstantiateStringFloors();
        else InstantiateFloatFloors(floor); //levelMaker.InstantiateFloatFloors();
        levelMaker.lm2 ??= levelMaker.GetComponent<scrLevelMaker2>();
        for(int index = 0; index < levelMaker.listFloors.Count; ++index) {
            scrFloor listFloor = levelMaker.listFloors[index];
            listFloor.styleNum = 0;
            listFloor.UpdateAngle();
            listFloor.SetTileColor(levelMaker.lm2.tilecolor);
            int order = (100 + (levelMaker.listFloors.Count - index)) * 5;
            listFloor.SetSortingOrder(order);
            listFloor.startPos = listFloor.transform.position;
            listFloor.startRot = listFloor.transform.rotation.eulerAngles;
            listFloor.tweenRot = listFloor.startRot;
            listFloor.offsetPos = Vector3.zero;
            if(listFloor.isportal && floor + 1 == floorAngles.Count)
                listFloor.SpawnPortalParticles();
        }
    }

    public static void InstantiateFloatFloors(int floor) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        Transform floorsTransform = GameObject.Find("Floors").transform;
        ADOBase.conductor.onBeats.Clear();
        scrFloor prevFloor = levelMaker.listFloors[floor];
        float prevFloorAngle = floorAngles[floor];
        double prevAngle = prevFloorAngle == 999.0 ? prevFloor.entryangle : (-prevFloorAngle + 90.0) * (Math.PI / 180.0);
        prevFloor.exitangle = prevAngle;
        GameObject newFloorObj = UnityEngine.Object.Instantiate(levelMaker.meshFloor, prevFloor.transform.position + scrMisc.getVectorFromAngle(prevAngle, scrController.instance.startRadius), Quaternion.identity);
        newFloorObj.transform.parent = floorsTransform;
        scrFloor curFloor = newFloorObj.GetComponent<scrFloor>();
        levelMaker.listFloors.Insert(floor + 1, curFloor);
        prevFloor.nextfloor = curFloor;
        curFloor.Reset();
        curFloor.seqID = floor + 1;
        curFloor.floatDirection = prevFloorAngle;
        curFloor.entryangle = (prevAngle + 3.1415927410125732) % 6.2831854820251465;
        if(prevFloor.midSpin) {
            prevFloor.midSpin = false;
            curFloor.midSpin = true;
        }
        if(prevFloorAngle == 999.0) prevFloor.midSpin = true;
        Vector3 addedPos = newFloorObj.transform.position - prevFloor.transform.position;
        if(floor + 1 == floorAngles.Count) {
            prevFloor.isportal = false;
            foreach(scrPortalParticles component in prevFloor.GetComponents<scrPortalParticles>()) UnityEngine.Object.Destroy(component.gameObject);
            curFloor.isportal = true;
            curFloor.levelnumber = Portal.EndOfLevel;
            curFloor.exitangle = curFloor.entryangle + 3.1415927410125732;
        } else {
            float nextFloorAngle = floorAngles[floor + 1];
            double nextAngle = curFloor.midSpin ? curFloor.entryangle : (-nextFloorAngle + 90.0) * (Math.PI / 180.0);
            curFloor.exitangle = nextAngle;
            scrFloor nextFloor = levelMaker.listFloors[floor + 2];
            curFloor.nextfloor = nextFloor;
            if(curFloor.midSpin) {
                nextFloor.entryangle = (nextAngle + 3.1415927410125732) % 6.2831854820251465;
                Main.Instance.Log(prevFloor.transform.position + " " + nextFloor.transform.position);
                addedPos = prevFloor.transform.position - nextFloor.transform.position;
            }
        }
        for(int i = floor + 2; i < levelMaker.listFloors.Count; i++) {
            scrFloor fl = levelMaker.listFloors[i];
            fl.seqID = i;
            fl.editorNumText.letterText.text = i.ToString();
            fl.transform.position += addedPos;
        }
    }
}