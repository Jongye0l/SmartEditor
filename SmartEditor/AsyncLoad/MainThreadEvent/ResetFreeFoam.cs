using System.Threading.Tasks;

namespace SmartEditor.AsyncLoad.MainThreadEvent;

public class ResetFreeFoam : ApplyMainThread {
    public TaskCompletionSource<bool> tcs = new();

    public override void Run() {
        scrLevelMaker.instance.ClearFreeroam();
        tcs.SetResult(true);
    }
}