using UnityEngine;

public class PlatformData : MonoBehaviour
{
    [Tooltip("The physical length of this platform chunk along the Z-axis.")]
    public float platformLength = 10f;

    [Tooltip("Optional entry offset if the track doesn't start at X=0.")]
    public float entryOffsetX = 0f;

    [Tooltip("Optional exit offset where the next platform should snap on the X-axis.")]
    public float exitOffsetX = 0f;
}
