using System.Collections.Generic;
using ADOFAI;
using JALib.Tools;

namespace SmartEditor.AsyncLoad.Sequence;

public class LoadDecoration : LoadSequence {
    public int cur;
    public bool init;
    public bool running;
    public bool loadComplete;

    public LoadDecoration() {
        MainThread.Run(Main.Instance, Init);
    }

    public void Init() {
        scrDecorationManager.instance.ClearDecorations();
        init = true;
        if(cur < scnEditor.instance.decorations.Count) AddDecoration();
    }

    public void AddDecoration() {
        lock(this) {
            if(!init || running) return;
            running = true;
        }
        MainThread.Run(Main.Instance, LoadDecorationObject);
    }

    public void LoadCompleteDecoration() {
        lock(this) {
            loadComplete = true;
            if(!running) Dispose();
        }
    }

    public void LoadDecorationObject() {
        List<LevelEvent> decorations = scnEditor.instance.decorations;
Restart:
        for(; cur < decorations.Count; cur++) {
            LevelEvent decoration = decorations[cur];
            if(!decoration.active) continue;
            scrDecorationManager.instance.CreateDecoration(decoration, out bool _);
        }
        bool end;
        lock(this) {
            if(cur < decorations.Count) goto Restart;
            end = loadComplete;
            running = end;
        }
        if(end) Dispose();
    }
}