using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartEditor.FixLoad;

public class RotateTileUpdate {
    public static void UpdateTile(int floor, int size, bool cw) {
        try {
            scnGame game = scnGame.instance;
            scrLevelMaker levelMaker = scrLevelMaker.instance;
            levelMaker.leveldata = game.levelData.pathData;
            levelMaker.isOldLevel = game.levelData.isOldLevel;
            MakeLevel(floor, size, cw); //game.levelMaker.MakeLevel();
            levelMaker.DrawHolds();
            levelMaker.DrawMultiPlanet();
            FixChartLoad.DrawEditor();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public static void UpdateTileSelection(bool cw) {
        List<scrFloor> selectedFloors = scnEditor.instance.selectedFloors;
        UpdateTile(selectedFloors[0].seqID, selectedFloors.Count, cw);
    }

    public static void MakeLevel(int floor, int size, bool cw) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        if(levelMaker.isOldLevel) levelMaker.InstantiateStringFloors();
        else InstantiateFloatFloors(floor, size, cw); //levelMaker.InstantiateFloatFloors();
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

    public static void InstantiateFloatFloors(int floor, int size, bool cw) {
        scrLevelMaker levelMaker = scrLevelMaker.instance;
        scrFloor prevFloor = levelMaker.listFloors[floor - 1];
        Vector3 original = prevFloor.startPos;
        for(int i = 0; i < size; i++) {
            scrFloor curFloor = levelMaker.listFloors[floor + i];
            if(!prevFloor.midSpin) {
                prevFloor.exitangle += cw ? 1.5707963705062866 : -1.5707963705062866;
                curFloor.floatDirection -= cw ? 90 : -90;
                curFloor.entryangle += cw ? 1.5707963705062866 : -1.5707963705062866;
                if(curFloor.midSpin) {
                    curFloor.exitangle = curFloor.entryangle;
                    curFloor.nextfloor.entryangle += cw ? 1.5707963705062866 : -1.5707963705062866;
                }
            }
            prevFloor = curFloor;
        }
        if(prevFloor.isportal) prevFloor.exitangle = prevFloor.entryangle + 3.1415927410125732;
        Vector3 change = default;
        for(int i = floor; i < levelMaker.listFloors.Count; i++) {
            scrFloor fl = levelMaker.listFloors[i];
            if(i < floor + size) {
                bool last = i == floor + size - 1;
                if(last) change = fl.startPos;
                Vector3 pos = fl.startPos - original;
                pos = cw ? new Vector3(pos.y, -pos.x, pos.z) : new Vector3(-pos.y, pos.x, pos.z);
                Vector3 added = fl.transform.position - fl.startPos;
                fl.startPos = original + pos;
                fl.transform.position = fl.startPos + added;
                if(last) {
                    change = fl.startPos - change;
                    if(fl.midSpin) {
                        change = fl.nextfloor.startPos - levelMaker.listFloors[i - 1].startPos;
                        change.x = (float) Math.Round(change.x, 3);
                        change.y = (float) Math.Round(change.y, 3);
                    }
                }
            } else {
                fl.startPos += change;
                fl.transform.position += change;
            }
        }
    }
}