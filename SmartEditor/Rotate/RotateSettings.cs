using ADOFAI.Editor;
using JALib.Core;
using JALib.Core.Setting;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SmartEditor.Rotate;

public class RotateSettings : JASetting {
    public KeyModifier addedKey = KeyModifier.Control | KeyModifier.Alt;
    public KeyCode minusKey = KeyCode.Comma;
    public KeyCode plusKey = KeyCode.Period;
    public float rotateAngle = 30;

    public RotateSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) { }
}