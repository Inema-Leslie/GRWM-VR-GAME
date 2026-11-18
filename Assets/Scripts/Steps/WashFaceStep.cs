using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Day 3: turn on the tap, lean in to splash, then pat your face with the towel.
public class WashFaceStep : RoutineStep
{
    public Pressable tap;
    public Transform spout;
    public XRGrabInteractable towel;
    public float splashSeconds = 2.5f;
    public float drySeconds = 2f;
    public float leanDistance = 0.9f;

    enum Phase { TapOn, Splash, Dry }
    Phase phase;
    float splash, dry;
    bool running;
    ParticleSystem water;
    AudioSource waterSound;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        water = SparkleFX.MakeWater(spout);
        waterSound = spout.gameObject.AddComponent<AudioSource>();
        waterSound.clip = AudioFX.Water;
        waterSound.loop = true;
        waterSound.spatialBlend = 1f;
        waterSound.volume = 0.5f;
        tap.Pressed += ToggleWater;   // the tap works any time, not just during this step
    }

    void ToggleWater()
    {
        running = !running;
        if (!water) return;
        if (running) { water.Play(); waterSound.Play(); } else { water.Stop(); waterSound.Stop(); }
    }

    public override float Progress => IsComplete ? 1f :
        phase == Phase.TapOn ? 0f : 0.1f + 0.5f * splash / splashSeconds + 0.4f * dry / drySeconds;

    public override string Hint => phase switch
    {
        Phase.TapOn => "Turn on the tap (the pink knob)",
        Phase.Splash => running ? "Lean over the sink to splash your face (desktop: hold Space)" : "Turn the water back on",
        _ => "Dry your face with the towel (desktop: hold Space)"
    };

    protected override void OnBegin()
    {
        phase = running ? Phase.Splash : Phase.TapOn;
        splash = dry = 0f;
    }

    void Update()
    {
        if (!IsActive) return;
        if (cam == null) cam = Camera.main;

        switch (phase)
        {
            case Phase.TapOn:
                if (running) phase = Phase.Splash;
                break;

            case Phase.Splash:
                if (running && cam && Vector3.Distance(cam.transform.position, spout.position) < leanDistance)
                    splash += Time.deltaTime;
                if (splash >= splashSeconds)
                {
                    splash = splashSeconds;
                    phase = Phase.Dry;
                    SparkleFX.Burst(spout.position);
                }
                break;

            case Phase.Dry:
                if (MouthZone.IsNear(towel.transform.position, 0.3f)) dry += Time.deltaTime;
                if (dry >= drySeconds)
                {
                    dry = drySeconds;
                    if (running) ToggleWater();   // don't waste water
                    SparkleFX.Burst(towel.transform.position);
                    CompleteStep();
                }
                break;
        }
    }
}
