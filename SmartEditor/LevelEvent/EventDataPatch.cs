using System.Collections.Generic;
using System.Reflection;
using JALib.Core.Patch;

namespace SmartEditor.LevelEvent;

public class EventDataPatch {
    [JAPatch(typeof(Dictionary<string, object>.KeyCollection), "CopyTo", PatchType.Prefix, false)]
    public static bool KeyCopyTo(Dictionary<string, object> ____dictionary, string[] array, int index) {
        if(____dictionary is not EventData eventData || array == null || index < 0 || index > array.Length || array.Length - index < ____dictionary.Count) return true;
        int count = eventData.Count;
        FieldInfo[] fields = eventData.Fields;
        for(int i = 0; i < count; ++i) array[index++] = fields[i].Name;
        return false;
    }

    [JAPatch(typeof(Dictionary<string, object>.KeyCollection.Enumerator), "MoveNext", PatchType.Prefix, false)]
    public static bool KeyMoveNext(Dictionary<string, object> ____dictionary, ref int ____index, ref string ____currentKey, ref bool __result) {
        if(____dictionary is not EventData eventData) return true;
        __result = ____index >= eventData.Count;
        if(__result) ____currentKey = eventData.Fields[____index++].Name;
        return false;
    }

    [JAPatch(typeof(Dictionary<string, object>.ValueCollection), "CopyTo", PatchType.Prefix, false)]
    public static bool ValueCopyTo(Dictionary<string, object> ____dictionary, object[] array, int index) {
        if(____dictionary is not EventData eventData || array == null || index < 0 || index > array.Length || array.Length - index < ____dictionary.Count) return true;
        int count = eventData.Count;
        FieldInfo[] fields = eventData.Fields;
        for(int i = 0; i < count; ++i) array[index++] = fields[i].GetValue(eventData);
        return false;
    }

    [JAPatch(typeof(Dictionary<string, object>.ValueCollection.Enumerator), "MoveNext", PatchType.Prefix, false)]
    public static bool ValueMoveNext(Dictionary<string, object> ____dictionary, ref int ____index, ref object ____currentValue, ref bool __result) {
        if(____dictionary is not EventData eventData) return true;
        __result = ____index >= eventData.Count;
        if(__result) ____currentValue = eventData.Fields[____index++].GetValue(eventData);
        return false;
    }
}