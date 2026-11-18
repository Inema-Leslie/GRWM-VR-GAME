using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

// Fixed foveated rendering on Quest: the edges of each eye are drawn at lower resolution,
// where you can't see the difference, which saves GPU time. Needs the OpenXR "Foveated Rendering"
// feature ticked; does nothing in the editor or on headsets without it.
public class FoveationSetup : MonoBehaviour
{
    [Range(0f, 1f)] public float level = 0.5f;   // 0 = off, 1 = strongest

    void Start()
    {
        var displays = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displays);
        foreach (var d in displays)
        {
            d.foveatedRenderingLevel = level;
            d.foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
        }
    }
}
