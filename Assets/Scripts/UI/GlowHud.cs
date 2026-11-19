using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UIKit;

// Small card under your view: current step, hint, Glow Meter and the morning-rush timer.
public class GlowHud : MonoBehaviour
{
    public RoutineManager routine;
    public Vector3 offset = new Vector3(0f, -0.13f, 0.45f);

    const float W = 400f, H = 118f;

    TMP_Text title, hint, timerText;
    RectTransform fill;
    Image timerBg;
    GameObject panel;

    void Start()
    {
        var cam = Camera.main;
        if (cam == null || routine == null) { enabled = false; return; }
        Build(cam.transform);
    }

    void Update()
    {
        bool show = routine.Started && !routine.Finished;
        if (panel.activeSelf != show) panel.SetActive(show);
        if (!show) return;

        var step = routine.Current;
        title.text = step ? step.StepName : "";
        hint.text = step ? step.Hint : "";
        fill.sizeDelta = new Vector2(Mathf.Max(12f, 236f * Mathf.Clamp01(routine.Glow)), 12f);

        var t = routine.timer;
        if (t && t.Running)
        {
            int s = Mathf.CeilToInt(t.Remaining);
            timerText.text = $"{s / 60}:{s % 60:00}";
            bool rush = s <= 30;
            timerBg.color = rush ? Pink : Lilac;
            timerText.color = rush ? Color.white : Plum;
        }
        else
        {
            int s = Mathf.RoundToInt(routine.theme ? routine.theme.timeLimit : 300f);   // the clock starts once you're up
            timerText.text = $"{s / 60}:{s % 60:00}";
        }
    }

    void Build(Transform cam)
    {
        panel = new GameObject("GlowHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        panel.layer = 5;   // UI: mirrors don't reflect it
        var canvas = panel.GetComponent<Canvas>();
        var rt = (RectTransform)panel.transform;
        var desktop = FindAnyObjectByType<DesktopPlayer>();
        RectTransform root;
        if (desktop && desktop.desktopMode)
        {
            // flat on the screen, so walls and counters never hide it
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = panel.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            root = Node("Hud", rt, 20f, 20f, W, H);                   // top-left, out of the way
        }
        else
        {
            // VR: floats a little below your view
            canvas.renderMode = RenderMode.WorldSpace;
            rt.SetParent(cam, false);
            rt.sizeDelta = new Vector2(W, H);
            rt.localPosition = offset;
            rt.localRotation = Quaternion.Euler(12f, 0f, 0f);
            rt.localScale = Vector3.one * 0.0004f;
            panel.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 3f;
            root = Node("Hud", rt, 0f, 0f, W, H);
        }

        Box(root, "Card", 0f, 0f, W, H, new Color(Card.r, Card.g, Card.b, 0.96f), 20f, Border, 1.5f);
        title = Label(root, "", Face.Medium, 20f, Plum, 18f, 12f, 280f, 26f);
        timerBg = Box(root, "Timer", W - 96f, 12f, 80f, 28f, Lilac, 14f);
        timerText = Label(root, "", Face.Caps, 14f, Plum, W - 96f, 17f, 80f, 20f, TextAlignmentOptions.Top);
        hint = Label(root, "", Face.Body, 13f, Mauve, 18f, 44f, W - 36f, 36f);

        Label(root, "GLOW", Face.Caps, 10f, Mauve, 18f, 90f, 50f, 14f);
        Box(root, "Bar", 66f, 90f, 236f, 12f, Divider, 6f);
        fill = (RectTransform)Box(root, "Fill", 66f, 90f, 12f, 12f, Pink, 6f).transform;
        Icon(root, "heart_fill", 312f, 85f, 20f, Pink);
        Icon(root, "sparkle", W - 40f, 86f, 20f, SoftLilac);
    }
}
