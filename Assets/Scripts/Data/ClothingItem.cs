using UnityEngine;

public enum ClothingSlot { Top, Bottom, Shoes, Head }

[CreateAssetMenu(menuName = "GRWM/Clothing Item")]
public class ClothingItem : ScriptableObject
{
    public string itemName = "Tee";
    public ClothingSlot slot;
    public Color color = Color.white;
    public string[] styleTags = new string[0];
    [Tooltip("Name of the version fitted to her body in Blender (Fit_<name>), if there is one.")]
    public string fitName = "";

    public bool Matches(DayTheme theme)
    {
        if (theme == null) return false;
        foreach (var t in styleTags)
            if (System.Array.IndexOf(theme.wantedTags, t) >= 0) return true;
        return false;
    }
}
