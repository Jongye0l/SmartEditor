using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SmartEditor.AsyncLoad;

public class LoadScreen : MonoBehaviour {
    public static LoadScreen instance;
    public Text mainText;
    public Text[] subText;
    public readonly List<LoadSequence> Sequence = [];
    public bool needApply;
    public static event Action OnRemove;

    private void Awake() {
        instance = this;
        CreateCanvas();
        CreateBackground();
        CreateTitle();
        subText = [
            CreateSubTitle(1),
            CreateSubTitle(2),
            CreateSubTitle(3),
            CreateSubTitle(4),
        ];
    }

    private void CreateCanvas() {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.sortingOrder = 1;
        gameObject.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        gameObject.AddComponent<GraphicRaycaster>();
    }

    private void CreateBackground() {
        GameObject backgroundPanel = new("BackgroundPanel");
        RectTransform panelTransform = backgroundPanel.AddComponent<RectTransform>();
        backgroundPanel.transform.SetParent(transform, false);
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.sizeDelta = Vector2.zero;
        panelTransform.anchoredPosition = Vector2.zero;
        Image panelImage = backgroundPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
    }

    private void CreateTitle() {
        GameObject textObject = new("Load Title");
        RectTransform textTransform = textObject.AddComponent<RectTransform>();
        textObject.transform.SetParent(transform, false);
        textTransform.anchoredPosition = new Vector2(0, 340);
        textTransform.sizeDelta = new Vector2(900, 600);
        mainText = textObject.AddComponent<Text>();
        mainText.font = RDString.GetFontDataForLanguage(RDString.language).font;
        mainText.fontSize = 120;
        mainText.alignment = TextAnchor.MiddleCenter;
        mainText.text = Main.Instance.Localization["AsyncMapLoad.LoadMap"];
    }

    private Text CreateSubTitle(int i) {
        GameObject textObject = new("Load SubTitle" + i);
        RectTransform textTransform = textObject.AddComponent<RectTransform>();
        textObject.transform.SetParent(transform, false);
        textTransform.anchoredPosition = new Vector2(0, 320 - 60 * i);
        textTransform.sizeDelta = new Vector2(900, 600);
        Text text = textObject.AddComponent<Text>();
        text.font = RDString.GetFontDataForLanguage(RDString.language).font;
        text.fontSize = 50;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    public static void Show() {
        if(instance) return;
        GameObject obj = new("LoadScreen Object");
        obj.AddComponent<LoadScreen>();
    }

    public static void Hide() {
        if(instance) DestroyImmediate(instance.gameObject);
        instance = null;
    }

    public static void AddSequence(LoadSequence sequence) {
        if(!instance) return;
        instance.Sequence.Add(sequence);
    }

    public static void UpdateSequence() {
        if(!instance) return;
        instance.needApply = true;
    }

    public static void RemoveSequence(LoadSequence sequence) {
        if(!instance) return;
        instance.Sequence.Remove(sequence);
        instance.needApply = true;
        OnRemove?.Invoke();
    }

    private void Update() {
        if(needApply) return;
        int i = 0;
        foreach(LoadSequence loadSequence in Sequence) {
            if(loadSequence.SequenceText == null) continue;
            subText[i++].text = loadSequence.SequenceText;
            if(i > 4) break;
        }
        needApply = false;
    }
}