using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

// Lives on the same GameObject as the player's Cinemachine3rdPersonFollow ("cm" child of
// PlayerFollowCamera). Z cycles CameraDistance through a fixed list of steps. Index 0 always
// mirrors whatever CameraDistance is already set to in the Inspector, so the current framing
// stays the closest zoom level and this never needs to be kept in sync by hand.
public class CameraZoom : MonoBehaviour
{
    [Tooltip("Extra zoom-out distances, cycled in order after the current (closest) distance.")]
    public float[] extraZoomDistances = { 7f, 11f, 16f };

    private Cinemachine3rdPersonFollow thirdPersonFollow;
    private float[] zoomDistances;
    private int zoomIndex;

    void Awake()
    {
        thirdPersonFollow = GetComponent<Cinemachine3rdPersonFollow>();

        zoomDistances = new float[extraZoomDistances.Length + 1];
        zoomDistances[0] = thirdPersonFollow != null ? thirdPersonFollow.CameraDistance : 4f;
        extraZoomDistances.CopyTo(zoomDistances, 1);
    }

    void Update()
    {
        if (thirdPersonFollow == null || Keyboard.current == null) {
            return;
        }

        if (Keyboard.current.zKey.wasPressedThisFrame) {
            zoomIndex = (zoomIndex + 1) % zoomDistances.Length;
            thirdPersonFollow.CameraDistance = zoomDistances[zoomIndex];
        }
    }
}
