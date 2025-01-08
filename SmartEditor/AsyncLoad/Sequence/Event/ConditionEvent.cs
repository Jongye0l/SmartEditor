using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using JALib.Tools;

namespace SmartEditor.AsyncLoad.Sequence.Event;

public class ConditionEvent : LoadSequence {
    public SetupEvent setupEvent;
    public Dictionary<int, Dictionary<string, Tuple<int, float, bool>>> repeatEventData;
    public Dictionary<int, string[]> conditionalEventData;

    public ConditionEvent(SetupEvent setupEvent) {
        this.setupEvent = setupEvent;
        SequenceText = Main.Instance.Localization["AsyncMapLoad.ApplyEvent"];
        JATask.Run(Main.Instance, ApplyEvent);
    }

    public void ApplyEvent() {
        List<LevelEvent> events = scnGame.instance.levelData.levelEvents;
        List<scrFloor> floors = scrLevelMaker.instance.listFloors;
        repeatEventData = new Dictionary<int, Dictionary<string, Tuple<int, float, bool>>>();
        conditionalEventData = new Dictionary<int, string[]>();
        string text = Main.Instance.Localization["AsyncMapLoad.ApplyEvent7"];
        for(int i = 0; i < events.Count; i++) {
            LevelEvent levelEvent = events[i];
            if(!levelEvent.active) continue;
            if(levelEvent.info.type == LevelEventType.RepeatEvents) {
                SequenceText = string.Format(text, i, events.Count);
                bool isBeat = (RepeatType) levelEvent.data["repeatType"] == RepeatType.Beat;
                int repeat = isBeat ? Convert.ToInt32(levelEvent.data["repetitions"]) : Convert.ToInt32(levelEvent.data["floorCount"]);
                float interval = isBeat ? levelEvent.GetFloat("interval") : -1f;
                string[] tags = (levelEvent.GetString("tag") ?? "").Split(" ");
                bool executeOnCurrentFloor = levelEvent.GetBool("executeOnCurrentFloor");
                if(!repeatEventData.ContainsKey(levelEvent.floor)) repeatEventData[levelEvent.floor] = new Dictionary<string, Tuple<int, float, bool>>();
                foreach(string tag in tags) repeatEventData[levelEvent.floor][tag] = new Tuple<int, float, bool>(repeat, interval, executeOnCurrentFloor);
            }
            if(levelEvent.info.type == LevelEventType.SetConditionalEvents) {
                SequenceText = string.Format(text, i, events.Count);
                string[] strArray = [
                    levelEvent.GetString("perfectTag"),
                    levelEvent.GetString("earlyPerfectTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("hitTag"),
                    levelEvent.GetString("latePerfectTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("hitTag"),
                    levelEvent.GetString("veryEarlyTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("barelyTag"),
                    levelEvent.GetString("veryLateTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("barelyTag"),
                    levelEvent.GetString("tooEarlyTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("missTag"),
                    levelEvent.GetString("tooLateTag").NullIfEmptyConditionalTag() ?? levelEvent.GetString("missTag"),
                    levelEvent.GetString("lossTag"),
                    levelEvent.GetString("onCheckpointTag")
                ];
                conditionalEventData.Add(levelEvent.floor, strArray);
                floors[levelEvent.floor].hasConditionalChange = true;
            }
        }
        SequenceText = string.Format(text, events.Count, events.Count);
        MainThread.Run(Main.Instance, ApplyMainThreadEvent);
    }

    public void ApplyMainThreadEvent() {
        LevelData levelData = scnGame.instance.levelData;
        List<scrFloor> floors = scrLevelMaker.instance.listFloors;
        float pitch = scrConductor.instance.song.pitch * GCS.currentSpeedTrial;
        string text = Main.Instance.Localization["AsyncMapLoad.ApplyEvent8"];
        for(int i2 = 0; i2 < levelData.levelEvents.Count; i2++) {
            LevelEvent evnt = levelData.levelEvents[i2];
            if(!evnt.active) continue;
            SequenceText = string.Format(text, i2, levelData.levelEvents.Count);
            int repeat = 0;
            float interval = 0.0f;
            bool executeOnCurrentFloor = false;
            int id = evnt.floor;
            evnt.data.TryGetValue("eventTag", out object obj);
            if(obj != null) {
                string[] tags = obj.ToString().Split(" ");
                if(repeatEventData.TryGetValue(id, out Dictionary<string, Tuple<int, float, bool>> data)) {
                    foreach(string tag in tags) {
                        if(!data.TryGetValue(tag, out Tuple<int, float, bool> tuple)) continue;
                        repeat = tuple.Item1;
                        interval = tuple.Item2;
                        executeOnCurrentFloor = tuple.Item3;
                        break;
                    }
                }
            }
            scrFloor floor = floors[evnt.floor];
            for(int i = 0; i <= repeat; ++i) {
                bool existInterval = interval > 0.0;
                int intervalFloorId = floor.seqID + (existInterval ? 0 : i);
                if(intervalFloorId < floors.Count) {
                    scrFloor fl = floors[intervalFloorId];
                    float offset = existInterval ? interval * i * 180 : executeOnCurrentFloor ? 0 : (float) (fl.entryBeat - floor.entryBeat) * 180;
                    ffxPlusBase ffxPlusBase = scnGame.ApplyEvent(evnt, levelData.bpm, pitch, floors, offset, executeOnCurrentFloor ? fl.seqID : null);
                    if(!EditorConstants.soloTypes.Contains(evnt.eventType) && evnt.eventType != LevelEventType.RepeatEvents) {
                        if(!conditionalEventData.ContainsKey(id)) continue;
                        if(ffxPlusBase) {
                            bool[] conditionalInfo = new bool[9];
                            bool usedEventTag = false;
                            for(int index3 = 0; index3 < conditionalEventData[id].Length; ++index3) {
                                string s = conditionalEventData[id][index3];
                                if(conditionalInfo[index3] = !s.IsNoneConditionalTag() && evnt.GetString("eventTag") == s) usedEventTag = true;
                            }
                            if(!usedEventTag) continue;
                            ffxPlusBase.conditionalInfo = conditionalInfo;
                        }
                    }
                }
                break;
            }
        }
        Dispose();
    }

    public override void Dispose() {
        base.Dispose();
        setupEvent.conditionEvent = null;
        setupEvent.UpdateDispose();
    }
}