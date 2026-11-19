using UnityEngine;

// Small sound effects made in code (no audio files to import or license).
public static class AudioFX
{
    const int Rate = 44100;
    static AudioClip chime, pop, click, water;

    public static AudioClip Chime => chime ??= Make("Chime", 0.6f, t =>
        Tone(t, 880f, 0f, 0.3f) + Tone(t, 1318.5f, 0.12f, 0.45f));          // two rising notes

    public static AudioClip Pop => pop ??= Make("Pop", 0.12f, t =>
        Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(900f, 300f, t / 0.12f) * t) * Mathf.Exp(-t * 30f) * 0.5f);

    public static AudioClip Click => click ??= Make("Click", 0.15f, t =>
        (Random.value * 2f - 1f) * Mathf.Exp(-t * 60f) * 0.6f);             // camera shutter

    public static AudioClip Water => water ??= Make("Water", 1f, t =>
        (Random.value * 2f - 1f) * 0.12f);                                   // soft hiss, looped

    public static void Play(AudioClip clip, Vector3 at, float volume = 0.7f)
    {
        if (clip && UIPrefs.SoundEffects) AudioSource.PlayClipAtPoint(clip, at, volume);
    }

    static float Tone(float t, float hz, float start, float len)
    {
        if (t < start || t > start + len) return 0f;
        float k = t - start;
        return Mathf.Sin(2f * Mathf.PI * hz * k) * Mathf.Exp(-k * 8f) * 0.35f;
    }

    static AudioClip Make(string name, float seconds, System.Func<float, float> f)
    {
        var data = new float[(int)(Rate * seconds)];
        for (int i = 0; i < data.Length; i++) data[i] = f((float)i / Rate);
        var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
