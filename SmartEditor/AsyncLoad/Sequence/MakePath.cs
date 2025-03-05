using System.Collections.Generic;
using JALib.Tools;
using UnityEngine;

namespace SmartEditor.AsyncLoad.Sequence;

public class MakePath : LoadSequence {
    public SetupTileData setupTileData;
    public SetupEvent setupEvent;
    public bool makingPath;
    public bool angleDataEnd;

    public MakePath() {
        ADOBase.conductor.onBeats.Clear();
        setupTileData = new SetupTileData(this);
        setupEvent = new SetupEvent();
    }

    public void AddTileCount() {
        lock(this) {
            if(makingPath || scrLevelMaker.instance.listFloors.Count > scnGame.instance.levelData.angleData.Count) return;
            makingPath = true;
        }
        MainThread.Run(Main.Instance, MakeTile);
    }

    public void FinishTileLoad() {
        lock(this) angleDataEnd = true;
    }

    public void FinishEventLoad() {
        setupEvent.Setup();
    }

    public void FinishSettingLoad() {
        SequenceText = Main.Instance.Localization["AsyncMapLoad.SetupSettings"];
        MainThread.Run(Main.Instance, SetupSettings);
        scnGame game = scnGame.instance;
        game.SetValue("backgroundsLoaded", true);
        game.SetValue("floorSpritesLoaded", true);
        ADOBase.conductor.SetupConductorWithLevelData(game.levelData);
        DiscordController.instance?.UpdatePresence();
    }

    public void SetupSettings() {
        scnEditor.instance.UpdateSongAndLevelSettings();
        scnGame game = scnGame.instance;
        game.ReloadSong(true);
        game.SetBackground();
        game.UpdateVideo();
        if(angleDataEnd && scrLevelMaker.instance.listFloors.Count <= game.levelData.angleData.Count) {
            SequenceText = null;
            Dispose();
        }
    }

    public void MakeTile() {
        GameObject gameObject1 = GameObject.Find("Floors");
        List<scrFloor> listFloors = scrLevelMaker.instance.listFloors;
        List<float> angleData = scnGame.instance.levelData.angleData;
Restart:
        for(int i = listFloors.Count; i < angleData.Count + 1; i++) {
            GameObject gameObject3 = Object.Instantiate(scrLevelMaker.instance.meshFloor, Vector3.zero, Quaternion.identity);
            gameObject3.gameObject.transform.parent = gameObject1.transform;
            scrFloor floor = gameObject3.GetComponent<scrFloor>();
            listFloors.Add(floor);
            setupTileData.AddTileCount();
            floor.GetOrAddComponent<ffxChangeTrack>();
        }
        bool end;
        lock(this) {
            if(listFloors.Count > angleData.Count) makingPath = false;
            else goto Restart;
            end = angleDataEnd;
        }
        int angleCount = angleData.Count + 1;
        if(angleCount < listFloors.Count) {
            for(int i = angleCount; i < listFloors.Count; i++) {
                scrFloor floor = listFloors[i];
                if(floor) Object.DestroyImmediate(floor.gameObject);
            }
            listFloors.RemoveRange(angleCount, listFloors.Count - angleCount);
        }
        SequenceText = end ? null : string.Format(Main.Instance.Localization["AsyncMapLoad.MakeTileObject"], listFloors.Count, angleCount + '+');
        if(SequenceText == null) Dispose();
    }
}