namespace SmartEditor.AsyncLoad.MainThreadEvent;

public class ResetFreeFoam : ApplyMainThread {
    public override void Run() {
        scrLevelMaker.instance.ClearFreeroam();
    }
}