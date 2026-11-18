using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Skincare: pump the moisturiser (trigger while holding it), then pat it on your face.
public class SkincareStep : RoutineStep
{
    public XRGrabInteractable bottle;
    public Activatable pump;
    public Transform nozzle;
    public int pumpsNeeded = 2;
    public float patSeconds = 2f;

    int pumps;
    float pat;

    void Start()
    {
        if (pump) pump.Activated += OnPump;
    }

    void OnPump()
    {
        SparkleFX.Burst(nozzle ? nozzle.position : bottle.transform.position);
        AudioFX.Play(AudioFX.Pop, bottle.transform.position);
        if (IsActive && pumps < pumpsNeeded) pumps++;
    }

    public override float Progress => IsComplete ? 1f : 0.5f * pumps / pumpsNeeded + 0.5f * pat / patSeconds;

    public override string Hint => pumps < pumpsNeeded
        ? $"Pump the moisturiser {pumpsNeeded - pumps}x (trigger / desktop: E)"
        : "Pat it on your face (desktop: hold Space)";

    protected override void OnBegin()
    {
        pumps = 0;
        pat = 0f;
    }

    void Update()
    {
        if (!IsActive || pumps < pumpsNeeded) return;
        if (MouthZone.IsNear(bottle.transform.position, 0.3f)) pat += Time.deltaTime;
        if (pat < patSeconds) return;
        pat = patSeconds;
        SparkleFX.Burst(bottle.transform.position);
        CompleteStep();
    }
}
