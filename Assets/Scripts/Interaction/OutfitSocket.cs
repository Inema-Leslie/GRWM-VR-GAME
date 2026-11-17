using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Socket on your body (chest, hips, feet). Only takes clothes for its slot; when filled it hides
// the hanger and shows the fitted mesh in the item's colour. Empty top/bottom = pyjamas.
public class OutfitSocket : XRSocketInteractor
{
    public ClothingSlot slot;
    public Renderer wornMesh;
    public Color pajamaColor = new Color(0.80f, 0.82f, 0.98f);

    public ClothingItem Worn { get; private set; }

    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock block;

    protected override void Awake()
    {
        base.Awake();
        block = new MaterialPropertyBlock();
        ShowEmpty();
    }

    // pyjamas on top and bottom, bare feet
    void ShowEmpty()
    {
        if (!wornMesh) return;
        if (slot == ClothingSlot.Shoes) { wornMesh.enabled = false; return; }
        Tint(pajamaColor);
        wornMesh.enabled = true;
    }

    void Tint(Color c)
    {
        wornMesh.GetPropertyBlock(block);
        block.SetColor(BaseColor, c);
        wornMesh.SetPropertyBlock(block);
    }

    public override bool CanHover(IXRHoverInteractable i) => base.CanHover(i) && Fits(i);
    public override bool CanSelect(IXRSelectInteractable i) => base.CanSelect(i) && Fits(i);

    public bool Fits(IXRInteractable i)
    {
        var p = i.transform.GetComponent<ClothingProp>();
        return p && p.item && p.item.slot == slot;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        var p = args.interactableObject.transform.GetComponent<ClothingProp>();
        if (p == null) return;
        Worn = p.item;
        ShowProp(p, false);
        if (wornMesh) { Tint(p.item.color); wornMesh.enabled = true; }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        var p = args.interactableObject.transform.GetComponent<ClothingProp>();
        if (p) ShowProp(p, true);
        Worn = null;
        ShowEmpty();
    }

    // while worn the hanger rides along invisibly; its colliders turn into triggers so they
    // don't bump your body, but a VR hand can still grab it to take it off
    static void ShowProp(ClothingProp p, bool show)
    {
        foreach (var r in p.GetComponentsInChildren<Renderer>()) r.enabled = show;
        foreach (var c in p.GetComponentsInChildren<Collider>()) c.isTrigger = !show;
    }
}
