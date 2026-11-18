using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Selfie: pick up the phone, point it at a mirror and press the trigger (desktop: E).
// The phone's camera looks out of its back, so it photographs your reflection.
public class SelfieStep : RoutineStep
{
    public XRGrabInteractable phone;
    public Activatable shutter;
    public Camera phoneCam;
    public Renderer screen;

    public Texture Photo { get; private set; }

    RenderTexture live;
    MaterialPropertyBlock block;
    static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        live = new RenderTexture(270, 480, 16) { name = "PhoneViewfinder" };
        phoneCam.targetTexture = live;
        phoneCam.enabled = false;          // only renders while this step is on (saves GPU)
        block = new MaterialPropertyBlock();
    }

    void Start()
    {
        if (shutter) shutter.Activated += Snap;
    }

    public override string Hint => "Grab the phone, face a mirror and take your selfie (trigger / desktop: E)";

    protected override void OnBegin()
    {
        phoneCam.enabled = true;
        screen.GetPropertyBlock(block);
        block.SetColor(BaseColor, Color.white);
        block.SetTexture(BaseMap, live);
        screen.SetPropertyBlock(block);
    }

    void Snap()
    {
        if (!IsActive) return;
        var shot = new RenderTexture(live.width, live.height, 0) { name = "Selfie" };
        phoneCam.Render();
        Graphics.Blit(live, shot);
        Photo = shot;
        AudioFX.Play(AudioFX.Click, phone.transform.position);
        SparkleFX.Burst(phone.transform.position);
        CompleteStep();
    }

    protected override void OnEnd()
    {
        phoneCam.enabled = false;
        if (Photo == null) return;
        screen.GetPropertyBlock(block);
        block.SetTexture(BaseMap, Photo);    // the photo stays on the phone
        screen.SetPropertyBlock(block);
    }
}
