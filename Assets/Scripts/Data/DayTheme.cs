using UnityEngine;

[CreateAssetMenu(menuName = "GRWM/Day Theme")]
public class DayTheme : ScriptableObject
{
    public string themeName = "Picnic Date";
    [TextArea] public string description = "";
    public string[] wantedTags = { "cute", "pastel" };
    public float timeLimit = 300f;   // seconds after the alarm
}
