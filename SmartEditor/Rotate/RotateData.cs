using System;
using System.Threading.Tasks;
using ADOFAI.Editor;
using DG.Tweening;
using JALib.Tools;
using UnityEngine;

namespace SmartEditor.Rotate;

public class RotateData : MonoBehaviour {
    public float angle;
    public Transform shortcutsContainer;
    public Action<bool> onChange;

    private void Awake() {
        shortcutsContainer = scnEditor.instance.floorButtonCanvas.transform.Find("ShortcutsContainer");
        onChange = (Action<bool>) Delegate.CreateDelegate(typeof(Action<bool>), scnEditor.instance, typeof(scnEditor).Method("UpdateFloorDirectionButtons"));
    }

    private void Update() {
        try {
            if(!scrController.instance.paused) return;
            RotateSettings settings = RotateScreen.settings;
            if(settings.addedKey.HasFlag(KeyModifier.Control) && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) return;
            if(settings.addedKey.HasFlag(KeyModifier.Alt) && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) return;
            if(settings.addedKey.HasFlag(KeyModifier.Shift) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) return;
            if(settings.addedKey.HasFlag(KeyModifier.BackQuote) && !Input.GetKey(KeyCode.BackQuote)) return;
            float originalAngle = angle;
            if(Input.GetKeyDown(settings.minusKey)) angle -= settings.rotateAngle;
            if(Input.GetKeyDown(settings.plusKey)) angle += settings.rotateAngle;
            if(originalAngle == angle) return;
            if(angle < 0) angle += 360;
            if(angle >= 360) angle -= 360;
            Vector3 vector3 = new(0, 0, -angle);
            scrCamera.instance.transform.DORotate(vector3, 0.1f).SetEase(Ease.OutQuad).SetUpdate(true);
            Transform transform = shortcutsContainer.parent;
            for(int i = 0; i < transform.childCount; i++) transform.GetChild(i).DORotate(vector3, 0.1f).SetEase(Ease.OutQuad).SetUpdate(true);
            Task.Delay(100).GetAwaiter().UnsafeOnCompleted(SetupAngle);
        } catch (Exception e) {
            Main.Instance.LogReportException("RotateScreen RotateData Updated Failed", e);
        }
    }

    private void SetupAngle() {
        if(scnEditor.instance.SelectionIsSingle()) onChange(true);
    }
}