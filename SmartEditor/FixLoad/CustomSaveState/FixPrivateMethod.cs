using System.Reflection;
using ADOFAI;
using JALib.Tools;

namespace SmartEditor.FixLoad.CustomSaveState;

public class FixPrivateMethod {
    public static MethodInfo DeleteFloorMethod = typeof(scnEditor).Method("DeleteFloor");
    public static MethodInfo MoveCameraToFloorMethod = typeof(scnEditor).Method("MoveCameraToFloor");
    public static MethodInfo OffsetFloorIDsInEventsMethod = typeof(scnEditor).Method("OffsetFloorIDsInEvents");
    public static FieldInfo copiedHitsoundField = typeof(scnEditor).Field("copiedHitsound");
    public static FieldInfo previousHitsoundField = typeof(scnEditor).Field("previousHitsound");

    public static LevelEvent copiedHitsound => (LevelEvent) copiedHitsoundField.GetValue(scnEditor.instance);
    public static LevelEvent previousHitsound => (LevelEvent) previousHitsoundField.GetValue(scnEditor.instance);

    public static void DeleteFloor(int index, bool remakePath = true) {
        DeleteFloorMethod.Invoke(scnEditor.instance, [index, remakePath]);
    }

    public static void MoveCameraToFloor(scrFloor floor) {
        MoveCameraToFloorMethod.Invoke(scnEditor.instance, [floor]);
    }

    public static void OffsetFloorIDsInEvents(int index, int offset) {
        OffsetFloorIDsInEventsMethod.Invoke(scnEditor.instance, [index, offset]);
    }
}