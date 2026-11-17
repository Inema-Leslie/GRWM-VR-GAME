using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Socket on the mannequin. Only takes clothes for its slot; when filled it hides the folded
// prop and shows the fitted mesh in the item's colour.
public class OutfitSocket : XRSocketInteractor
{
    public ClothingSlot slot;
    public Renderer wornMesh;

    public ClothingItem Worn { get; private set; }

    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock block;

    protected override void Awake()
    {
        base.Awake();
        block = new MaterialPropertyBlock();
        if (wornMesh) wornMesh.enabled = false;
    }

    public override bool CanHover(IXRHoverInteractable i) => base.CanHover(i) && Fits(i);
    public override bool CanSelect(IXRSelectInteractable i) => base.CanSelect(i) && Fits(i);

    bool Fits(IXRInteractable i)
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
        if (wornMesh)
        {
            wornMesh.GetPropertyBlock(block);
            block.SetColor(BaseColor, p.item.color);
            wornMesh.SetPropertyBlock(block);
            wornMesh.enabled = true;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        var p = args.interactableObject.transform.GetComponent<ClothingProp>();
        if (p) ShowProp(p, true);
        Worn = null;
        if (wornMesh) wornMesh.enabled = false;
    }

    // the folded prop stays inside the mannequin (invisible) so you can grab it back off her
    static void ShowProp(ClothingProp p, bool show)
    {
        foreach (var r in p.GetComponentsInChildren<Renderer>()) r.enabled = show;
    }
}
