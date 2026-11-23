using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UIKit;

// Small card under your view: current step, hint, Glow Meter and the morning-rush timer.
public class GlowHud : MonoBehaviour
{
    public RoutineManager routine;
    public Vector3 offset = new Vector3(0f, -0.13f, 0.45f);

    const float W = 440f, H = 118f;

    TMP_Text title, hint, timerText;
    Transform cam, vrHud;   // VR: the card sits in the room and only catches up when it leaves your view
    bool catchingUp;
    float leaveUntil;   // first tap on the house arms it, a second tap within 3 s goes back
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
        hint.text = Time.time < leaveUntil ? "Tap the house again to go back to the main menu. Your progress resets."
                  : step ? step.Hint : "";
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

    Vector3 HudSpot()
    {
        var f = cam.forward; f.y = 0f;
        if (f.sqrMagnitude < 0.01f) f = Vector3.forward;
        f.Normalize();
        return cam.position + f * offset.z + Vector3.up * (offset.y - 0.05f);
    }

    Quaternion HudFacing(Vector3 at) => Quaternion.LookRotation(at - cam.position);

    void LateUpdate()
    {
        if (vrHud == null || cam == null) return;
        var to = vrHud.position - cam.position; to.y = 0f;
        var f = cam.forward; f.y = 0f;
        var target = HudSpot();
        if (!catchingUp && (Vector3.Angle(to, f) > 35f || Mathf.Abs(vrHud.position.y - target.y) > 0.25f || (vrHud.position - target).magnitude > 0.5f))
            catchingUp = true;
        if (!catchingUp) return;
        vrHud.position = Vector3.Lerp(vrHud.position, target, 1f - Mathf.Exp(-Time.deltaTime * 12f));   // quick, then still
        vrHud.rotation = HudFacing(vrHud.position);
        if ((vrHud.position - target).magnitude < 0.01f) catchingUp = false;
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
            // VR: a little below your view. Not glued to the head (that makes people sick):
            // it waits in place and only catches up when you look well away
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            this.cam = cam;
            vrHud = new GameObject("GlowHudHolder").transform;
            rt.SetParent(vrHud, false);
            rt.sizeDelta = new Vector2(W, H);
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one * 0.0004f;
            vrHud.SetPositionAndRotation(HudSpot(), HudFacing(HudSpot()));
            panel.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 3f;
            root = Node("Hud", rt, 0f, 0f, W, H);
        }

        Box(root, "Card", 0f, 0f, W, H, new Color(Card.r, Card.g, Card.b, 0.96f), 20f, Border, 1.5f);
        title = Label(root, "", Face.Medium, 20f, Plum, 18f, 12f, 260f, 26f);
        timerBg = Box(root, "Timer", W - 150f, 14f, 80f, 28f, Lilac, 14f);
        timerText = Label(root, "", Face.Caps, 14f, Plum, W - 150f, 19f, 80f, 20f, TextAlignmentOptions.Top);
        hint = Label(root, "", Face.Body, 13f, Mauve, 18f, 44f, W - 90f, 36f);

        // back to the main menu (asks twice, so a stray tap doesn't throw away the morning)
        IconButton(root, "BackToMenu", W - 60f, 10f, "home", () =>
        {
            if (Time.time < leaveUntil) UIKit.BackToMenu();
            else leaveUntil = Time.time + 3f;
        });
        UIKit.MakeInteractive(canvas);

        Label(root, "GLOW", Face.Caps, 10f, Mauve, 18f, 90f, 50f, 14f);
        Box(root, "Bar", 66f, 90f, 236f, 12f, Divider, 6f);
        fill = (RectTransform)Box(root, "Fill", 66f, 90f, 12f, 12f, Pink, 6f).transform;
        Icon(root, "heart_fill", 312f, 85f, 20f, Pink);   // end of the glow bar
        Icon(root, "sparkle", W - 40f, 86f, 20f, SoftLilac);
    }
}
