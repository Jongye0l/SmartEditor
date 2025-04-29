using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartEditor.FixLoad;

public static class FlipTileUpdate {
    public static void UpdateTile(int floor, int size, bool horizontal) {
        try {
            scnGame game = scnGame.instance;
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            levelMaker.leveldata = game.levelData.pathData;
            levelMaker.isOldLevel = game.levelData.isOldLevel;
            MakeLevel(floor, size, horizontal); //game.levelMaker.MakeLevel();
            game.ApplyEventsToFloors(levelMaker.listFloors);
            levelMaker.DrawHolds();
            levelMaker.DrawMultiPlanet();
            FixChartLoad.DrawEditor();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static void UpdateTileSelection(bool horizontal) {
        List<scrFloor> selectedFloors = scnEditor.instance.selectedFloors;
        UpdateTile(selectedFloors[0].seqID, selectedFloors.Count, horizontal);
    }

    public static void MakeLevel(int floor, int size, bool horizontal) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        if(levelMaker.isOldLevel) levelMaker.InstantiateStringFloors();
        else InstantiateFloatFloors(floor, size, horizontal); //levelMaker.InstantiateFloatFloors();
        levelMaker.lm2 ??= levelMaker.GetComponent<scrLevelMaker2>();
        levelMaker.listFloors[floor - 1].UpdateAngle();
        for(int index = floor; index < levelMaker.listFloors.Count; ++index) {
            scrFloor listFloor = levelMaker.listFloors[index];
            listFloor.startRot = listFloor.transform.rotation.eulerAngles;
            listFloor.tweenRot = listFloor.startRot;
            listFloor.offsetPos = Vector3.zero;
            if(index < floor + size) {
                listFloor.UpdateAngle();
                if(listFloor.floorIcon is FloorIcon.Swirl or FloorIcon.SwirlCW) listFloor.UpdateIconSprite();
            }
        }
    }

    public static void InstantiateFloatFloors(int floor, int size, bool horizontal) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        scrFloor prevFloor = levelMaker.listFloors[floor - 1];
        List<float> floorAngles = scnGame.instance.levelData.angleData;
        for(int i = 0; i < size; i++) {
            float prevFloorAngle = floorAngles[floor - 1 + i];
            double prevAngle = prevFloorAngle == 999.0 ? prevFloor.entryangle : (-prevFloorAngle + 90.0) * (Math.PI / 180.0);
            prevFloor.exitangle = prevAngle;
            scrFloor curFloor = levelMaker.listFloors[floor + i];
            curFloor.floatDirection = prevFloorAngle;
            curFloor.entryangle = (prevAngle + 3.1415927410125732) % 6.2831854820251465;
            if(curFloor.midSpin) curFloor.exitangle = curFloor.entryangle;
            prevFloor = curFloor;
        }
        if(prevFloor.isportal) prevFloor.exitangle = prevFloor.entryangle + 3.1415927410125732;
        prevFloor = levelMaker.listFloors[floor - 1];
        float mid = (horizontal ? prevFloor.startPos.x : prevFloor.startPos.y) * 2;
        Vector3 change = default;
        for(int i = floor; i < levelMaker.listFloors.Count; i++) {
            scrFloor fl = levelMaker.listFloors[i];
            if(i < floor + size) {
                bool last = i == floor + size - 1;
                if(last) change = fl.startPos;
                Vector3 added = fl.transform.position - fl.startPos;
                fl.startPos = horizontal ? new Vector3(mid - fl.startPos.x, fl.startPos.y, fl.startPos.z) : new Vector3(fl.startPos.x, mid - fl.startPos.y, fl.startPos.z);
                fl.transform.position = fl.startPos + added;
                if(last) {
                    change = fl.startPos - change;
                    if(fl.midSpin) change = levelMaker.listFloors[i - 1].startPos - fl.nextfloor.startPos;
                }
            } else {
                fl.startPos += change;
                fl.transform.position += change;
            }
        }
    }
}