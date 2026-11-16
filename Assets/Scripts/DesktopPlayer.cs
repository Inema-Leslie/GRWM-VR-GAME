// Glow Up Morning - temporary desktop controls for testing without a headset.
// Sits on the XR Origin. While "Desktop Mode" is ticked it hides the VR controllers
// and the XR simulator, and lets you play with mouse + keyboard:
//   hold RIGHT mouse button + move mouse = look around (or use the arrow keys)
//   W A S D = walk     Shift = walk faster
// Untick "Desktop Mode" (or remove this component) for the real VR build.

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class DesktopPlayer : MonoBehaviour
{
    public bool desktopMode = true;
    public float lookSpeed = 0.15f;
    public float walkSpeed = 1.6f;

    Transform cam;
    CharacterController body;
    float pitch, floorY;

    void Awake()
    {
        if (!desktopMode) { enabled = false; return; }

        body = GetComponent<CharacterController>();
        floorY = transform.position.y;
        cam = Camera.main ? Camera.main.transform : GetComponentInChildren<Camera>().transform;

        // the headset normally drives the camera; switch that off so the mouse can
        var pose = cam.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
        if (pose) pose.enabled = false;

        // hide VR-only pieces while testing on the desktop
        foreach (var n in new[] { "Left Controller", "Right Controller" })
        {
            var t = FindChild(transform, n);
            if (t) t.gameObject.SetActive(false);
        }
        var sim = GameObject.Find("XR Interaction Simulator");
        if (sim) sim.SetActive(false);
    }

    void Update()
    {
        var mouse = Mouse.current;
        var keys = Keyboard.current;
        if (mouse == null || keys == null) return;

        // look: hold right mouse button
        if (mouse.rightButton.isPressed)
        {
            Vector2 d = mouse.delta.ReadValue() * lookSpeed;
            transform.Rotate(0f, d.x, 0f, Space.World);
            pitch = Mathf.Clamp(pitch - d.y, -80f, 80f);
            cam.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // look with the arrow keys too
        float turn = (keys.rightArrowKey.isPressed ? 1 : 0) - (keys.leftArrowKey.isPressed ? 1 : 0);
        float tilt = (keys.downArrowKey.isPressed ? 1 : 0) - (keys.upArrowKey.isPressed ? 1 : 0);
        if (turn != 0 || tilt != 0)
        {
            transform.Rotate(0f, turn * 90f * Time.deltaTime, 0f, Space.World);
            pitch = Mathf.Clamp(pitch + tilt * 60f * Time.deltaTime, -80f, 80f);
            cam.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // walk: WASD relative to where you're facing
        Vector3 input = new Vector3(
            (keys.dKey.isPressed ? 1 : 0) - (keys.aKey.isPressed ? 1 : 0), 0f,
            (keys.wKey.isPressed ? 1 : 0) - (keys.sKey.isPressed ? 1 : 0));
        float speed = walkSpeed * (keys.leftShiftKey.isPressed ? 2f : 1f);
        Vector3 move = transform.TransformDirection(Vector3.ClampMagnitude(input, 1f)) * speed;

        body.Move(move * Time.deltaTime);

        // the floor is flat, so just keep the player's feet on it
        if (Mathf.Abs(transform.position.y - floorY) > 0.001f)
        {
            body.enabled = false;
            transform.position = new Vector3(transform.position.x, floorY, transform.position.z);
            body.enabled = true;
        }
    }

    static Transform FindChild(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }
}
