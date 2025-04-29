using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SmartEditor.FixLoad;

public static class DeleteTileUpdate {

    public static void UpdateTile(int floor, int size) {
        try {
            scnGame game = scnGame.instance;
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            levelMaker.leveldata = game.levelData.pathData;
            levelMaker.isOldLevel = game.levelData.isOldLevel;
            MakeLevel(floor, size); //game.levelMaker.MakeLevel();
            game.ApplyEventsToFloors(levelMaker.listFloors);
            levelMaker.DrawHolds();
            levelMaker.DrawMultiPlanet();
            FixChartLoad.DrawEditor();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static void UpdateTileSubsequent(int floor) => UpdateTile(floor + 1, scrLevelMaker.instance.listFloors.Count - floor - 1);

    public static void UpdateTilePreceding() => UpdateTile(1, scnEditor.instance.selectedFloors[0].seqID);

    public static void MakeLevel(int floor, int size) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        if(levelMaker.isOldLevel) levelMaker.InstantiateStringFloors();
        else InstantiateFloatFloors(floor, size); //levelMaker.InstantiateFloatFloors();
        levelMaker.lm2 ??= levelMaker.GetComponent<scrLevelMaker2>();
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
        ADOBase.conductor.onBeats.Clear();
        scrFloor removedFloor = levelMaker.listFloors[floor + size - 1];
        for(int i = 0; i < size - 1; i++) Object.DestroyImmediate(levelMaker.listFloors[floor + i].gameObject);
        levelMaker.listFloors.RemoveRange(floor, size);
        try {
            scrFloor prevFloor = levelMaker.listFloors[floor - 1];
            if(floor == levelMaker.listFloors.Count) {
                prevFloor.nextfloor = null;
                prevFloor.isportal = true;
                prevFloor.levelnumber = Portal.EndOfLevel;
                prevFloor.exitangle = prevFloor.entryangle + 3.1415927410125732;
            } else {
                scrFloor curFloor = levelMaker.listFloors[floor];
                float prevFloorAngle = scnGame.instance.levelData.angleData[floor - 1];
                prevFloor.midSpin = prevFloorAngle == 999.0;
                prevFloor.exitangle = prevFloor.midSpin ? prevFloor.entryangle : (-prevFloorAngle + 90.0) * (Math.PI / 180.0);
                prevFloor.nextfloor = curFloor;
                Vector3 addedPos;
                if(prevFloor.midSpin) {
                    curFloor.entryangle = (prevFloor.exitangle + 3.1415927410125732) % 6.2831854820251465;
                    addedPos = levelMaker.listFloors[floor - 2].startPos - curFloor.startPos;
                } else addedPos = prevFloor.startPos - removedFloor.startPos;
                for(int i = floor; i < levelMaker.listFloors.Count; i++) {
                    scrFloor fl = levelMaker.listFloors[i];
                    fl.seqID = i;
                    fl.editorNumText.letterText.text = i.ToString();
                    fl.transform.position = fl.startPos + addedPos;
                }
            }
        } finally {
            Object.DestroyImmediate(removedFloor.gameObject);
        }
    }
}