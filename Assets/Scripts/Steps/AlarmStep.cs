using UnityEngine;

// Day 1: the clock rings and shakes until you tap it.
public class AlarmStep : RoutineStep
{
    public Pressable button;
    public Transform clock;

    AudioSource source;
    Quaternion rest;
    bool ringing;

    void Awake()
    {
        source = clock.gameObject.AddComponent<AudioSource>();
        source.clip = MakeBeep();
        source.loop = true;
        source.spatialBlend = 1f;
        source.minDistance = 0.5f;
        source.volume = 0.6f;
    }

    protected override void OnBegin()
    {
        rest = clock.rotation;
        ringing = true;
        source.Play();
        button.Pressed += TurnOff;
    }

    void TurnOff()
    {
        if (IsActive) CompleteStep();
    }

    protected override void OnEnd()
    {
        button.Pressed -= TurnOff;
        ringing = false;
        source.Stop();
        clock.rotation = rest;
        SparkleFX.Burst(clock.position + Vector3.up * 0.1f);
    }

    void Update()
    {
        if (ringing) clock.rotation = Quaternion.AngleAxis(Mathf.Sin(Time.time * 45f) * 7f, Vector3.up) * rest;
    }

    // beep-beep ... beep-beep, made in code so there's no audio file to import
    static AudioClip MakeBeep()
    {
        const int rate = 44100;
        var data = new float[rate];
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / rate;
            bool on = t < 0.12f || (t > 0.2f && t < 0.32f);
            data[i] = on ? Mathf.Sin(2f * Mathf.PI * 1046f * t) * 0.35f : 0f;
        }
        var clip = AudioClip.Create("AlarmBeep", data.Length, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
