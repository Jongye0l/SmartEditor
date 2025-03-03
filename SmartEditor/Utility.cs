using System;

namespace SmartEditor;

public static class Utility {
    public static double GetAngle(scrFloor prevFloor) {
        scrFloor curFloor = prevFloor.nextfloor;
        double prevAngle = prevFloor.floatDirection;
        if(prevAngle == 999) prevAngle = prevFloor.prevfloor.floatDirection + 180;
        double curAngle = curFloor.floatDirection;
        if(curAngle == 999) {
            if(!curFloor.nextfloor) return 0;
            curAngle = curFloor.nextfloor.floatDirection + 180;
        }
        return NormalizeAngle((180 + prevAngle - curAngle) * (curFloor.isCCW ? -1 : 1));
    }

    public static double NormalizeAngle(double angle) {
        angle = Math.Round(angle, 4);
        if(angle == 999) return angle;
        while(angle < 0) angle += 360;
        angle %= 360;
        return angle == 0 ? 360 : angle;
    }
}