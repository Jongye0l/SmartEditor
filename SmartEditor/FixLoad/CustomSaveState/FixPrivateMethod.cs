using System.Reflection;
using JALib.Tools;

namespace SmartEditor.FixLoad.CustomSaveState;

public class FixPrivateMethod {
    public static MethodInfo DeleteFloorMethod = typeof(scnEditor).Method("DeleteFloor");
    public static MethodInfo MoveCameraToFloorMethod = typeof(scnEditor).Method("MoveCameraToFloor");
    public static MethodInfo OffsetFloorIDsInEventsMethod = typeof(scnEditor).Method("OffsetFloorIDsInEvents");

    public static void DeleteFloor(int index) {
        DeleteFloorMethod.Invoke(scnEditor.instance, [index, true]);
    }

    public static void MoveCameraToFloor(scrFloor floor) {
        MoveCameraToFloorMethod.Invoke(scnEditor.instance, [floor]);
    }

    public static void OffsetFloorIDsInEvents(int index, int offset) {
        OffsetFloorIDsInEventsMethod.Invoke(scnEditor.instance, [index, offset]);
    }
}