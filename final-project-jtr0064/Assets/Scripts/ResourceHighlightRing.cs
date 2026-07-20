using UnityEngine;

// Golden ring dropped under a resource while Ability Two (Highlight) is active. Built
// entirely from a procedural mesh + material since the project has no outline/glow shaders
// or ring textures to reuse - see PlayerAbilities.OnAbilityTwo.
public class ResourceHighlightRing : MonoBehaviour
{
    private const int Segments = 48;
    private const float RingWidth = 0.35f;
    private const float SpinDegreesPerSecond = 60f;
    private const float PulseSpeed = 3f;

    private static readonly Color GoldColor = new Color(1f, 0.82f, 0.15f);

    private Material material;

    public static ResourceHighlightRing Attach(Transform target)
    {
        Bounds bounds = new Bounds(target.position, Vector3.zero);
        bool hasBounds = false;
        foreach (Renderer r in target.GetComponentsInChildren<Renderer>()) {
            if (!hasBounds) {
                bounds = r.bounds;
                hasBounds = true;
            } else {
                bounds.Encapsulate(r.bounds);
            }
        }

        float radius = hasBounds ? Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.3f : 1.5f;
        float groundY = hasBounds ? bounds.min.y : target.position.y;

        GameObject go = new GameObject("HighlightRing");
        go.transform.SetParent(target, false);
        go.transform.position = new Vector3(target.position.x, groundY + 0.05f, target.position.z);

        // Ring radius above is computed in world space, so cancel out any non-uniform scale
        // on the resource itself to avoid it being re-applied on top.
        Vector3 parentScale = target.lossyScale;
        go.transform.localScale = new Vector3(
            parentScale.x != 0f ? 1f / parentScale.x : 1f,
            parentScale.y != 0f ? 1f / parentScale.y : 1f,
            parentScale.z != 0f ? 1f / parentScale.z : 1f);

        ResourceHighlightRing ring = go.AddComponent<ResourceHighlightRing>();
        ring.Build(radius);
        return ring;
    }

    private void Build(float radius)
    {
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        filter.mesh = BuildRingMesh(radius, radius + RingWidth, Segments);

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        material = new Material(shader);
        ConfigureTransparent(material);
        material.SetColor("_BaseColor", GoldColor);

        meshRenderer.material = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    // Standard URP script-side recipe for switching a Lit/Unlit material to alpha-blended
    // transparent (the same thing the shader's Inspector GUI does when you flip the
    // Surface Type dropdown to Transparent).
    private static void ConfigureTransparent(Material m)
    {
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, SpinDegreesPerSecond * Time.deltaTime, Space.World);

        if (material != null) {
            Color c = GoldColor;
            c.a = 0.55f + 0.45f * Mathf.Sin(Time.time * PulseSpeed);
            material.SetColor("_BaseColor", c);
        }
    }

    private void OnDestroy()
    {
        if (material != null) {
            Destroy(material);
        }
    }

    private static Mesh BuildRingMesh(float innerRadius, float outerRadius, int segments)
    {
        Vector3[] vertices = new Vector3[segments * 2];
        // Double-sided so the ring is visible from above and from below.
        int[] triangles = new int[segments * 12];

        for (int i = 0; i < segments; i++) {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);
            vertices[i * 2] = new Vector3(x * innerRadius, 0f, z * innerRadius);
            vertices[i * 2 + 1] = new Vector3(x * outerRadius, 0f, z * outerRadius);
        }

        for (int i = 0; i < segments; i++) {
            int next = (i + 1) % segments;
            int a = i * 2;
            int b = i * 2 + 1;
            int c = next * 2;
            int d = next * 2 + 1;

            int t = i * 12;
            triangles[t] = a; triangles[t + 1] = c; triangles[t + 2] = b;
            triangles[t + 3] = b; triangles[t + 4] = c; triangles[t + 5] = d;
            triangles[t + 6] = a; triangles[t + 7] = b; triangles[t + 8] = c;
            triangles[t + 9] = b; triangles[t + 10] = d; triangles[t + 11] = c;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
