using UnityEngine;
using UnityEngine.UI;

// Mirror, the talking smart mirror: a speech bubble that floats over the station you need next,
// greets you with the day's theme, gives the hint for each step and reacts to your score.
public class MiraHints : MonoBehaviour
{
    public RoutineManager routine;
    public float heightAbove = 0.45f;
    public float greetSeconds = 7f;

    Transform bubble;
    Text body;
    Camera cam;
    float greetUntil;
    [System.NonSerialized] string greeting, ending;
    [System.NonSerialized] Vector3 endSpot;

    void Start()
    {
        cam = Camera.main;
        if (routine == null || cam == null) { enabled = false; return; }
        Build();
        var t = routine.theme;
        greeting = t ? $"Good morning! Today is {t.themeName}. {t.description}" : "Good morning! Let's get ready.";
        greetUntil = Time.time + greetSeconds;
        routine.StepStarted += _ => AudioFX.Play(AudioFX.Pop, bubble.position, 0.4f);
        routine.DayFinished += OnDone;
        bubble.position = cam.transform.position + cam.transform.forward;
    }

    void OnDone(int hearts)
    {
        ending = hearts >= 5 ? "Five hearts! You're glowing!"
               : hearts >= 3 ? "Cute! You're ready to go."
               : "A little rushed, but you made it!";
        var f = cam.transform.forward; f.y = 0f; f.Normalize();
        endSpot = cam.transform.position + f * ClearDistance(cam.transform.position, f, 1.1f) + Vector3.up * 0.45f;
    }

    // how far ahead something can float before it ends up inside a wall
    public static float ClearDistance(Vector3 from, Vector3 dir, float wanted)
    {
        int mask = ~(1 << 5);                                   // skip UI
        int avatar = LayerMask.NameToLayer("Avatar");
        if (avatar >= 0) mask &= ~(1 << avatar);
        return Physics.Raycast(from, dir, out var hit, wanted + 0.2f, mask, QueryTriggerInteraction.Ignore)
            ? Mathf.Max(0.4f, hit.distance - 0.2f) : wanted;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        Vector3 target;
        if (!string.IsNullOrEmpty(ending))
        {
            body.text = ending;
            target = endSpot;
        }
        else
        {
            var step = routine.Current;
            body.text = Time.time < greetUntil ? greeting : step ? step.Hint : "";
            target = step && step.station && Time.time >= greetUntil
                ? step.station.position + Vector3.up * heightAbove
                : cam.transform.position + cam.transform.forward * 1.2f + Vector3.up * 0.25f;
        }
        target += Vector3.up * Mathf.Sin(Time.time * 2f) * 0.02f;   // gentle bob
        bubble.position = Vector3.Lerp(bubble.position, target, 1f - Mathf.Exp(-Time.deltaTime * 4f));
        bubble.rotation = Quaternion.LookRotation(bubble.position - cam.transform.position);
    }

    void Build()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        // a plain parent moves around; the canvas just sits under it
        bubble = new GameObject("Mirror").transform;
        bubble.gameObject.layer = 5;                    // UI: mirrors don't reflect it
        var go = new GameObject("Canvas", typeof(RectTransform));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(bubble, false);
        go.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        rt.sizeDelta = new Vector2(380f, 130f);
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * 0.0011f;

        var bg = new GameObject("Bubble", typeof(Image));
        bg.layer = 5;
        var brt = (RectTransform)bg.transform;
        brt.SetParent(rt, false);
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = brt.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(1f, 0.93f, 0.97f, 0.92f);

        Label(rt, font, "Name", "Mirror", 24, FontStyle.Bold, new Color(0.85f, 0.3f, 0.6f), new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.95f));
        body = Label(rt, font, "Line", "", 22, FontStyle.Normal, new Color(0.35f, 0.15f, 0.3f), new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.7f));
    }

    static Text Label(RectTransform parent, Font font, string name, string text, int size, FontStyle style, Color c, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(Text));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.GetComponent<Text>();
        t.font = font; t.fontSize = size; t.fontStyle = style; t.color = c; t.text = text;
        t.alignment = TextAnchor.UpperLeft;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }
}
