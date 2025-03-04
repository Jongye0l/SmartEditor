using System.Collections.Generic;
using ADOFAI;
using DG.Tweening;
using JALib.Core;
using SmartEditor.AsyncLoad.Sequence.Event;

namespace SmartEditor.AsyncLoad.Sequence;

public class SetupEvent : LoadSequence {
    public SetupEventMainThread setupEventMainThread = new();
    public CoreEvent coreEvent;
    public TileEntryTime tileEntryTime;
    public GenericEvent genericEvent;
    public FreeRoamEvent freeRoamEvent;
    public EventIcon eventIcon;
    public ConditionEvent conditionEvent;
    public List<LevelEvent>[] floorEvents;
    public int updatedTile;
    public bool updated;

    public void AddSetupTile(int tile) {
        updatedTile = tile;
        coreEvent?.LoadEvent();
    }

    public void Setup() {
        JALocalization localization = Main.Instance.Localization;
        SequenceText = localization["AsyncMapLoad.ApplyEvent"];
        scrLevelMaker lm = scrLevelMaker.instance;
        List<scrFloor> floors = lm.listFloors;
        LevelData levelData = scnGame.instance.levelData;
        List<LevelEvent> events = levelData.levelEvents;
        scrConductor.instance.countdownTicks = levelData.countdownTicks;
        floorEvents = new List<LevelEvent>[levelData.angleData.Count + 1];
        for(int index = 0; index < floorEvents.Length; ++index) floorEvents[index] = [];
        foreach(LevelEvent levelEvent in events) floorEvents[levelEvent.floor].Add(levelEvent);
        coreEvent = new CoreEvent(this);
        freeRoamEvent = new FreeRoamEvent(this);
        eventIcon = new EventIcon(this);
        if(!scrController.instance.paused) conditionEvent = new ConditionEvent(this);
        tileEntryTime = new TileEntryTime(this);
        genericEvent = new GenericEvent(this);
        ffxFlashPlus.legacyFlash = levelData.legacyFlash;
        ffxCameraPlus.legacyRelativeTo = levelData.legacyCamRelativeTo;
        scrVfx.instance.currentColourScheme.colourText = levelData.defaultTextColor;
        scrVfx.instance.currentColourScheme.colourTextShadow = levelData.defaultTextShadowColor;
        if(scrController.instance.paused) return;
        ffxCameraPlus camera = floors[0].GetComponent<ffxCameraPlus>();
        floors[0].plusEffects.Insert(0, camera);
        camera.startTime = 0.0;
        camera.duration = 0.0f;
        camera.targetPos = levelData.camPosition * 1.5f;
        camera.targetRot = levelData.camRotation;
        camera.targetZoom = levelData.camZoom / 100f;
        camera.ease = Ease.Linear;
        camera.movementType = levelData.camRelativeTo;
        camera.dontDisable = levelData.camEnabledOnLowVFX;
        updated = true;
        Dispose();
        UpdateDispose();
    }

    public void OnCoreEventUpdate(int floor) {
        tileEntryTime?.LoadEvent(floor);
        genericEvent?.LoadEvent();
    }

    public void UpdateDispose() {
        if(updated && coreEvent == null && tileEntryTime == null && genericEvent == null && freeRoamEvent == null && eventIcon == null && conditionEvent == null)
            setupEventMainThread.End();
    }
}