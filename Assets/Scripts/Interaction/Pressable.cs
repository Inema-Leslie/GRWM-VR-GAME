using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Fires Pressed when a VR hand selects it (XR Simple Interactable) or the desktop mouse clicks it.
public class Pressable : MonoBehaviour, IPressable
{
    public event Action Pressed;
    XRBaseInteractable xr;

    void OnEnable()
    {
        xr = GetComponent<XRBaseInteractable>();
        if (xr) xr.selectEntered.AddListener(OnSelect);
    }

    void OnDisable()
    {
        if (xr) xr.selectEntered.RemoveListener(OnSelect);
    }

    void OnSelect(SelectEnterEventArgs args) => Press();

    public void Press() => Pressed?.Invoke();
}
