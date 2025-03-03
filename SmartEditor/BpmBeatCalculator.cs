using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using ADOFAI.LevelEditor.Controls;
using JALib.Core;
using JALib.Core.Patch;
using UnityEngine;

namespace SmartEditor;

public class BpmBeatCalculator() : Feature(Main.Instance, nameof(BpmBeatCalculator), patchClass: typeof(BpmBeatCalculator)) {
    protected override void OnEnable() {
        LevelEventInfo levelEventInfo = GCS.levelEventsInfo["SetSpeed"];
        List<PropertyInfo> propertyInfos = levelEventInfo.propertiesInfo.Values.ToList();
        PropertyInfo propertyInfo = new(new Dictionary<string, object> {
            { "name", "realBPM" },
            { "type", "Float" },
            { "default", 100f },
            { "unit", "bpm" },
            { "key", "jamod.SmartEditor.BpmBeatCalculator.RealBPM" }
        }, levelEventInfo) {
            order = 0
        };
        propertyInfos.Insert(2, propertyInfo);
        levelEventInfo.propertiesInfo.Clear();
        foreach(PropertyInfo info in propertyInfos) levelEventInfo.propertiesInfo.TryAdd(info.name, info);
        foreach(LevelEventInfo eventInfo in GCS.levelEventsInfo.Values) {
            if(!eventInfo.propertiesInfo.TryGetValue("duration", out PropertyInfo duration)) continue;
            propertyInfos = eventInfo.propertiesInfo.Values.ToList();
            propertyInfo = new PropertyInfo(new Dictionary<string, object> {
                { "name", "durationAngle" },
                { "type", duration.type.ToString() },
                { "default", duration.type == PropertyType.Int ? (int) duration.value_default * 180 : (float) duration.value_default * 180 },
                { "unit", "\u00b0" },
                { "key", "editor.duration" }
            }, eventInfo) {
                order = 0
            };
            for(int i = 0; i < eventInfo.propertiesInfo.Count; i++) {
                if(propertyInfos[i].name == "duration") {
                    propertyInfos.Insert(i + 1, propertyInfo);
                    break;
                }
            }
            eventInfo.propertiesInfo.Clear();
            foreach(PropertyInfo info in propertyInfos) eventInfo.propertiesInfo.TryAdd(info.name, info);
        }
    }

    protected override void OnDisable() {
        LevelEventInfo levelEventInfo = GCS.levelEventsInfo["SetSpeed"];
        levelEventInfo.propertiesInfo.Remove("realBPM");
        levelEventInfo = GCS.levelEventsInfo["Pause"];
        levelEventInfo.propertiesInfo.Remove("durationAngle");
    }

    [JAPatch(typeof(PropertyControl_Text), nameof(ValidateInput), PatchType.Postfix, false)]
    private static void ValidateInput(PropertyControl_Text __instance) {
        scnEditor editor = scnEditor.instance;
        LevelEvent curEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
        scrFloor curFloor = editor.floors[curEvent.floor];
        if(__instance.propertyInfo.levelEventInfo.type == LevelEventType.SetSpeed) {
            float bpm = curFloor.prevfloor.speed * editor.levelData.bpm;
            List<LevelEvent> levelEvents = editor.events.Where(editorEvent => editorEvent.floor == curFloor.seqID && editorEvent.eventType == LevelEventType.SetSpeed).ToList();
            bool nextEvent = false;
            bool useAngle = __instance.propertyInfo.name == "angleOffset" && float.TryParse(__instance.text, out float angleOffset) && angleOffset != 0;
            float eventAngle = curEvent.GetFloat("angleData");
            foreach(LevelEvent levelEvent in levelEvents) {
                if(levelEvent == curEvent) {
                    nextEvent = true;
                    continue;
                }
                float curAngle = levelEvent.GetFloat("angleData");
                if(curAngle != 0) useAngle = true;
                if(curAngle > eventAngle || Mathf.Approximately(curAngle, eventAngle) && nextEvent) continue;
                switch (levelEvent["speedType"]) {
                    case SpeedType.Bpm:
                        bpm = levelEvent.GetFloat("beatsPerMinute");
                        break;
                    case SpeedType.Multiplier:
                        bpm *= levelEvent.GetFloat("bpmMultiplier");
                        break;
                }
            }
            switch (__instance.propertyInfo.name) {
                case "beatsPerMinute":
                    float multiplier = (float) Math.Round(float.Parse(__instance.text) / bpm, 6);
                    bpm = float.Parse(__instance.text);
                    curEvent["bpmMultiplier"] = multiplier;
                    __instance.propertiesPanel.properties["bpmMultiplier"].control.text = multiplier.ToString();
                    if(!useAngle) SetRealBPM(bpm, curEvent, __instance.propertiesPanel);
                    break;
                case "bpmMultiplier":
                    bpm *= float.Parse(__instance.text);
                    bpm = (float) Math.Round(bpm, 6);
                    curEvent["beatsPerMinute"] = bpm;
                    __instance.propertiesPanel.properties["beatsPerMinute"].control.text = bpm.ToString();
                    if(!useAngle) SetRealBPM(bpm, curEvent, __instance.propertiesPanel);
                    break;
                case "realBPM":
                    float bpm2 = float.Parse(__instance.text);
                    bpm2 = (float) Math.Round(bpm2 * (curFloor.nextfloor ? Utility.GetAngle(curFloor) / 180 : 1), 6);
                    curEvent["beatsPerMinute"] = bpm2;
                    __instance.propertiesPanel.properties["beatsPerMinute"].control.text = bpm2.ToString();
                    bpm = (float) Math.Round(bpm2 / bpm, 6);
                    curEvent["bpmMultiplier"] = bpm;
                    __instance.propertiesPanel.properties["bpmMultiplier"].control.text = bpm.ToString();
                    break;
            }
        } else if(__instance.propertyInfo.type == PropertyType.Int) {
            int beat, angle;
            switch(__instance.propertyInfo.name) {
                case "duration":
                    beat = int.Parse(__instance.text);
                    angle = beat * (__instance.propertyInfo.levelEventInfo.type == LevelEventType.Hold ? 360 : 180);
                    curEvent["durationAngle"] = angle;
                    __instance.propertiesPanel.properties["durationAngle"].control.text = angle.ToString();
                    break;
                case "durationAngle":
                    angle = int.Parse(__instance.text);
                    int split = __instance.propertyInfo.levelEventInfo.type == LevelEventType.Hold ? 360 : 180;
                    beat = Mathf.RoundToInt((float) angle / split);
                    angle = beat * split;
                    curEvent["duration"] = beat;
                    __instance.propertiesPanel.properties["duration"].control.text = beat.ToString();
                    curEvent["durationAngle"] = angle;
                    __instance.text = angle.ToString();
                    break;
            }
        } else if(__instance.propertyInfo.type == PropertyType.Float) {
            float beat, angle;
            switch(__instance.propertyInfo.name) {
                case "duration":
                    beat = float.Parse(__instance.text);
                    angle = beat * 180;
                    curEvent["durationAngle"] = angle;
                    __instance.propertiesPanel.properties["durationAngle"].control.text = angle.ToString();
                    break;
                case "durationAngle":
                    angle = float.Parse(__instance.text);
                    beat = angle / 180;
                    curEvent["duration"] = beat;
                    __instance.propertiesPanel.properties["duration"].control.text = beat.ToString();
                    break;
            }
        }
    }

    private static void SetRealBPM(float bpm, LevelEvent speedEvent, PropertiesPanel panel) {
        scrFloor floor = scnEditor.instance.floors[speedEvent.floor];
        float realBPM = (float) (bpm / (floor.nextfloor ? Utility.GetAngle(floor) / 180 : 1));
        realBPM = (float) Math.Round(realBPM, 6);
        speedEvent["realBPM"] = realBPM;
        panel.properties["realBPM"].control.text = realBPM.ToString();
    }

    [JAPatch(typeof(PropertyControl_Toggle), nameof(OnSelectedEventChanged), PatchType.Postfix, false)]
    private static void OnSelectedEventChanged(PropertyControl_Toggle __instance) {
        scnEditor editor = scnEditor.instance;
        if(!editor.initialized) return;
        LevelEvent curEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
        scrFloor curFloor = editor.floors[curEvent.floor];
        if(__instance.propertyInfo.levelEventInfo.type == LevelEventType.SetSpeed) {
            List<LevelEvent> levelEvents = editor.events.Where(editorEvent => editorEvent.floor == curFloor.seqID && editorEvent.eventType == LevelEventType.SetSpeed).ToList();
            float bpm = curFloor.prevfloor.speed * editor.levelData.bpm;
            bool nextEvent = false;
            bool useAngle = __instance.propertyInfo.name == "angleOffset" && float.TryParse(__instance.text, out float angleOffset) && angleOffset != 0;
            float eventAngle = curEvent.GetFloat("angleData");
            foreach(LevelEvent levelEvent in levelEvents) {
                if(levelEvent == curEvent) {
                    nextEvent = true;
                    continue;
                }
                float curAngle = levelEvent.GetFloat("angleData");
                if(curAngle != 0) useAngle = true;
                if(curAngle > eventAngle || Mathf.Approximately(curAngle, eventAngle) && nextEvent) continue;
                switch (levelEvent["speedType"]) {
                    case SpeedType.Bpm:
                        bpm = levelEvent.GetFloat("beatsPerMinute");
                        break;
                    case SpeedType.Multiplier:
                        bpm *= levelEvent.GetFloat("bpmMultiplier");
                        break;
                }
            }
            if((SpeedType) curEvent["speedType"] == SpeedType.Bpm) {
                float rbpm = curEvent.GetFloat("beatsPerMinute");
                curEvent["bpmMultiplier"] = (float) Math.Round(rbpm / bpm, 6);
                if(!useAngle) SetRealBPM(rbpm, curEvent, __instance.propertiesPanel);
            } else {
                bpm *= curEvent.GetFloat("bpmMultiplier");
                curEvent["beatsPerMinute"] = (float) Math.Round(bpm, 6);
                if(!useAngle) SetRealBPM(bpm, curEvent, __instance.propertiesPanel);
            }
        }
    }


    [JAPatch(typeof(PropertyControl), nameof(UpdateEnabled), PatchType.Prefix, false)]
    private static bool UpdateEnabled(PropertyControl __instance) {
        switch(__instance.propertyInfo.name) {
            case "beatsPerMinute" or "bpmMultiplier":
                __instance.SetEnabled(true);
                return false;
            case "realBPM":
                LevelEvent curEvent = __instance.propertiesPanel.inspectorPanel.selectedEvent;
                bool useAngle = __instance.propertyInfo.name == "angleOffset" && float.TryParse(__instance.text, out float angleOffset) && angleOffset != 0;
                float eventAngle = curEvent.GetFloat("angleData");
                float bpm = scnEditor.instance.floors[curEvent.floor].prevfloor.speed * scnEditor.instance.levelData.bpm;
                foreach(LevelEvent levelEvent in scnEditor.instance.events) {
                    if(levelEvent.floor != curEvent.floor || levelEvent.eventType != LevelEventType.SetSpeed) continue;
                    float curAngle = levelEvent.GetFloat("angleData");
                    if(curAngle != 0) {
                        useAngle = true;
                        break;
                    }
                    if(curAngle > eventAngle || Mathf.Approximately(curAngle, eventAngle)) continue;
                    switch (levelEvent["speedType"]) {
                        case SpeedType.Bpm:
                            bpm = levelEvent.GetFloat("beatsPerMinute");
                            break;
                        case SpeedType.Multiplier:
                            bpm *= levelEvent.GetFloat("bpmMultiplier");
                            break;
                    }
                }
                __instance.SetShown(!useAngle);
                return false;
            default:
                return true;
        }
    }

    [JAPatch(typeof(PropertiesPanel), nameof(SetProperties), PatchType.Postfix, false)]
    private static void SetProperties(LevelEvent levelEvent, PropertiesPanel __instance) {
        if(!__instance.properties.TryGetValue("durationAngle", out Property durationAngle)) return;
        Property duration = __instance.properties["duration"];
        durationAngle.control.text = duration.info.type == PropertyType.Int ? (int.Parse(duration.control.text) * (levelEvent.eventType == LevelEventType.Hold ? 360 : 180)).ToString() :
                                         (float.Parse(duration.control.text) * 180).ToString();
    }
}