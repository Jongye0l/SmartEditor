﻿using System;
using System.Collections.Generic;
using ADOFAI;

namespace SmartEditor.FixLoad.CustomSaveState.Scope;

public class DeleteFloorScope : CustomSaveStateScope {
    public int index;
    public float angle;
    public LevelEvent[] events;
    public LevelEvent[] startTileEvents;
    public LevelEvent[] endTileEvents;

    public DeleteFloorScope(int index) {
        if(CreateFloorScope.instance != null) CreateFloorScope.instance.deleted = this;
        scnEditor editor = scnEditor.instance;
        this.index = index;
        angle = editor.levelData.angleData[index - 1];
        List<LevelEvent> events = [];
        List<LevelEvent> startTileEvents = [];
        List<LevelEvent> endTileEvents = [];
        foreach(LevelEvent @event in editor.events) {
            if(@event.floor != index) continue;
            events.Add(@event);
            if(@event.data.TryGetValue("startTile", out object value)) {
                Tuple<int, TileRelativeTo> tile = (Tuple<int, TileRelativeTo>) value;
                if(tile.Item2 == TileRelativeTo.Start && tile.Item1 > index || tile.Item2 == TileRelativeTo.End && tile.Item1 <= index) startTileEvents.Add(@event);
            }
            if(@event.data.TryGetValue("endTile", out value)) {
                Tuple<int, TileRelativeTo> tile = (Tuple<int, TileRelativeTo>) value;
                if(tile.Item2 == TileRelativeTo.Start && tile.Item1 > index || tile.Item2 == TileRelativeTo.End && tile.Item1 <= index) endTileEvents.Add(@event);
            }
        }
        this.events = events.ToArray();
        this.startTileEvents = startTileEvents.ToArray();
        this.endTileEvents = endTileEvents.ToArray();
    }

    public override void Undo() {
        if(index <= 1) return;
        scnEditor editor = scnEditor.instance;
        FixPrivateMethod.OffsetFloorIDsInEvents(index - 1, 1);
        editor.InsertFloatFloor(index - 1, angle);
        if(events.Length > 0) {
            editor.events.AddRange(events);
            foreach(LevelEvent @event in startTileEvents) {
                Tuple<int, TileRelativeTo> tile = (Tuple<int, TileRelativeTo>) @event.data["startTile"];
                if(tile.Item2 == TileRelativeTo.Start) @event.data["startTile"] = (tile.Item1 + 1, tile.Item2);
                else @event.data["startTile"] = (tile.Item1 - 1, tile.Item2);
            }
            foreach(LevelEvent @event in endTileEvents) {
                Tuple<int, TileRelativeTo> tile = (Tuple<int, TileRelativeTo>) @event.data["endTile"];
                if(tile.Item2 == TileRelativeTo.Start) @event.data["endTile"] = (tile.Item1 + 1, tile.Item2);
                else @event.data["endTile"] = (tile.Item1 - 1, tile.Item2);
            }
            scnGame.instance.ApplyEventsToFloors(scrLevelMaker.instance.listFloors);
        }
        scrFloor floor = editor.floors[index];
        editor.SelectFloor(floor);
        FixPrivateMethod.MoveCameraToFloor(floor);
    }

    public override void Redo() {
        if(index <= 1) return;
        FixPrivateMethod.DeleteFloor(index);
    }
}