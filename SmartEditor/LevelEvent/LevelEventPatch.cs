using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using ADOFAI.LevelEditor.Controls;
using HarmonyLib;
using JALib.Core.Patch;
using JALib.Tools;
using UnityEngine;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace SmartEditor.LevelEvent;

public class LevelEventPatch {
    [JAPatch(typeof(ADOFAI.LevelEvent), ".ctor", PatchType.Transpiler, true, ArgumentTypesType = [typeof(int), typeof(LevelEventType), typeof(LevelEventInfo), typeof(Dictionary<string, object>), typeof(Dictionary<string, bool>), typeof(bool), typeof(bool), typeof(bool)])]
    public static IEnumerable<CodeInstruction> EventCtorPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> instructionList = instructions.ToList();
        Label label = generator.DefineLabel();
        for(int i = 0; i < instructionList.Count; i++) {
            if(instructionList[i].opcode != OpCodes.Ldarg_S || (byte) instructionList[i].operand != 4) continue;
            instructionList.InsertRange(i, [
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Newobj, typeof(EventDisableList).Constructor([])),
                new CodeInstruction(OpCodes.Stfld, SimpleReflect.Field(typeof(ADOFAI.LevelEvent), "disabled")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Isinst, typeof(CustomEvent)),
                new CodeInstruction(OpCodes.Brtrue, label)
            ]);
            i += 6;
            while(++i < instructionList.Count) {
                if(instructionList[i].operand is not FieldInfo { Name: "disabled" }) continue;
                instructionList.RemoveRange(i - 2, 3);
                break;
            }
            while(++i < instructionList.Count) {
                if(instructionList[i].operand is not FieldInfo { Name: "disabled" } || instructionList[i].opcode != OpCodes.Stfld) continue;
                instructionList[i - 1].operand = typeof(EventDisableList).Constructor(typeof(IDictionary<string, bool>));
                break;
            }
            break;
        }
        instructionList[^1].WithLabels(label);
        return instructionList;
    }

    [JAPatch(typeof(ADOFAI.LevelEvent), "Get", PatchType.Prefix, true, GenericType = [typeof(object)])]
    [JAPatch(typeof(ADOFAI.LevelEvent), "[].get", PatchType.Prefix, true)]
    public static bool Get(string key, ADOFAI.LevelEvent __instance, ref object __result) {
        if(__instance is not CustomEvent customEvent) return true;
        __result = SimpleReflect.Field(customEvent.GetType(), key)?.GetValue(customEvent);
        return false;
    }

    [JAPatch(typeof(ADOFAI.LevelEvent), "[].set", PatchType.Prefix, true)]
    public static bool Set(string key, object value, ADOFAI.LevelEvent __instance) {
        if(__instance is not CustomEvent customEvent) return true;
        SimpleReflect.Field(customEvent.GetType(), key)?.SetValue(customEvent, value);
        return false;
    }

    [JAPatch(typeof(ADOFAI.LevelEvent), "Encode", PatchType.Transpiler, true)]
    public static IEnumerable<CodeInstruction> EncodePatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction > instructionList = instructions.ToList();
        LocalBuilder local = null;
        for(int i = 0; i < instructionList.Count; i++) {
            CodeInstruction code = instructionList[i];
            if(code.opcode != OpCodes.Ldarg_0) continue;
            code = instructionList[++i];
            if(code.opcode != OpCodes.Ldfld || code.operand is not FieldInfo { Name: "data" }) continue;
            if(local == null) {
                if(instructionList[i + 3].opcode != OpCodes.Pop) continue;
                local = generator.DeclareLocal(typeof(object));
                instructionList.RemoveAt(i++);
                instructionList[i++] = new CodeInstruction(OpCodes.Call, typeof(ADOFAI.LevelEvent).Method("Get").MakeGenericMethod(typeof(object)));
                instructionList[i] = new CodeInstruction(OpCodes.Stloc, local);
            } else {
                code = instructionList[i + 2];
                if(code.opcode == OpCodes.Call || code.opcode == OpCodes.Callvirt) {
                    instructionList[i - 1] = new CodeInstruction(OpCodes.Ldloc, local);
                    instructionList.RemoveRange(i, 3);
                } else instructionList[i] = new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(GenerateEventType.eventTypes[LevelEventType.SetFilterAdvanced], "filterProperties"));
            }
        }
        return instructionList;
    }

    [JAPatch(typeof(ADOFAI.LevelEvent), "Copy", PatchType.Replace, true)]
    public static ADOFAI.LevelEvent Copy(ADOFAI.LevelEvent __instance) {
        CustomEvent @event = GenerateEventType.eventTypes[__instance.eventType].New<CustomEvent>(__instance.floor);
        ((EventDisableList) @event.disabled).SetValues(__instance.disabled);
        if(__instance is CustomEvent) {
            foreach(FieldInfo field in __instance.GetType().Fields()) {
                if(field.DeclaringType == typeof(ADOFAI.LevelEvent) || field.IsStatic) continue;
                object value = field.GetValue(__instance);
                if(value is Dictionary<string, object> objects) value = new Dictionary<string, object>(objects);
                field.SetValue(@event, value);
            }
        } else {
            Dictionary<string, object> filterProperties = __instance.eventType == LevelEventType.SetFilterAdvanced ? new Dictionary<string, object>() : null;
            foreach(KeyValuePair<string,object> pair in __instance.data) {
                if(pair.Key.StartsWith("filter_")) filterProperties[pair.Key] = pair.Value;
                else @event[pair.Key] = pair.Value;
            }
            if(filterProperties != null) @event.SetValue("filterProperties", filterProperties);
        }
        return @event;
    }

    [JAPatch(typeof(ADOFAI.LevelEvent), "ApplyPropertiesToRealEvents", PatchType.Replace, true)]
    public static void ApplyPropertiesToRealEvents(ADOFAI.LevelEvent __instance) {
        IEnumerator<PropertyInfo> enumerator = __instance.info.propertiesInfo.Values.GetEnumerator();
        while(enumerator.MoveNext()) {
            PropertyInfo propertyInfo = enumerator.Current;
            if(__instance.disabled[propertyInfo.name]) continue;
            foreach(ADOFAI.LevelEvent realEvent in __instance.realEvents) {
                if(propertyInfo.name == "floor") realEvent.floor = __instance.floor;
                else if(propertyInfo.type == PropertyType.Vector2) {
                    Vector2 v1 = (Vector2) realEvent[propertyInfo.name];
                    Vector2 v2 = (Vector2) __instance[propertyInfo.name];
                    if(!float.IsNaN(v2.x)) v1.x = v2.x;
                    if(!float.IsNaN(v2.y)) v1.y = v2.y;
                    realEvent[propertyInfo.name] = v1;
                } else realEvent[propertyInfo.name] = __instance[propertyInfo.name];
            }
        }
        if(!__instance.IsDecoration || !ADOBase.isEditingLevel) return;
        foreach(ADOFAI.LevelEvent realEvent in __instance.realEvents) ADOBase.editor.UpdateDecorationObject(realEvent);
    }

    [JAPatch(typeof(scnEditor), "CreateDecoration", PatchType.Replace, false)]
    public static ADOFAI.LevelEvent CreateDecoration(scnEditor __instance, LevelEventType eventType) {
        DecorationEvent decoration = GenerateEventType.eventTypes[eventType].New<DecorationEvent>(-1);
        if(__instance.selectedFloors.Count == 1) {
            decoration.relativeTo = DecPlacementType.Tile;
            decoration.floor = __instance.selectedFloors[0].seqID;
        } else decoration.position = (Vector2) Camera.main.transform.position / __instance.customLevel.GetTileSize();
        Main.Instance.Log("Decoration created.");
        return decoration;
    }

    [JAPatch(typeof(scrDecoration), "Setup", PatchType.Transpiler, false, Debug = true)]
    public static IEnumerable<CodeInstruction> DecorationSetupPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
        List<CodeInstruction> instructionList = instructions.ToList();
        for(int i = 0; i < instructionList.Count; i++) {
            CodeInstruction code = instructionList[i];
            if(code.operand is FieldInfo { Name: "eventType" }) {
                code = instructionList[++i];
                if(code.opcode != OpCodes.Stloc && code.opcode != OpCodes.Stloc_S) continue;
                LocalBuilder local = (LocalBuilder) code.operand;
                Dictionary<Label, LevelEventType> labelType = new();
                while(++i < instructionList.Count) {
                    code = instructionList[i];
                    if(code.operand == local) {
                        LevelEventType eventType = (LevelEventType) (sbyte) instructionList[++i].operand;
                        labelType[(Label) instructionList[++i].operand] = eventType;
                    } else if(code.opcode == OpCodes.Br) {
                        labelType[(Label) code.operand] = LevelEventType.None;
                        break;
                    }
                }
                LevelEventType curType = LevelEventType.None;
                FieldInfo dataField = SimpleReflect.Field(typeof(ADOFAI.LevelEvent), "data");
                while(++i < instructionList.Count) {
                    code = instructionList[i];
                    if(code.labels.Count != 0 && labelType.TryGetValue(code.labels[0], out LevelEventType type)) {
                        curType = type;
                        if(curType == LevelEventType.None) break;
                    }
                    if(code.operand is FieldInfo field && field == dataField) {
                        string key = (string) instructionList[i + 1].operand;
                        field = SimpleReflect.Field(GenerateEventType.eventTypes[curType], key);
                        instructionList[i++] = new CodeInstruction(OpCodes.Ldfld, field);
                        if(instructionList[i + 1].operand is MethodInfo { Name: "get_Item" }) instructionList.RemoveRange(i, 2);
                        else {
                            for(int i2 = i; i2 < instructionList.Count; i2++) {
                                if(instructionList[i2].operand is not MethodInfo { Name: "GetValueOrDefault" }) continue;
                                instructionList.RemoveRange(i, i2 - i + 1);
                                break;
                            }
                        }
                        code = instructionList[i];
                        if(code.operand is MethodInfo methodInfo && (methodInfo.DeclaringType == typeof(Convert) || methodInfo.Name == "ToString")) instructionList.RemoveAt(i);
                        else if(code.opcode == OpCodes.Castclass || code.opcode == OpCodes.Unbox || code.opcode == OpCodes.Unbox_Any) instructionList.RemoveAt(i);
                        else if(field.FieldType.IsValueType && code.opcode != OpCodes.Box) instructionList.Insert(i, new CodeInstruction(OpCodes.Box, field.FieldType));
                    } else if(code.operand is MethodInfo { Name: "GetBool" }) {
                        string key = (string) instructionList[--i].operand;
                        instructionList[i++] = new CodeInstruction(OpCodes.Ldfld, SimpleReflect.Field(GenerateEventType.eventTypes[curType], key));
                        instructionList.RemoveAt(i);
                    }
                }
                while(++i < instructionList.Count) {
                    code = instructionList[i];
                    if(code.operand is FieldInfo field && field == dataField) {
                        string key = (string) instructionList[i + 1].operand;
                        field = SimpleReflect.Field(typeof(DecorationEvent), key);
                        if(field == null) continue;
                        instructionList[i++] = new CodeInstruction(OpCodes.Ldfld, field);
                        if(instructionList[i + 1].operand is MethodInfo { Name: "get_Item" }) instructionList.RemoveRange(i, 2);
                        else {
                            for(int i2 = i; i2 < instructionList.Count; i2++) {
                                if(instructionList[i2].operand is not MethodInfo { Name: "GetValueOrDefault" }) continue;
                                instructionList.RemoveRange(i, i2 - i + 1);
                                break;
                            }
                        }
                        code = instructionList[i];
                        if(code.operand is MethodInfo methodInfo && (methodInfo.DeclaringType == typeof(Convert) || methodInfo.Name == "ToString")) instructionList.RemoveAt(i);
                        else if(code.opcode == OpCodes.Castclass || code.opcode == OpCodes.Unbox || code.opcode == OpCodes.Unbox_Any) instructionList.RemoveAt(i);
                        else if(field.FieldType.IsValueType && code.opcode != OpCodes.Box) instructionList.Insert(i, new CodeInstruction(OpCodes.Box, field.FieldType));
                    } else if(code.operand is MethodInfo { Name: "GetBool" }) {
                        string key = (string) instructionList[i - 1].operand;
                        field = SimpleReflect.Field(typeof(DecorationEvent), key);
                        if(field == null) continue;
                        instructionList[i - 1] = new CodeInstruction(OpCodes.Ldfld, field);
                        instructionList.RemoveAt(i);
                    }
                }
            }
        }
        return instructionList;
    }
}