using UnityEngine;

// Settings from "Your cozy corner", kept between sessions.
public static class UIPrefs
{
    public static bool Music        { get => Get("Music", true);         set => Set("Music", value); }
    public static bool SoundEffects { get => Get("SoundEffects", true);  set => Set("SoundEffects", value); }
    public static bool Haptics      { get => Get("Haptics", true);       set => Set("Haptics", value); }
    public static bool GentleMotion { get => Get("GentleMotion", false); set => Set("GentleMotion", value); }

    static bool Get(string k, bool fallback) => PlayerPrefs.GetInt("GRWM_" + k, fallback ? 1 : 0) == 1;
    static void Set(string k, bool v) { PlayerPrefs.SetInt("GRWM_" + k, v ? 1 : 0); PlayerPrefs.Save(); }
}
