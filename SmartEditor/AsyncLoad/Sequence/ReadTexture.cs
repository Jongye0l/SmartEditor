using System.Collections.Concurrent;
using System.IO;
using ADOFAI;
using JALib.Tools;

namespace SmartEditor.AsyncLoad.Sequence;

public class ReadTexture : LoadSequence {
    public ConcurrentQueue<string> queue = new();
    public MakePath makePath;
    public int count;
    public bool running;
    public bool runFinishLoad;

    public ReadTexture() {
        if(scnEditor.instance) scnGame.instance.imgHolder.MarkAllUnused();
    }

    public void AddRequest(string path) {
        queue.Enqueue(path);
        lock(this) {
            if(running) return;
            running = true;
        }
        MainThread.Run(Main.Instance, Read);
    }

    public void FinishLoad() {
        lock(this) {
            if(running || queue.Count != 0) {
                runFinishLoad = true;
                return;
            }
        }
        makePath.FinishEventLoad();
        SequenceText = null;
        Dispose();
    }

    public void Read() {
Restart:
        while(queue.TryDequeue(out string path)) {
            scnGame.instance.imgHolder.AddTexture(path, out LoadResult status, Path.Combine(Path.GetDirectoryName(ADOBase.customLevel.levelPath), path))?.GetTexture(scrExtImgHolder.ImageOptions.None);
            ADOBase.editor?.UpdateImageLoadResult(path, status);
            count++;
        }
        lock(this) {
            if(queue.Count > 0) goto Restart;
            running = false;
        }
        if(runFinishLoad) {
            makePath.FinishEventLoad();
            SequenceText = null;
            Dispose();
        } else SequenceText = string.Format(Main.Instance.Localization["AsyncMapLoad.ReadTexture"], count);
    }
}