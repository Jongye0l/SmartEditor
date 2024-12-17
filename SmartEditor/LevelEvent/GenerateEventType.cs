using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ADOFAI;
using ADOFAI.Editor.Models;
using JALib.Tools;
using UnityEngine;
using PropertyInfo = ADOFAI.PropertyInfo;

namespace SmartEditor.LevelEvent;

public class GenerateEventType {
    public static Dictionary<LevelEventType, Type> eventTypes;

    public static void Generate() {
        if(eventTypes != null) return;
        if(GCS.levelEventsInfo == null) {
            MainThread.WaitForMainThread().GetAwaiter().OnCompleted(Generate);
            return;
        }
        eventTypes = new Dictionary<LevelEventType, Type>();
        ModuleBuilder builder = Main.ModuleBuilder;
        foreach(LevelEventInfo info in GCS.levelEventsInfo.Values) MakeType(builder, info, false);
        foreach(LevelEventInfo info in GCS.settingsInfo.Values) MakeType(builder, info, true);
    }

    private static void MakeType(ModuleBuilder builder, LevelEventInfo info, bool settings) {
        TypeBuilder typeBuilder = builder.DefineType(info.name, TypeAttributes.Public, settings ? typeof(SettingEvent) :
            info.isDecoration ? typeof(DecorationEvent) : info.propertiesInfo.ContainsKey("angleOffset") ? typeof(AngleEvent) : typeof(CustomEvent));
        FieldBuilder infoField = typeBuilder.DefineField("_info", typeof(LevelEventInfo), FieldAttributes.Private | FieldAttributes.Static);
        ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, [typeof(int)]);
        ILGenerator il = constructorBuilder.GetILGenerator();
        foreach(PropertyInfo propertyInfo in info.propertiesInfo.Values) {
            if(propertyInfo.type is PropertyType.Export or PropertyType.ParticlePlayback or PropertyType.Note || typeBuilder.BaseType.Field(propertyInfo.name) != null) continue;
            FieldBuilder fieldBuilder = typeBuilder.DefineField(propertyInfo.name, propertyInfo.type switch {
                PropertyType.Bool => typeof(bool),
                PropertyType.Int or PropertyType.Rating => typeof(int),
                PropertyType.Float => typeof(float),
                PropertyType.String or PropertyType.LongString or PropertyType.File or PropertyType.Color => typeof(string),
                PropertyType.Enum => propertyInfo.enumType,
                PropertyType.Vector2 => typeof(Vector2),
                PropertyType.Tile => typeof(Tuple<int, TileRelativeTo>),
                PropertyType.Array => typeof(object[]),
                PropertyType.FloatPair => typeof(Tuple<float, float>),
                PropertyType.Vector2Range => typeof(Tuple<Vector2, Vector2>),
                PropertyType.MinMaxGradient => typeof(SerializedMinMaxGradient),
                PropertyType.List => typeof(object),
                PropertyType.FilterProperties => typeof(Dictionary<string, object>),
                _ => throw new NotSupportedException(propertyInfo.type + " is not supported")
            }, FieldAttributes.Public);
            switch(propertyInfo.type) {
                case PropertyType.Vector2:
                    Vector2 vector2 = (Vector2) propertyInfo.value_default;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_R4, vector2.x);
                    il.Emit(OpCodes.Ldc_R4, vector2.y);
                    il.Emit(OpCodes.Newobj, typeof(Vector2).Constructor(typeof(float), typeof(float)));
                    il.Emit(OpCodes.Stfld, fieldBuilder);
                    break;
                case PropertyType.MinMaxGradient:
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, ((Delegate) SerializedMinMaxGradient.Default).Method);
                    il.Emit(OpCodes.Stfld, fieldBuilder);
                    break;
                case PropertyType.Array:
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Newarr, typeof(object));
                    il.Emit(OpCodes.Stfld, fieldBuilder);
                    break;
                case PropertyType.String or PropertyType.LongString or PropertyType.File or PropertyType.Color:
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, (string) propertyInfo.value_default);
                    il.Emit(OpCodes.Stfld, fieldBuilder);
                    break;
                default:
                    fieldBuilder.SetConstant(propertyInfo.value_default);
                    break;
            }
        }
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        EmitInt(il, (int) info.type);
        il.Emit(OpCodes.Ldsfld, infoField);
        il.Emit(OpCodes.Call, typeBuilder.BaseType.Constructor(typeof(int), typeof(LevelEventType), typeof(LevelEventInfo)));
        il.Emit(OpCodes.Ret);
        (eventTypes[info.type] = typeBuilder.CreateType()).SetValue("_info", info);
    }

    public static void EmitInt(ILGenerator il, int i) {
        switch(i) {
            case >= -1 and <= 8:
                il.Emit(i switch {
                    -1 => OpCodes.Ldc_I4_M1,
                    0 => OpCodes.Ldc_I4_0,
                    1 => OpCodes.Ldc_I4_1,
                    2 => OpCodes.Ldc_I4_2,
                    3 => OpCodes.Ldc_I4_3,
                    4 => OpCodes.Ldc_I4_4,
                    5 => OpCodes.Ldc_I4_5,
                    6 => OpCodes.Ldc_I4_6,
                    7 => OpCodes.Ldc_I4_7,
                    8 => OpCodes.Ldc_I4_8,
                });
                break;
            case >= -128 and < 128:
                il.Emit(OpCodes.Ldc_I4_S, (sbyte) i);
                break;
            default:
                il.Emit(OpCodes.Ldc_I4, i);
                break;
        }
    }
}