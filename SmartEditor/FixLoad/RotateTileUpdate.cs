using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartEditor.FixLoad;

public class RotateTileUpdate {
    public static void UpdateTile(int floor, int size, bool cw, bool is180) {
        try {
            scnGame game = scnGame.instance;
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            levelMaker.leveldata = game.levelData.pathData;
            levelMaker.isOldLevel = game.levelData.isOldLevel;
            MakeLevel(floor, size, cw, is180); //game.levelMaker.MakeLevel();
            game.ApplyEventsToFloors(levelMaker.listFloors);
            levelMaker.DrawHolds();
            levelMaker.DrawMultiPlanet();
            FixChartLoad.DrawEditor();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static void UpdateTileSelection(bool cw, bool is180) {
        List<scrFloor> selectedFloors = scnEditor.instance.selectedFloors;
        UpdateTile(selectedFloors[0].seqID, selectedFloors.Count, cw, is180);
    }

    public static void MakeLevel(int floor, int size, bool cw, bool is180) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        if(levelMaker.isOldLevel) levelMaker.InstantiateStringFloors();
        else InstantiateFloatFloors(floor, size, cw, is180); //levelMaker.InstantiateFloatFloors();
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
                if(listFloor.midSpin && index == floor + size - 1) listFloor.nextfloor.UpdateAngle();
            }
        }
    }

    public static void InstantiateFloatFloors(int floor, int size, bool cw, bool is180) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        scrFloor prevFloor = levelMaker.listFloors[floor - 1];
        Vector3 original = prevFloor.startPos;
        float movedAngle = FlipAndRotateTilesAPI.CheckMod() ? FlipAndRotateTilesAPI.CustomAngle ?? 90 : 90;
        for(int i = 0; i < size; i++) {
            scrFloor curFloor = levelMaker.listFloors[floor + i];
            if(!prevFloor.midSpin) {
                SetupAngle(ref prevFloor.exitangle, cw, is180, movedAngle);
                curFloor.floatDirection += is180 ? movedAngle * 2 : cw ? -movedAngle : movedAngle;
                SetupAngle(ref curFloor.entryangle, cw, is180, movedAngle);
                if(curFloor.midSpin) {
                    curFloor.exitangle = curFloor.entryangle;
                    SetupAngle(ref curFloor.exitangle, cw, is180, movedAngle);
                }
            }
            prevFloor = curFloor;
        }
        if(prevFloor.isportal) prevFloor.exitangle = prevFloor.entryangle + 3.14159274101257325;
        Vector3 change = default;
        for(int i = floor; i < levelMaker.listFloors.Count; i++) {
            scrFloor fl = levelMaker.listFloors[i];
            if(i < floor + size) {
                bool last = i == floor + size - 1;
                if(last) change = fl.startPos;
                Vector3 pos;
                if(movedAngle == 90) {
                    pos = fl.startPos - original;
                    pos = is180 ? new Vector3(-pos.x, -pos.y, pos.z) : cw ? new Vector3(pos.y, -pos.x, pos.z) : new Vector3(-pos.y, pos.x, pos.z);
                } else pos = scrMisc.getVectorFromAngle(levelMaker.listFloors[i - 1].exitangle, scrController.instance.startRadius);
                Main.Instance.Log(pos);
                Vector3 added = fl.transform.position - fl.startPos;
                fl.startPos = original + pos;
                fl.transform.position = fl.startPos + added;
                if(last) {
                    change = fl.startPos - change;
                    if(fl.midSpin) {
                        change = fl.nextfloor.startPos - levelMaker.listFloors[i - 1].startPos;
                        change.x = (float) Math.Round(change.x, 6);
                        change.y = (float) Math.Round(change.y, 6);
                    }
                }
            } else {
                fl.startPos += change;
                fl.transform.position += change;
            }
        }
    }

    private static void SetupAngle(ref double value, bool cw, bool is180, float movedAngle) {
        float multiplier = is180 ? movedAngle * 2 / 180 : (cw ? 1 : -1) * movedAngle / 180;
        value += 3.14159274101257325 * multiplier;
        value %= 6.2831854820251465;
    }
}