﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ADOFAI;
using Newtonsoft.Json;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence;

public class ReadLevel : LoadSequence {
    public string path;
    public LoadMap loadMap;
    public StreamReader streamReader;
    public JsonTextReader jsonReader;
    public Task<bool> readTask;
    public Action<JsonToken, object> action;
    public MakePath makePath;
    public ReadTexture readTexture;
    public LoadDecoration loadDecoration;

    public ReadLevel(LoadMap map, string path) {
        loadMap = map;
        this.path = path;
    }

    public void Read() {
        LoadResult status = LoadResult.Error;
        try {
            SequenceText = Main.Instance.Localization["AsyncMapLoad.ReadMap"];
            if(ADOBase.isInternalLevel) {
                throw new NotSupportedException("Internal Level is not supported yet.");
                bool complete = scnGame.instance.levelData.LoadLevel(path, out status);
                loadMap.LoadAfter(complete, status, "");
                Dispose();
                return;
            }
            makePath = new MakePath();
            streamReader = new StreamReader(path);
            jsonReader = new JsonTextReader(streamReader);
            readTask = jsonReader.ReadAsync();
            action = ReadActionName;
            readTask.GetAwaiter().UnsafeOnCompleted(OnRead);
        } catch (Exception e) {
            Main.Instance.LogException(e);
            string errorMessage = LoadMap.MakeExceptionMessage(e);
            loadMap.LoadAfter(false, status, errorMessage);
            Dispose();
        }
    }

    public void OnRead() {
        try {
            JsonToken tokenType = jsonReader.TokenType;
            object value = jsonReader.Value;
            readTask = jsonReader.ReadAsync();
            action(tokenType, value);
            readTask.GetAwaiter().UnsafeOnCompleted(OnRead);
        } catch (Exception e) {
            Main.Instance.LogException(e);
            string errorMessage = LoadMap.MakeExceptionMessage(e);
            loadMap.LoadAfter(false, LoadResult.Error, errorMessage);
            Dispose();
        }
    }

    public void ReadActionName(JsonToken tokenType, object value) {
        if(tokenType == JsonToken.StartObject) return;
        if(tokenType == JsonToken.EndObject) {
            Dispose();
            return;
        }
        if(tokenType != JsonToken.PropertyName) throw new Exception("Expected PropertyName");
        string propertyName = value as string;
        action = propertyName switch {
            "angleData" => ReadAngleData,
            "settings" => ReadSettings,
            "actions" => ReadActions,
            "decorations" => ReadDecorations,
            _ => WaitingEndThisProperty
        };
        if(action == WaitingEndThisProperty) Main.Instance.Warning("Unknown Property: " + propertyName);
    }

#region LoadAngleData

    public void ReadAngleData(JsonToken tokenType, object value) {
        List<float> angleData = scnGame.instance.levelData.angleData;
        if(tokenType == JsonToken.StartArray) {
            angleData.Clear();
            SequenceText = Main.Instance.Localization["AsyncMapLoad.ReadMap.AngleData"];
            return;
        }
        if(tokenType == JsonToken.EndArray) {
            action = ReadActionName;
            makePath.FinishTileLoad();
            return;
        }
        angleData.Add(Convert.ToSingle(value));
        makePath.AddTileCount();
    }

#endregion

#region LoadSettings

    public PropertyInfo info;
    public LevelEvent ev;
    public List<object> cacheList;

    public void ReadSettings(JsonToken tokenType, object value) {
        if(tokenType == JsonToken.StartObject) {
            SequenceText = Main.Instance.Localization["AsyncMapLoad.ReadMap.Settings"];
            return;
        }
        if(tokenType == JsonToken.EndObject) {
            action = ReadActionName;
            makePath.FinishSettingLoad();
            return;
        }
        if(tokenType == JsonToken.PropertyName) {
            string propertyName = value as string;
            foreach(LevelEventInfo eventInfo in GCS.settingsInfo.Values)
                if(eventInfo.propertiesInfo.TryGetValue(propertyName, out info)) {
                    LevelData data = scnGame.instance.levelData;
                    ev = eventInfo.name switch {
                        "SongSettings" => data.songSettings,
                        "LevelSettings" => data.levelSettings,
                        "TrackSettings" => data.trackSettings,
                        "BackgroundSettings" => data.backgroundSettings,
                        "CameraSettings" => data.cameraSettings,
                        "MiscSettings" => data.miscSettings,
                        "EventSettings" => data.eventSettings,
                        "DecorationSettings" => data.decorationSettings,
                        _ => null
                    };
                    break;
                }
            return;
        }
        if(cacheList != null && tokenType != JsonToken.EndArray) {
            cacheList.Add(value);
            return;
        }
        if(info == null) return;
        switch(info.type) {
            case PropertyType.Enum:
                value = value is int i ? Enum.ToObject(info.enumType, i) : Enum.Parse(info.enumType, value as string);
                break;
            case PropertyType.Bool:
                if(value is string s) value = s == "Enabled";
                break;
            case PropertyType.Float:
                value = Convert.ToSingle(value);
                break;
            case PropertyType.Int or PropertyType.Rating:
                value = Convert.ToInt32(value);
                break;
            case PropertyType.Vector2:
                if(tokenType == JsonToken.StartArray) {
                    cacheList = new List<object>(2);
                    return;
                }
                value = new Vector2(Convert.ToSingle(cacheList[0]), Convert.ToSingle(cacheList[1]));
                cacheList = null;
                break;
            case PropertyType.Tile:
                if(tokenType == JsonToken.StartArray) {
                    cacheList = new List<object>(2);
                    return;
                }
                value = new Tuple<int, TileRelativeTo>(Convert.ToInt32(cacheList[0]), Enum.Parse<TileRelativeTo>(cacheList[1].ToString()));
                cacheList = null;
                break;
            case PropertyType.Array:
                if(tokenType == JsonToken.StartArray) {
                    cacheList = [];
                    return;
                }
                value = RDEditorUtils.DecodeModsArray(cacheList);
                cacheList = null;
                break;
            // I think FilterProperties is not required
            case PropertyType.List:
                if(tokenType == JsonToken.StartArray) {
                    cacheList = [];
                    return;
                }
                value = cacheList;
                cacheList = null;
                break;
            case PropertyType.FloatPair:
                if(tokenType == JsonToken.StartArray) {
                    cacheList = new List<object>(2);
                    return;
                }
                value = new Tuple<float, float>(Convert.ToSingle(cacheList[0]), Convert.ToSingle(cacheList[1]));
                cacheList = null;
                break;
            // I think MinMaxGradient, Vector2Range is not required
        }
        ev[info.name] = value;
    }

#endregion

#region LoadActions

    public bool isAction;

    public void ReadActions(JsonToken tokenType, object value) {
        if(json.Count != 0) {
            LevelEvent @event = new(json);
            string str = null;
            if(@event.eventType == LevelEventType.ColorTrack) str = @event.data["trackTexture"] as string;
            else if(@event.eventType == LevelEventType.CustomBackground) str = @event.data["bgImage"] as string;
            else if(@event.eventType == LevelEventType.MoveDecorations) str = @event.data["decorationImage"] as string;
            if(!string.IsNullOrEmpty(str)) {
                readTexture ??= new ReadTexture { makePath = makePath };
                readTexture.AddRequest(str);
            }
            if(@event.IsDecoration) {
                loadDecoration ??= new LoadDecoration();
                scnEditor.instance.decorations.Add(@event);
                loadDecoration.AddDecoration();
            } else scnEditor.instance.events.Add(@event);
            json.Clear();
        }
        if(tokenType == JsonToken.EndArray) {
            action = ReadActionName;
            readTexture.FinishLoad();
        } else if(tokenType == JsonToken.StartObject) {
            isAction = true;
            action = ReadEvent;
        }
    }

#endregion

#region LoadDecorations

    public void ReadDecorations(JsonToken tokenType, object value) {
        if(json.Count != 0) {
            scnEditor.instance.decorations.Add(new LevelEvent(json));
            json.Clear();
            loadDecoration.AddDecoration();
        }
        if(tokenType == JsonToken.EndArray) {
            loadDecoration.LoadCompleteDecoration();
            action = ReadActionName;
        }
        else if(tokenType == JsonToken.StartObject) {
            isAction = true;
            action = ReadEvent;
            loadDecoration ??= new LoadDecoration();
        }
    }

#endregion

#region ReadEvent
    public List<object> cachedListOrDict = [];
    public Dictionary<string, object> json = new();
    public string propertyName;
    public readonly object noneObj = new();

    public void ReadEvent(JsonToken tokenType, object value) {
        switch(tokenType) {
            case JsonToken.PropertyName:
                if(cachedListOrDict.Count == 0) propertyName = value as string;
                else if(cachedListOrDict[^1] is Dictionary<string, object> dict) {
                    dict[value as string] = noneObj;
                    return;
                }
                return;
            case JsonToken.StartObject:
                cachedListOrDict.Add(new Dictionary<string, object>());
                return;
            case JsonToken.StartArray:
                cachedListOrDict.Add(new List<object>());
                return;
            case JsonToken.EndObject:
            case JsonToken.EndArray:
                if(cachedListOrDict.Count == 0) {
                    if(isAction) action = ReadActions;
                    else action = ReadDecorations;
                    return;
                }
                value = cachedListOrDict[^1];
                cachedListOrDict.RemoveAt(cachedListOrDict.Count - 1);
                break;
        }
        if(cachedListOrDict.Count == 0) json[propertyName] = value;
        else if(cachedListOrDict[^1] is List<object> list) list.Add(value);
        else if(cachedListOrDict[^1] is Dictionary<string, object> dict) {
            foreach(KeyValuePair<string,object> valuePair in dict) {
                if(valuePair.Value != noneObj) continue;
                dict[valuePair.Key] = value;
                return;
            }
        }
    }

#endregion

#region LoadUnknown

    public int currentCount;

    public void WaitingEndThisProperty(JsonToken tokenType, object value) {
        switch(tokenType) {
            case JsonToken.StartObject:
            case JsonToken.StartArray:
            case JsonToken.StartConstructor:
                if(currentCount == 0) SequenceText = Main.Instance.Localization["AsyncMapLoad.ReadMap.ReadUnknownJson"];
                currentCount++;
                return;
            case JsonToken.EndObject:
            case JsonToken.EndArray:
            case JsonToken.EndConstructor:
                currentCount--;
                return;
        }
        if(currentCount == 0) action = ReadActionName;
    }

#endregion

    public override void Dispose() {
        base.Dispose();
        streamReader?.Close();
        jsonReader?.Close();
        makePath?.Dispose();
    }
}