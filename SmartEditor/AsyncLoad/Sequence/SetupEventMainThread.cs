using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using JALib.Tools;

namespace SmartEditor.AsyncLoad.Sequence;

public class SetupEventMainThread : LoadSequence {
    public List<ApplyMainThread> apply = [];
    public int curApply;
    public List<bool> requestApplySprite = [];
    public int curApplySprite;
    public bool running;
    public bool applyEvents;
    public bool applySuccessEvents;
    public bool finish;
    public Dictionary<int, Dictionary<string, Tuple<int, float, bool>>> repeatEventData;
    public Dictionary<int, string[]> conditionalEventData;


    public void AddEvent(ApplyMainThread apply) {
        this.apply.Add(apply);
        CheckRunning();
    }

    public void AddRequestSprite(bool value) {
        requestApplySprite.Add(value);
        CheckRunning();
    }

    public void ApplyEvent() {
        applyEvents = true;
        CheckRunning();
    }

    private void CheckRunning() {
        lock(this) {
            if(running) return;
            running = true;
        }
        MainThread.Run(Main.Instance, Run);
    }

    public void Run() {
Restart:
        for(; curApply < apply.Count; curApply++) apply[curApply].Run();
        for(; curApplySprite < requestApplySprite.Count; curApplySprite++) {
            scrFloor floor = scrLevelMaker.instance.listFloors[curApplySprite];
            floor.UpdateIconSprite();
            floor.UpdateCommentGlow(scrController.instance.paused & requestApplySprite[curApply]);
        }
        if(curApply < apply.Count || curApplySprite < requestApplySprite.Count) goto Restart;
        if(applyEvents) {
            LevelData levelData = scnGame.instance.levelData;
            List<scrFloor> floors = scrLevelMaker.instance.listFloors;
            float pitch = scrConductor.instance.song.pitch * GCS.currentSpeedTrial;
            foreach(LevelEvent evnt in levelData.levelEvents) {
                if(!evnt.active) continue;
                int repeat = 0;
                float interval = 0.0f;
                bool executeOnCurrentFloor = false;
                int floor3 = evnt.floor;
                evnt.data.TryGetValue("eventTag", out object obj);
                if(obj != null) {
                    string[] tags = (obj as string).Split(" ");
                    if(repeatEventData.TryGetValue(floor3, out Dictionary<string, Tuple<int, float, bool>> data)) {
                        foreach(string tag in tags) {
                            if(!data.TryGetValue(tag, out Tuple<int, float, bool> tuple)) continue;
                            repeat = tuple.Item1;
                            interval = tuple.Item2;
                            executeOnCurrentFloor = tuple.Item3;
                            break;
                        }
                    }
                }
                scrFloor floor4 = floors[evnt.floor];
                for(int i = 0; i <= repeat; ++i) {
                    bool flag19 = interval > 0.0;
                    int index2 = floor4.seqID + (flag19 ? 0 : i);
                    if(index2 < floors.Count) {
                        scrFloor floor5 = floors[index2];
                        float offset = flag19 ? (float) (interval * i * 180.0) : (float) ((executeOnCurrentFloor ? 0.0 : floor5.entryBeat - floor4.entryBeat) * 180.0);
                        ffxPlusBase ffxPlusBase = scnGame.ApplyEvent(evnt, levelData.bpm, pitch, floors, offset, executeOnCurrentFloor ? floor5.seqID : null);
                        if(!EditorConstants.soloTypes.Contains(evnt.eventType) && evnt.eventType != LevelEventType.RepeatEvents) {
                            if(!conditionalEventData.ContainsKey(floor3)) continue;
                            if(ffxPlusBase) {
                                bool[] flagArray = new bool[9];
                                bool flag20 = false;
                                for(int index3 = 0; index3 < conditionalEventData[floor3].Length; ++index3) {
                                    string s = conditionalEventData[floor3][index3];
                                    flagArray[index3] = !s.IsNoneConditionalTag() && evnt.GetString("eventTag") == s;
                                    if(flagArray[index3]) flag20 = true;
                                }
                                if(!flag20) continue;
                                ffxPlusBase.conditionalInfo = flagArray;
                            }
                        }
                    }
                    break;
                }
            }
            scnEditor editor = scnEditor.instance;
            editor.Invoke("DrawFloorOffsetLines");
            scrLevelMaker.instance.DrawHolds();
            editor.Invoke("DrawFloorNums");
            scrLevelMaker.instance.DrawMultiPlanet();
            applyEvents = false;
            applySuccessEvents = true;
        }
        lock(this) {
            if(curApply < apply.Count || curApplySprite < requestApplySprite.Count || applyEvents) goto Restart;
            running = false;
        }
        if(applySuccessEvents) Dispose();
        else {
            int cur = curApply + curApplySprite;
            int total = apply.Count + requestApplySprite.Count + scnGame.instance.levelData.levelEvents.Count;
            SequenceText = string.Format(Main.Instance.Localization["AsyncMapLoad.AddEvent"], cur, total);
        }
    }
}