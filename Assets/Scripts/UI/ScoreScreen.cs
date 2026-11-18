using UnityEngine;
using UnityEngine.UI;

// End of the morning: your selfie as a polaroid with the hearts and how the day went.
public class ScoreScreen : MonoBehaviour
{
    public RoutineManager routine;
    public SelfieStep selfie;

    void Start()
    {
        if (routine) routine.DayFinished += Show;
    }

    void Show(int hearts)
    {
        var cam = Camera.main;
        if (cam == null) return;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // place a plain parent, then hang the canvas under it (a root world canvas snaps back to the origin)
        var f = cam.transform.forward; f.y = 0f; f.Normalize();
        var holder = new GameObject("ScoreScreen").transform;
        holder.gameObject.layer = 5;
        holder.SetPositionAndRotation(cam.transform.position + f * MiraHints.ClearDistance(cam.transform.position, f, 1.1f), Quaternion.LookRotation(f));
        var go = new GameObject("Canvas", typeof(RectTransform));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(holder, false);
        go.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        rt.sizeDelta = new Vector2(420f, 620f);
        rt.localPosition = Vector3.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * 0.0012f;

        Box(rt, "Card", Vector2.zero, Vector2.one, new Color(1f, 0.97f, 0.98f));
        var photoArea = Box(rt, "PhotoBg", new Vector2(0.08f, 0.33f), new Vector2(0.92f, 0.95f), new Color(0.9f, 0.85f, 0.9f));
        var photo = new GameObject("Photo", typeof(RawImage));
        photo.layer = 5;
        var prt = (RectTransform)photo.transform;
        prt.SetParent(photoArea, false);
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one; prt.offsetMin = prt.offsetMax = Vector2.zero;
        var img = photo.GetComponent<RawImage>();
        if (selfie && selfie.Photo) img.texture = selfie.Photo;
        else { img.color = Color.clear; Label(photoArea, font, "No selfie this time", 22, new Vector2(0f, 0.4f), new Vector2(1f, 0.6f)); }

        string themeName = routine.theme ? routine.theme.themeName : "";
        Label(rt, font, "Glow Up Complete!", 34, new Vector2(0f, 0.22f), new Vector2(1f, 0.32f), FontStyle.Bold);
        var h = "<color=#FF73B3>" + new string('♥', hearts) + "</color><color=#D8C8D2>" + new string('♥', 5 - hearts) + "</color>";
        Label(rt, font, h, 40, new Vector2(0f, 0.12f), new Vector2(1f, 0.23f));
        int left = routine.timer ? Mathf.CeilToInt(routine.timer.Remaining) : 0;
        int match = routine.score ? Mathf.RoundToInt(routine.score.LastThemeMatch * 100f) : 0;
        Label(rt, font, $"{themeName} · {routine.CompletedCount}/{routine.steps.Count} steps · {left / 60}:{left % 60:00} left · theme {match}%",
              18, new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.12f));
    }

    static RectTransform Box(RectTransform parent, string name, Vector2 min, Vector2 max, Color c)
    {
        var go = new GameObject(name, typeof(Image));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = c;
        return rt;
    }

    static void Label(RectTransform parent, Font font, string text, int size, Vector2 min, Vector2 max, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject("Text", typeof(Text));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.GetComponent<Text>();
        t.font = font; t.fontSize = size; t.fontStyle = style; t.text = text; t.supportRichText = true;
        t.alignment = TextAnchor.MiddleCenter; t.color = new Color(0.4f, 0.15f, 0.32f);
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
    }
}
