using UnityEngine;
using TMPro;

public class CompassMarkerBar : MonoBehaviour
{

    public Transform player;
    public RectTransform markerContainer;
    public TMP_Text markerPrefab;

    private float panelWidth = 500f;
    private float pixelsPerDegree = 4f;

    private readonly string[] labels = 
    { 
        "N", "NE", "E", "SE", "S", "SW", "W", "NW"
    };

    private TMP_Text[] markers;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        markers = new TMP_Text[labels.Length];

        // Deactivate the template prefab in case it is an active scene object
        if (markerPrefab != null) {
            markerPrefab.gameObject.SetActive(false);
        }

        for (int i = 0; i < labels.Length; i++) {
            TMP_Text marker = Instantiate(markerPrefab, markerContainer);
            marker.text = labels[i];
            marker.alignment = TextAlignmentOptions.Center;
            marker.gameObject.SetActive(true);
            markers[i] = marker;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || markers == null) {
            return;
        }

        float heading = player.eulerAngles.y;

        for (int i = 0; i < markers.Length; i++) {
            float markerAngle = i * 45f;
            float delta = Mathf.DeltaAngle(heading, markerAngle);
            float x = delta * pixelsPerDegree;

            RectTransform rect = markers[i].rectTransform;
            rect.anchoredPosition = new Vector2(x, 0f);

            // Hide markers that are outside the half-width of the panel
            bool visible = Mathf.Abs(x) < (panelWidth * 0.5f);
            markers[i].gameObject.SetActive(visible);
        }
    }
}

