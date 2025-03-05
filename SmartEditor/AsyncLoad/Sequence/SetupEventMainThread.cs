using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        try {
            Restart:
            while(apply.TryDequeue(out ApplyMainThread result)) result.Run();
            List<scrFloor> floors = scrLevelMaker.instance.listFloors;
            for(; curApplySprite < floors.Count && requestApplySprite.TryDequeue(out bool result); curApplySprite++) {
                scrFloor floor = floors[curApplySprite];
                floor.UpdateIconSprite();
                floor.UpdateCommentGlow(scrController.instance.paused & result);
            }
            if(!apply.IsEmpty) goto Restart;
            if(!requestApplySprite.IsEmpty) {
                if(curApplySprite < floors.Count) goto Restart;
                Task.Yield().GetAwaiter().UnsafeOnCompleted(Run);
                return;
            }
            bool end;
            lock(this) {
                if(!apply.IsEmpty || !requestApplySprite.IsEmpty) goto Restart;
                end = finish;
                running = false;
            }
            if(end) Dispose();
            else SequenceText = string.Format(Main.Instance.Localization["AsyncMapLoad.AddEvent"], curApplySprite, scnGame.instance.levelData.angleData.Count + 1);
        } catch (Exception e) {
            Main.Instance.LogReportException(e);
        }
    }
}