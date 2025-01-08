using System.Collections.Generic;
using JALib.Tools;

namespace SmartEditor.AsyncLoad.Sequence;

public class SetupEventMainThread : LoadSequence {
    public List<ApplyMainThread> apply = [];
    public int curApply;
    public List<bool> requestApplySprite = [];
    public int curApplySprite;
    public bool running;
    public bool finish;


    public void AddEvent(ApplyMainThread apply) {
        this.apply.Add(apply);
        CheckRunning();
    }

    public void AddRequestSprite(bool value) {
        requestApplySprite.Add(value);
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
        for(; curApply < apply.Count; curApply++) apply[curApply].Run();
        for(; curApplySprite < requestApplySprite.Count; curApplySprite++) {
            scrFloor floor = scrLevelMaker.instance.listFloors[curApplySprite];
            floor.UpdateIconSprite();
            floor.UpdateCommentGlow(scrController.instance.paused & requestApplySprite[curApply]);
        }
        if(curApply < apply.Count || curApplySprite < requestApplySprite.Count) goto Restart;
        bool end;
        lock(this) {
            if(curApply < apply.Count || curApplySprite < requestApplySprite.Count) goto Restart;
            end = finish;
            running = false;
        }
        if(end) Dispose();
        else {
            int cur = curApply + curApplySprite;
            int total = apply.Count + requestApplySprite.Count + scnGame.instance.levelData.levelEvents.Count;
            SequenceText = string.Format(Main.Instance.Localization["AsyncMapLoad.AddEvent"], cur, total);
        }
    }
}