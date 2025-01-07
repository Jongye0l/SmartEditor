using System.Collections.Generic;
using ADOFAI;
using UnityEngine;

namespace SmartEditor.AsyncLoad.MainThreadEvent;

public class ApplyFreeRoamEvent : ApplyMainThread {
    public scrFloor floor;
    public List<LevelEvent> events;

    public ApplyFreeRoamEvent(scrFloor floor, List<LevelEvent> events) {
        this.floor = floor;
        this.events = events;
    }

    public override void Run() {
        foreach(LevelEvent levelEvent in events) {
            Vector2 position;
            int index;
            scrLevelMaker lm = scrLevelMaker.instance;
            switch(levelEvent.eventType) {
                case LevelEventType.FreeRoam:
                    lm.MakeFreeroamGrid(floor);
                    continue;
                case LevelEventType.FreeRoamTwirl:
                    position = (Vector2) levelEvent["position"];
                    index = (int) floor.freeroamDimensions.x * (int) position.y + (int) position.x;
                    if(index < floor.freeroamDimensions.x * (double) floor.freeroamDimensions.y) {
                        scrFloor freeroam = lm.listFreeroam[floor.freeroamRegion][index];
                        freeroam.floorIcon = FloorIcon.Swirl;
                        freeroam.UpdateIconSprite();
                        freeroam.isSwirl = true;
                    }
                    continue;
                case LevelEventType.FreeRoamRemove:
                    position = (Vector2) levelEvent["position"];
                    Vector2 size = (Vector2) levelEvent["size"];
                    for(int y = (int) position.y; y < (int) position.y + (int) size.y; ++y) {
                        for(int x = (int) position.x; x < (int) position.x + (int) size.x; ++x) {
                            index = (int) floor.freeroamDimensions.x * y + x;
                            if(index < floor.freeroamDimensions.x * floor.freeroamDimensions.y) {
                                scrFloor freeroam = lm.listFreeroam[floor.freeroamRegion][index];
                                freeroam.isLandable = false;
                                freeroam.transform.position = Vector3.one * 99999f;
                                freeroam.freeroamRemoved = true;
                            }
                        }
                    }
                    continue;
                case LevelEventType.FreeRoamWarning:
                    position = (Vector2) levelEvent["position"];
                    index = (int) floor.freeroamDimensions.x * (int) position.y + (int) position.x;
                    if(index < floor.freeroamDimensions.x * floor.freeroamDimensions.y) lm.listFreeroam[floor.freeroamRegion][index].isWarning = true;
                    continue;
                default:
                    continue;
            }
        }
    }
}