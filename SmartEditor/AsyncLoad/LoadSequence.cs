using System;

namespace SmartEditor.AsyncLoad;

public abstract class LoadSequence : IDisposable {
    private string _sequenceText;

    public string SequenceText {
        get => _sequenceText;
        set {
            if(value == _sequenceText) return;
            _sequenceText = value;
            LoadScreen.UpdateSequence();
        }
    }

    public LoadSequence() => LoadScreen.AddSequence(this);

    public virtual void Dispose() => LoadScreen.RemoveSequence(this);
}