using UnityEngine;
using UnityEngine.UI;

// Small world-space panel under your view: current step, hint, Glow Meter, timer, hearts.
public class GlowHud : MonoBehaviour
{
    public RoutineManager routine;
    public Vector3 offset = new Vector3(0f, -0.13f, 0.45f);

    Text title, hint, timerText, hearts;
    RectTransform fill;
    GameObject panel;

    static readonly Color Pink = new Color(1f, 0.45f, 0.7f);

    void Start()
    {
        var cam = Camera.main;
        if (cam == null || routine == null) { enabled = false; return; }
        Build(cam.transform);
        routine.DayFinished += ShowResult;
    }

    void Update()
    {
        if (routine.Finished) return;
        var step = routine.Current;
        title.text = step ? step.StepName : "";
        hint.text = step ? step.Hint : "";
        fill.anchorMax = new Vector2(Mathf.Clamp01(routine.Glow), 1f);

        var t = routine.timer;
        if (t && t.Running)
        {
            int s = Mathf.CeilToInt(t.Remaining);
            timerText.text = $"{s / 60}:{s % 60:00}";
            timerText.color = s <= 30 ? Pink : Color.white;
        }
        else timerText.text = "";
    }

    void ShowResult(int n)
    {
        title.text = "Ready! " + (routine.theme ? routine.theme.themeName : "");
        hint.text = $"Theme match {Mathf.RoundToInt(routine.score.LastThemeMatch * 100)}%";
        hearts.text = "<color=#FF73B3>" + new string('♥', n) + "</color><color=#FFFFFF55>" + new string('♥', 5 - n) + "</color>";
        timerText.text = "";
        fill.anchorMax = new Vector2(Mathf.Clamp01(routine.Glow), 1f);
    }

    void Build(Transform cam)
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        panel = new GameObject("GlowHud", typeof(Canvas), typeof(CanvasScaler));
        panel.layer = 5;   // UI: mirrors don't reflect it
        var canvas = panel.GetComponent<Canvas>();
        var rt = (RectTransform)panel.transform;
        var desktop = FindFirstObjectByType<DesktopPlayer>();
        RectTransform root;
        if (desktop && desktop.desktopMode)
        {
            // flat on the screen, so walls and counters never hide it
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = panel.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            root = Box("Panel", rt, new Vector2(0f, 1f), new Vector2(0f, 1f), Color.clear);   // top-left, out of the way
            root.sizeDelta = new Vector2(380f, 130f);
            root.anchoredPosition = new Vector2(205f, -80f);
        }
        else
        {
            // VR: floats a little below your view
            canvas.renderMode = RenderMode.WorldSpace;
            rt.SetParent(cam, false);
            rt.sizeDelta = new Vector2(440f, 150f);
            rt.localPosition = offset;
            rt.localRotation = Quaternion.Euler(12f, 0f, 0f);
            rt.localScale = Vector3.one * 0.0004f;
            root = rt;
        }
        rt = root;

        Box("Bg", rt, Vector2.zero, Vector2.one, new Color(0.35f, 0.12f, 0.3f, 0.55f));
        title = Label("Title", rt, font, 30, FontStyle.Bold, new Vector2(0.04f, 0.68f), new Vector2(0.75f, 0.95f), TextAnchor.MiddleLeft);
        timerText = Label("Timer", rt, font, 28, FontStyle.Bold, new Vector2(0.72f, 0.68f), new Vector2(0.96f, 0.95f), TextAnchor.MiddleRight);
        hint = Label("Hint", rt, font, 20, FontStyle.Normal, new Vector2(0.04f, 0.36f), new Vector2(0.96f, 0.68f), TextAnchor.MiddleLeft);
        hearts = Label("Hearts", rt, font, 26, FontStyle.Normal, new Vector2(0.62f, 0.06f), new Vector2(0.96f, 0.3f), TextAnchor.MiddleRight);
        hearts.supportRichText = true;

        Label("GlowLabel", rt, font, 16, FontStyle.Bold, new Vector2(0.04f, 0.08f), new Vector2(0.2f, 0.3f), TextAnchor.MiddleLeft).text = "GLOW";
        var bar = Box("Bar", rt, new Vector2(0.18f, 0.12f), new Vector2(0.6f, 0.26f), new Color(1f, 1f, 1f, 0.25f));
        fill = Box("Fill", bar, Vector2.zero, new Vector2(0f, 1f), Pink);
    }

    static RectTransform Box(string name, RectTransform parent, Vector2 min, Vector2 max, Color c)
    {
        var go = new GameObject(name, typeof(Image));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = c;
        return rt;
    }

    static Text Label(string name, RectTransform parent, Font font, int size, FontStyle style, Vector2 min, Vector2 max, TextAnchor align)
    {
        var go = new GameObject(name, typeof(Text));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.GetComponent<Text>();
        t.font = font; t.fontSize = size; t.fontStyle = style; t.alignment = align;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}
