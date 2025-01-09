using System.Collections.Concurrent;
using JALib.Tools;

namespace SmartEditor.AsyncLoad.Sequence;

public class SetupEventMainThread : LoadSequence {
    public ConcurrentQueue<ApplyMainThread> apply = [];
    public ConcurrentQueue<bool> requestApplySprite = [];
    public int curApplySprite;
    public bool running;
    public bool finish;


    public void AddEvent(ApplyMainThread apply) {
        this.apply.Enqueue(apply);
        CheckRunning();
    }

    public void AddRequestSprite(bool value) {
        requestApplySprite.Enqueue(value);
        CheckRunning();
    }

    private void CheckRunning() {
        lock(this) {
            if(running) return;
            running = true;
        }
        MainThread.Run(Main.Instance, Run);
    }

    public void End() {
        lock(this) {
            if(running) finish = true;
            else Dispose();
        }
    }

    public void Run() {
Restart:
        while(apply.TryDequeue(out ApplyMainThread result)) result.Run();
        for(; requestApplySprite.TryDequeue(out bool result); curApplySprite++) {
            scrFloor floor = scrLevelMaker.instance.listFloors[curApplySprite];
            floor.UpdateIconSprite();
            floor.UpdateCommentGlow(scrController.instance.paused & result);
        }
        if(!apply.IsEmpty || !requestApplySprite.IsEmpty) goto Restart;
        bool end;
        lock(this) {
            if(!apply.IsEmpty || !requestApplySprite.IsEmpty) goto Restart;
            end = finish;
            running = false;
        }
        if(end) Dispose();
        else SequenceText = string.Format(Main.Instance.Localization["AsyncMapLoad.AddEvent"], curApplySprite, scnGame.instance.levelData.angleData.Count + 1);
    }
}