using TMPro;
using UnityEngine;
using static UIKit;

// First thing you see: what GRWM is, how a morning goes, and the button that starts it.
// The gear opens "Your cozy corner" (settings) on the same panel.
public class LandingScreen : MonoBehaviour
{
    public RoutineManager routine;
    public float metersPerPixel = 0.0012f;

    const float W = 784f, H = 640f;

    Transform holder;
    RectTransform canvas, landing, settings;

    void Start()
    {
        if (routine == null || routine.Started) { enabled = false; return; }
        canvas = WorldPanel("LandingScreen", new Vector2(W, H), metersPerPixel, out holder);
        holder.gameObject.AddComponent<FloatInView>().distance = 1.3f;
        Box(canvas, "Card", 0f, 0f, W, H, Bg, 36f, Border, 2f);
        landing = Node("Landing", canvas, 0f, 0f, W, H);
        settings = Node("Settings", canvas, 0f, 0f, W, H);
        BuildLanding(landing);
        BuildSettings(settings);
        settings.gameObject.SetActive(false);
    }

    void Begin()
    {
        holder.gameObject.SetActive(false);
        routine.BeginDay();
    }

    void ShowSettings(bool on)
    {
        landing.gameObject.SetActive(!on);
        settings.gameObject.SetActive(on);
    }

    // ---------------- landing ----------------

    void BuildLanding(RectTransform p)
    {
        // top bar: hearts balance + settings
        Box(p, "HeartBalance", 24f, 24f, 100f, 42f, Card, 21f, Border, 1f);
        Icon(p, "heart_fill", 36f, 36f, 18f, Pink);
        Label(p, "250", Face.Body, 16f, Plum, 62f, 33f, 50f, 22f);
        IconButton(p, "OpenSettings", W - 68f, 24f, "gear", () => ShowSettings(true));

        // left: welcome + illustration + daily treat
        const float L = 24f, C = 354f;
        Label(p, "a little me-time magic", Face.Caps, 13f, Mauve, L, 84f, C, 18f, TextAlignmentOptions.Top);
        Label(p, "GRWM", Face.Title, 51f, Pink, L, 98f, C, 62f, TextAlignmentOptions.Top);
        Icon(p, "sparkle", L + 40f, 120f, 20f, Pink);
        Icon(p, "heart_line", L + 296f, 150f, 16f, Pink);
        Label(p, "Get ready. Glow your own way.", Face.Body, 16f, Plum, L, 162f, C, 22f, TextAlignmentOptions.Top);
        Picture(p, UISkin.Get().vanity, L, 196f, C, 290f);
        Box(p, "Reminder", L + 71f, 466f, 212f, 28f, Card, 14f);
        Label(p, "a good day starts with you", Face.Body, 12f, Mauve, L + 71f, 471f, 212f, 18f, TextAlignmentOptions.Top);

        Box(p, "DailyGift", L, 510f, C, 80f, Lilac, 22f, LilacEdge, 1f);
        Box(p, "GiftBadge", L + 13f, 523f, 54f, 54f, Card, 18f);
        Icon(p, "gift", L + 27f, 537f, 26f, Orchid);
        Label(p, "A little treat for you", Face.Medium, 17f, Plum, L + 80f, 527f, 240f, 22f);
        Label(p, "Your daily gift is ready tomorrow", Face.Body, 12f, Mauve, L + 80f, 553f, 251f, 16f);

        // right: today's theme + how a morning goes + start
        const float R = 406f;
        string theme = routine.theme ? routine.theme.themeName : "Today";
        Box(p, "ThemeTag", R + 77f, 80f, 200f, 28f, Lilac, 14f);
        Label(p, ("TODAY · " + theme).ToUpper(), Face.Caps, 10f, Plum, R + 77f, 86f, 200f, 16f, TextAlignmentOptions.Top);
        Label(p, "How it works", Face.Medium, 22f, Plum, R, 118f, C, 28f);
        int mins = Mathf.RoundToInt((routine.theme ? routine.theme.timeLimit : 300f) / 60f);
        Label(p, $"Mirror floats over each spot and tells you what to do. You get {mins} minutes once the alarm is off.",
              Face.Body, 13f, Mauve, R, 148f, C, 36f);

        int n = routine.steps.Count;
        float rowH = Mathf.Min(32f, 224f / Mathf.Max(1, n));
        float cardH = n * rowH + 24f;
        Box(p, "Steps", R, 192f, C, cardH, Card, 24f, Border, 1f);
        for (int i = 0; i < n; i++)
        {
            float y = 204f + i * rowH;
            Box(p, "Num", R + 16f, y + (rowH - 24f) * 0.5f, 24f, 24f, Lilac, 12f);
            Label(p, (i + 1).ToString(), Face.Caps, 11f, Plum, R + 16f, y + (rowH - 24f) * 0.5f + 5f, 24f, 16f, TextAlignmentOptions.Top);
            var s = routine.steps[i];
            Label(p, s ? s.StepName : "", Face.Body, 15f, Plum, R + 52f, y + (rowH - 20f) * 0.5f, C - 70f, 20f);
        }

        float by = 192f + cardH + 14f;
        Button(p, "Start", R, by, C, 56f, true, "Let’s get ready!", "sparkle", 20f, Begin);
        Button(p, "LastLook", R, by + 68f, C, 50f, false, "My last look", "heart_line", 19f, null);   // just for looks for now

        Label(p, "MADE FOR YOUR MAIN CHARACTER MOMENT", Face.Caps, 9f, Mauve, 0f, H - 30f, W, 14f, TextAlignmentOptions.Top);
    }

    // ---------------- settings ----------------

    void BuildSettings(RectTransform p)
    {
        IconButton(p, "Back", 24f, 24f, "back", () => ShowSettings(false));
        Label(p, "MAKE YOURSELF AT HOME", Face.Caps, 11f, Mauve, 0f, 38f, W, 16f, TextAlignmentOptions.Top);
        Icon(p, "heart_line", W - 58f, 36f, 20f, Pink);

        Label(p, "Your cozy corner", Face.Title, 33f, Plum, 0f, 80f, W, 40f, TextAlignmentOptions.Top);
        Label(p, "Little tweaks. Just your vibe.", Face.Body, 15f, Mauve, 0f, 126f, W, 20f, TextAlignmentOptions.Top);

        PrefsCard(p, 24f, 172f, "SOUND & GOOD VIBES",
            ("music", "Music", "A dreamy little soundtrack", UIPrefs.Music, v => UIPrefs.Music = v),
            ("sound", "Sound effects", "Tiny taps and happy chimes", UIPrefs.SoundEffects, v => UIPrefs.SoundEffects = v));
        PrefsCard(p, 406f, 172f, "PLAY YOUR WAY",
            ("phone", "Haptics", "A soft little buzz", UIPrefs.Haptics, v => UIPrefs.Haptics = v),
            ("sparkle", "Gentle motion", "Keep things calm and still", UIPrefs.GentleMotion, v => UIPrefs.GentleMotion = v));

        Box(p, "Gentle", 24f, 368f, W - 48f, 73f, Lilac, 20f);
        Icon(p, "heart_line", 42f, 393f, 23f, Orchid);
        Label(p, "No rush. No rules. Just you.", Face.Medium, 17f, Plum, 78f, 383f, 500f, 22f);
        Label(p, "Your glow-up, at your own pace.", Face.Body, 12f, Mauve, 78f, 409f, 500f, 16f);

        Button(p, "Done", (W - 354f) * 0.5f, 474f, 354f, 56f, true, "All cozy!", null, 20f, () => ShowSettings(false));
        Label(p, "GRWM · V1.0", Face.Caps, 10f, Mauve, 0f, H - 30f, W, 14f, TextAlignmentOptions.Top);
    }

    static void PrefsCard(RectTransform p, float x, float y, string heading,
                          params (string icon, string title, string sub, bool on, System.Action<bool> set)[] rows)
    {
        Box(p, heading, x, y, 354f, 173f, Card, 24f, Border, 1f);
        Label(p, heading, Face.Caps, 10f, Mauve, x + 17f, y + 14f, 318f, 14f);
        for (int i = 0; i < rows.Length; i++)
        {
            float ry = y + i * 71f;
            var r = rows[i];
            Icon(p, r.icon, x + 17f, ry + 45f, 22f, Pink);
            Label(p, r.title, Face.Body, 16f, Plum, x + 51f, ry + 42f, 203f, 22f);
            Label(p, r.sub, Face.Body, 11f, Mauve, x + 51f, ry + 66f, 207f, 15f);
            Switch(p, x + 275f, ry + 49f, r.on, r.set);
            if (i < rows.Length - 1) Box(p, "Divider", x + 51f, ry + 98f, 282f, 1f, Divider, 0f);
        }
    }
}
