using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EditorTweaks.Patch.Timeline;
using HarmonyLib;
using JALib.Core.Patch;
using JALib.Tools;
using SmartEditor.FixLoad.CustomSaveState.Scope;

namespace SmartEditor.FixLoad;

public class EditorTweaksPatch {
    [JAPatch(typeof(TimelinePanel), nameof(OnBeginDrag), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> OnBeginDrag(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode != OpCodes.Newobj || code.operand is ConstructorInfo { DeclaringType.Name: "SaveStateScope" }) continue;
            list[i - 4].opcode = OpCodes.Ldarg_0;
            list[i - 3] = new CodeInstruction(OpCodes.Newobj, SimpleReflect.Constructor(typeof(EventMoveScope)));
            list.RemoveRange(i - 2, 3);
        }
        return list;
    }

    [JAPatch(typeof(TimelinePanel), nameof(OnEndDrag), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> OnEndDrag(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode != OpCodes.Newobj || code.operand is not ConstructorInfo { DeclaringType.Name: "SaveStateScope" }) continue;
            int count;
            for(int j = 1; ; j++)
                if(list[i + j].opcode == OpCodes.Endfinally) {
                    count = 5 + j;
                    break;
                }
            List<Label> labels = list[i - 4].labels;
            list.RemoveRange(i - 4, count);
            list[i - 4].labels[1] = labels[0];
        }
        return list;
    }

    [JAPatch(typeof(TimelinePatch.TimelineLevelEventPatch), nameof(MultiCutEvents), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> MultiCutEvents(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> list = instructions.ToList();
        for(int i = 0; i < list.Count; i++) {
            CodeInstruction code = list[i];
            if(code.opcode != OpCodes.Newobj || code.operand is ConstructorInfo { DeclaringType.Name: "SaveStateScope" }) continue;
            list[i - 4].opcode = OpCodes.Ldsfld;
            list[i - 4].operand = typeof(TimelinePatch).GetField("timeline");
            list[i - 3] = new CodeInstruction(OpCodes.Newobj, typeof(EventsChangeScope).Constructor(typeof(TimelinePanel)));
            list.RemoveRange(i - 2, 3);
        }
        return list;
    }

    [JAPatch(typeof(TimelinePatch.TimelineLevelEventPatch), nameof(PasteMultipleEvents), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> PasteMultipleEvents(IEnumerable<CodeInstruction> instructions) => ScopePatch.DuplicateDecorations(instructions);

    [JAPatch(typeof(TimelinePatch.TimelineLevelEventPatch), nameof(ShowMultipleEvents), PatchType.Transpiler, false)]
    public static IEnumerable<CodeInstruction> ShowMultipleEvents(IEnumerable<CodeInstruction> instructions) => ScopePatch.ShowPanel(instructions);
}