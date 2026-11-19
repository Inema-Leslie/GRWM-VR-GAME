using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UIKit;

// "The big reveal" at the end of the morning: your selfie as a polaroid, the glow score out of 100,
// hearts, a stylist note about the outfit, and what to do next.
public class ScoreScreen : MonoBehaviour
{
    public RoutineManager routine;
    public SelfieStep selfie;
    public float metersPerPixel = 0.001f;

    const float W = 744f, H = 860f, X = 366f, C = 354f;   // X = right column

    Transform holder;

    void Start()
    {
        if (routine) routine.DayFinished += Show;
    }

    void Show(int hearts)
    {
        if (Camera.main == null) return;
        var p = WorldPanel("ScoreScreen", new Vector2(W, H), metersPerPixel, out holder);
        var fl = holder.gameObject.AddComponent<FloatInView>();
        fl.distance = 1.2f;
        fl.below = 0.12f;
        Box(p, "Card", 0f, 0f, W, H, Bg, 36f, Border, 2f);

        string theme = routine.theme ? routine.theme.themeName : "Today";
        int glow = routine.score ? routine.score.LastGlow : hearts * 20;
        float match = routine.score ? routine.score.LastThemeMatch : 0f;

        // top bar
        IconButton(p, "Close", 24f, 28f, "back", () => holder.gameObject.SetActive(false));
        Label(p, "THE BIG REVEAL", Face.Caps, 12f, Mauve, 0f, 42f, W, 16f, TextAlignmentOptions.Top);
        IconButton(p, "Home", W - 68f, 28f, "home", Restart);

        Polaroid(p, theme);
        Stats(p, match);

        // celebration
        Icon(p, "heart_fill", X + 132f, 99f, 22f, PaleHeart);
        Icon(p, "heart_fill", X + 162f, 91f, 30f, Pink);
        Icon(p, "heart_fill", X + 200f, 99f, 22f, PaleHeart);
        Label(p, Headline(hearts), Face.Title, 35f, Plum, X, 132f, C, 90f, TextAlignmentOptions.Top);
        Icon(p, "sparkle", X + 13f, 151f, 23f, SoftLilac);
        Icon(p, "sparkle", X + 321f, 191f, 17f, SoftLilac);

        // score card
        const float SY = 259f;
        Box(p, "ScoreCard", X, SY, C, 194f, Card, 28f, Border, 1f);
        Box(p, "ThemeTag", X + 91f, SY - 14f, 172f, 28f, Lilac, 14f);
        Label(p, ("LOOK 01 · " + theme).ToUpper(), Face.Caps, 10f, Plum, X + 91f, SY - 8f, 172f, 14f, TextAlignmentOptions.Top);
        Label(p, "Your glow score", Face.Body, 15f, Mauve, X, SY + 31f, C, 20f, TextAlignmentOptions.Top);
        Label(p, glow.ToString(), Face.Title, 76f, Pink, X + 24f, SY + 44f, 205f, 92f, TextAlignmentOptions.TopRight);
        Label(p, "/100", Face.Medium, 20f, Mauve, X + 231f, SY + 92f, 65f, 24f);
        Hearts(p, X + 110f, SY + 144f, hearts);
        Icon(p, "sparkle", X + 32f, SY + 89f, 27f, SoftLilac);
        Icon(p, "sparkle", X + 301f, SY + 49f, 19f, SoftLilac);

        // stylist note
        var (noteTitle, noteBody) = Note(match, theme);
        Box(p, "StylistNote", X, 474f, C, 104f, Lilac, 22f);
        Label(p, noteTitle, Face.Medium, 19f, Plum, X + 20f, 489f, C - 40f, 23f);
        Label(p, noteBody, Face.Body, 14f, Plum, X + 20f, 518f, C - 40f, 50f);

        // hearts earned (for looks: there's no shop yet)
        Box(p, "Rewards", X, 598f, C, 64f, Blush, 20f);
        Icon(p, "heart_fill", X + 18f, 618f, 24f, Pink);
        Label(p, $"+{hearts * 10} hearts", Face.Medium, 18f, Plum, X + 55f, 608f, 176f, 22f);
        Label(p, "A little love for your look", Face.Body, 12f, Mauve, X + 55f, 634f, 231f, 16f);
        Icon(p, "check", X + 312f, 620f, 19f, Pink);

        // actions
        Button(p, "Again", X, 690f, C, 56f, true, "Another little glow-up", "sparkle", 20f, Restart);
        Button(p, "Room", X, 760f, C, 44f, false, "Back to my room", null, 16f, () => holder.gameObject.SetActive(false));

        Label(p, "YOUR KIND OF PRETTY. ALWAYS.", Face.Caps, 10f, Mauve, 0f, 825f, W, 14f, TextAlignmentOptions.Top);
        AudioFX.Play(AudioFX.Chime, holder.position, 0.6f);
    }

    // left column: the selfie, a little tilted like a real polaroid
    void Polaroid(RectTransform p, string theme)
    {
        var shadow = Box(p, "PolaroidShadow", 54f, 128f, 266f, 500f, Shadow, 10f);
        var frame = Box(p, "Polaroid", 48f, 120f, 266f, 500f, Card, 10f, Border, 1f);
        var root = (RectTransform)frame.transform.parent;
        root.localRotation = Quaternion.Euler(0f, 0f, 3f);
        ((RectTransform)shadow.transform).localRotation = root.localRotation;

        var photoBg = Box(root, "PhotoBg", 18f, 18f, 230f, 409f, Lilac, 6f);
        var photo = new GameObject("Photo", typeof(UnityEngine.UI.RawImage));
        photo.layer = 5;
        var prt = (RectTransform)photo.transform;
        prt.SetParent(photoBg.transform, false);
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one; prt.offsetMin = prt.offsetMax = Vector2.zero;
        var img = photo.GetComponent<UnityEngine.UI.RawImage>();
        img.raycastTarget = false;
        if (selfie && selfie.Photo) img.texture = selfie.Photo;
        else
        {
            img.enabled = false;
            Icon((RectTransform)photoBg.transform, "heart_line", 95f, 170f, 40f, PaleHeart);
            Label((RectTransform)photoBg.transform, "No selfie this time", Face.Body, 14f, Mauve, 0f, 220f, 230f, 20f, TextAlignmentOptions.Top);
        }
        Label(root, theme, Face.Medium, 20f, Plum, 0f, 444f, 266f, 26f, TextAlignmentOptions.Top);
    }

    // small pills under the polaroid: steps, time left, theme match
    void Stats(RectTransform p, float match)
    {
        int left = routine.timer ? Mathf.CeilToInt(routine.timer.Remaining) : 0;
        string[] pills =
        {
            $"{routine.CompletedCount}/{routine.steps.Count} STEPS",
            routine.TimedOut ? "OUT OF TIME" : $"{left / 60}:{left % 60:00} LEFT",
            $"THEME {Mathf.RoundToInt(match * 100f)}%"
        };
        float y = 660f;
        foreach (var s in pills)
        {
            Box(p, "Stat", 64f, y, 236f, 30f, Card, 15f, Border, 1f);
            Label(p, s, Face.Caps, 11f, Plum, 64f, y + 8f, 236f, 16f, TextAlignmentOptions.Top);
            y += 40f;
        }
    }

    static string Headline(int hearts) => hearts >= 5 ? "You’re a total\nsweetheart!"
                                        : hearts == 4 ? "Glowing and\ngoing!"
                                        : hearts == 3 ? "Cute and\nready to go!"
                                        : "Rushed, but\nstill cute!";

    static (string, string) Note(float match, string theme)
    {
        if (match >= 0.66f) return ("A match made in blush", $"Every piece says {theme}.\nSo you. So cute.");
        if (match >= 0.33f) return ("Almost picture perfect", $"Some pieces fit {theme}.\nSwap one more next time.");
        return ("Bold choice, babe", $"Not quite {theme}, but you\nwore it with confidence.");
    }

    static void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
