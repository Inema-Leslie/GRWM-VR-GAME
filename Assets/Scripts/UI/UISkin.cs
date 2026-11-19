using TMPro;
using UnityEngine;

// The look from the Figma inspo: sprites and fonts in one place. Filled in by Glow Up > Build Apartment.
[CreateAssetMenu(menuName = "GRWM/UI Skin")]
public class UISkin : ScriptableObject
{
    public Sprite round;            // white 9-sliced rounded rect, tinted per use
    public Sprite vanity;           // heart mirror illustration for the landing page
    public Sprite[] icons;          // white line icons, named icon_<name>

    public TMP_FontAsset title;     // Fredoka SemiBold
    public TMP_FontAsset medium;    // Fredoka Medium
    public TMP_FontAsset body;      // Nunito Bold
    public TMP_FontAsset caps;      // Nunito ExtraBold

    static UISkin cached;
    public static UISkin Get()
    {
        if (cached == null) cached = Resources.Load<UISkin>("GRWM_UISkin");
        if (cached == null) cached = CreateInstance<UISkin>();   // still works, just plain
        return cached;
    }

    public Sprite Icon(string name)
    {
        if (icons == null) return null;
        foreach (var s in icons) if (s && s.name == "icon_" + name) return s;
        return null;
    }
}
