using UnityEngine;

// A piece of clothing you can pick up (on its hanger, or a pair of shoes).
// Its data lives in the ClothingItem asset. Remembers its spot so it can go back to the wardrobe.
public class ClothingProp : MonoBehaviour
{
    public ClothingItem item;

    Vector3 homePos;
    Quaternion homeRot;

    void Awake()
    {
        homePos = transform.position;
        homeRot = transform.rotation;
    }

    public void ReturnHome()
    {
        transform.SetPositionAndRotation(homePos, homeRot);
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
    }
}
