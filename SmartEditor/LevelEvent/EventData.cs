using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using JALib.Core.Patch;
using JALib.Tools;

namespace SmartEditor.LevelEvent;

public class EventData : Dictionary<string, object>,
    IDictionary<string, object>, IDictionary,
    IReadOnlyDictionary<string, object>, IDeserializationCallback {

    public CustomEvent Event;
    public FieldInfo[] Fields;

    public EventData(CustomEvent @event) {
        Event = @event;
        Fields = @event.GetType().GetFields().Where(f => !f.IsStatic && f.DeclaringType != typeof(ADOFAI.LevelEvent)).ToArray();
    }

    public new int Count {
        [JAOverridePatch]
        get => Fields.Length;
    }

    ICollection<string> IDictionary<string, object>.Keys {
        [JAOverridePatch]
        get => Fields.Select(f => f.Name).ToArray();
    }

    IEnumerable<string> IReadOnlyDictionary<string, object>.Keys {
        [JAOverridePatch]
        get => Fields.Select(f => f.Name);
    }

    ICollection<object> IDictionary<string, object>.Values {
        [JAOverridePatch]
        get => Fields.Select(f => f.GetValue(Event)).ToArray();
    }

    IEnumerable<object> IReadOnlyDictionary<string, object>.Values {
        [JAOverridePatch]
        get => Fields.Select(f => f.GetValue(Event));
    }

    public new object this[string key] {
        [JAOverridePatch]
        get => Event[key];
        [JAOverridePatch]
        set => Event[key] = value;
    }

    [JAOverridePatch]
    public new void Add(string key, object value) => Event[key] = value;

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> keyValuePair) => Add(keyValuePair.Key, keyValuePair.Value);

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> keyValuePair) =>
        TryGetValue(keyValuePair.Key, out object value) && EqualityComparer<object>.Default.Equals(value, keyValuePair.Value);

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> keyValuePair) => false;

    [JAOverridePatch]
    public new void Clear() => throw new NotSupportedException();

    [JAOverridePatch]
    public new bool ContainsKey(string key) => Event.GetType().Field(key) != null;

    [JAOverridePatch]
    public new bool ContainsValue(object value) => Fields.Any(field => EqualityComparer<object>.Default.Equals(field.GetValue(Event), value));

    private IEnumerator<KeyValuePair<string, object>> ValuePairEnumerator() => Fields.Select(f => new KeyValuePair<string, object>(f.Name, f.GetValue(Event))).GetEnumerator();

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => ValuePairEnumerator();

    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context) => throw new NotSupportedException();

    [JAOverridePatch]
    public virtual new void OnDeserialization(object sender) => throw new NotSupportedException();

    [JAOverridePatch]
    public new bool Remove(string key) {
        Event[key] = null;
        return true;
    }

    [JAOverridePatch]
    public new bool Remove(string key, out object value) {
        value = Event[key];
        Event[key] = null;
        return true;
    }

    [JAOverridePatch]
    public new bool TryGetValue(string key, out object value) {
        FieldInfo field = Event.GetType().Field(key);
        value = field?.GetValue(Event);
        return field != null;
    }

    [JAOverridePatch]
    public new bool TryAdd(string key, object value) {
        FieldInfo field = Event.GetType().Field(key);
        field?.SetValue(Event, value);
        return field != null;
    }

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int index) {
    }

    void ICollection.CopyTo(Array array, int index) {
    }

    IEnumerator IEnumerable.GetEnumerator() => ValuePairEnumerator();

    [JAOverridePatch]
    public new int EnsureCapacity(int capacity) => throw new NotSupportedException();

    [JAOverridePatch]
    public new void TrimExcess(int capacity) => throw new NotSupportedException();

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => throw new NotSupportedException();

    bool IDictionary.IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;

    ICollection IDictionary.Keys => Keys;

    ICollection IDictionary.Values => Values;

    object IDictionary.this[object key] {
        get => this[(string) key];
        set => this[(string) key] = value;
    }

    void IDictionary.Add(object key, object value) => Add((string) key, value);

    bool IDictionary.Contains(object key) => ContainsKey((string) key);

    IDictionaryEnumerator IDictionary.GetEnumerator() => throw new NotSupportedException();

    void IDictionary.Remove(object key) => Remove((string) key);
}