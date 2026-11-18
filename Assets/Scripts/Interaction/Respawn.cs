using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// GDD: anything dropped on the floor goes back to its spot after 5 seconds.
public class Respawn : MonoBehaviour
{
    public float floorHeight = 0.2f;
    public float delay = 5f;

    Vector3 homePos;
    Quaternion homeRot;
    XRBaseInteractable grab;
    float onFloor;

    void Awake()
    {
        homePos = transform.position;
        homeRot = transform.rotation;
        grab = GetComponent<XRBaseInteractable>();
    }

    void Update()
    {
        bool dropped = transform.position.y < floorHeight && !(grab && grab.isSelected);
        onFloor = dropped ? onFloor + Time.deltaTime : 0f;
        if (onFloor < delay) return;
        onFloor = 0f;
        transform.SetPositionAndRotation(homePos, homeRot);
        SparkleFX.Burst(homePos);
        AudioFX.Play(AudioFX.Pop, homePos);
    }
}
