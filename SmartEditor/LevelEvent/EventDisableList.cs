using System.Collections.Generic;
using System.Linq;
using JALib.Core.Patch;

namespace SmartEditor.LevelEvent;

public class EventDisableList : Dictionary<string, bool> {
    private string[] values;

    public EventDisableList() => values = [];
    public EventDisableList(IDictionary<string, bool> dict) => SetValues(dict);

    public void SetValues(IDictionary<string, bool> dict) =>
        values = (dict is EventDisableList list ? list.values : dict.Where(kv => kv.Value).Select(kv => kv.Key)).ToArray();

    [JAOverridePatch]
    public new void Clear() => values = [];

    [JAOverridePatch]
    public new bool ContainsKey(string key) => values.Contains(key);

    [JAOverridePatch]
    public new bool ContainsValue(bool value) => true;

    [JAOverridePatch]
    public new bool TryGetValue(string key, out bool value) {
        value = ContainsKey(key);
        return true;
    }

    [JAOverridePatch]
    public new void Add(string key, bool value) {
        if(ContainsKey(key) == value) return;
        values = (value ? values.Append(key) : values.Where(v => v != key)).ToArray();
    }

    [JAOverridePatch]
    public new bool TryAdd(string key, bool value) {
        Add(key, value);
        return true;
    }

    public new bool this[string key] {
        [JAOverridePatch]
        get => ContainsKey(key);
        [JAOverridePatch]
        set => Add(key, value);
    }

    [JAOverridePatch]
    public new bool Remove(string key) {
        if(!ContainsKey(key)) return false;
        values = values.Where(v => v != key).ToArray();
        return true;
    }
}