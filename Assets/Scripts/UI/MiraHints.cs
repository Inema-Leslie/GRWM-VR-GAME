using TMPro;
using UnityEngine;
using static UIKit;

// Mirror, the talking smart mirror: a little lilac card that floats over the station you need next,
// greets you with the day's theme, gives the hint for each step and reacts to your score.
public class MiraHints : MonoBehaviour
{
    public RoutineManager routine;
    public float heightAbove = 0.45f;
    public float greetSeconds = 7f;

    const float W = 340f;

    Transform bubble;
    RectTransform card, fill, tagBox;
    TMP_Text tagText, title, body;
    Camera cam;
    float greetUntil;
    string shown;
    [System.NonSerialized] string greetTitle, greetBody, endTitle, endBody;
    [System.NonSerialized] Vector3 endSpot, restSpot;

    void Start()
    {
        cam = Camera.main;
        if (routine == null || cam == null) { enabled = false; return; }
        Build();
        bubble.gameObject.SetActive(false);
        routine.DayStarted += OnBegin;
        routine.StepStarted += _ => AudioFX.Play(AudioFX.Pop, bubble.position, 0.4f);
        routine.DayFinished += OnDone;
        if (routine.Started) OnBegin();
    }

    void OnBegin()
    {
        var t = routine.theme;
        greetTitle = "Good morning!";
        greetBody = t ? $"Today is {t.themeName}. {t.description}" : "Let's get ready.";
        greetUntil = Time.time + greetSeconds;
        restSpot = SpotAhead(1.2f, 0.25f);        // picked once: it stays put instead of chasing your head
        bubble.position = restSpot;
        bubble.gameObject.SetActive(true);
    }

    void OnDone(int hearts)
    {
        endTitle = hearts >= 5 ? "Five hearts!" : hearts >= 3 ? "Cute! You're ready." : "You made it!";
        endBody = "Your big reveal is right here.";
        endSpot = SpotAhead(1.1f, 0.62f);
    }

    Vector3 SpotAhead(float dist, float up)
    {
        var f = cam.transform.forward; f.y = 0f;
        if (f.sqrMagnitude < 0.01f) f = Vector3.forward;
        f.Normalize();
        return cam.transform.position + f * ClearDistance(cam.transform.position, f, dist) + Vector3.up * up;
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
        if (cam == null || !bubble.gameObject.activeSelf) return;
        Vector3 target;
        if (!string.IsNullOrEmpty(endTitle))
        {
            Say("MIRROR", endTitle, endBody);
            target = endSpot;
        }
        else
        {
            var step = routine.Current;
            bool greeting = Time.time < greetUntil;
            int n = routine.steps.Count, i = routine.CurrentIndex + 1;
            if (greeting) Say("MIRROR", greetTitle, greetBody);
            else if (step) Say($"MIRROR · STEP {i} OF {n}", step.StepName, step.Hint);
            target = step && step.station && !greeting
                ? step.station.position + Vector3.up * heightAbove
                : restSpot;
        }
        // moves only when the step changes, and never bobs (floating motion reads as drifting in VR)
        bubble.position = Vector3.Lerp(bubble.position, target, 1f - Mathf.Exp(-Time.deltaTime * 4f));
        var look = bubble.position - cam.transform.position; look.y = 0f;   // upright, turns only around its own axis
        if (look.sqrMagnitude > 0.0001f) bubble.rotation = Quaternion.LookRotation(look);
    }

    // update the card only when the words change, and size it to fit them
    void Say(string tag, string head, string text)
    {
        string key = tag + "|" + head + "|" + text;
        if (key == shown) return;
        shown = key;
        tagText.text = tag;
        title.text = head;
        body.text = text;
        float bodyH = string.IsNullOrEmpty(text) ? 0f : body.GetPreferredValues(text, W - 40f, 0f).y;
        float h = 30f + 26f + 6f + bodyH + 18f;
        body.rectTransform.sizeDelta = new Vector2(W - 40f, bodyH + 4f);
        card.sizeDelta = new Vector2(W, h);
        fill.sizeDelta = new Vector2(W - 2f, h - 2f);
        float tagW = Mathf.Max(96f, tagText.GetPreferredValues(tag).x + 28f);
        tagBox.sizeDelta = new Vector2(tagW, 26f);
        tagBox.anchoredPosition = new Vector2((W - tagW) * 0.5f, 13f);
        ((RectTransform)tagBox.GetChild(0)).sizeDelta = new Vector2(tagW - 2f, 24f);
        tagText.rectTransform.sizeDelta = new Vector2(tagW, 16f);
    }

    void Build()
    {
        // a plain parent moves around; the canvas just sits under it
        var rt = WorldPanel("Mirror", new Vector2(W, 160f), 0.0011f, out bubble, false);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.GetComponent<Canvas>().sortingOrder = 1;

        var cardImg = Box(rt, "Card", 0f, 0f, W, 120f, Lilac, 22f, LilacEdge, 1f);
        fill = (RectTransform)cardImg.transform;
        card = (RectTransform)fill.parent;

        // little name tag sitting on the top edge, like "LOOK 01 · CAFÉ DATE" in the reveal
        var tagFill = Box(card, "Tag", 0f, -13f, 120f, 26f, Card, 13f, LilacEdge, 1f);
        tagBox = (RectTransform)tagFill.transform.parent;
        tagText = Label(tagBox, "MIRROR", Face.Caps, 10f, Plum, 0f, 6f, 120f, 16f, TextAlignmentOptions.Top);
        Icon(card, "sparkle", W - 34f, 18f, 16f, SoftLilac);

        title = Label(card, "", Face.Medium, 19f, Plum, 20f, 28f, W - 56f, 26f);
        body = Label(card, "", Face.Body, 14f, Plum, 20f, 60f, W - 40f, 40f);
    }
}
