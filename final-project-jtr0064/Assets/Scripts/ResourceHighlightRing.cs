using UnityEngine;

// Glowing shell dropped over a resource's own hitbox while Ability Two (Highlight) is
// active - see PlayerAbilities.OnAbilityTwo. The shell is a primitive (sphere/capsule/box)
// sized and positioned to match the resource's Collider exactly, so the thing that lights
// up is the actual interaction hitbox rather than an arbitrary ring under the model. Uses
// Custom/HighlightXRay (Assets/Shaders/HighlightXRay.shader) so the glow is visible through
// walls and through the resource's own mesh - that's the point of the ability.
public class ResourceHighlightRing : MonoBehaviour
{
    private const float SpinDegreesPerSecond = 60f;
    private const float PulseSpeed = 3f;
    // Slightly larger than the collider so the shell doesn't z-fight with a mesh surface
    // sitting right at the hitbox boundary.
    private const float ShapePadding = 1.05f;
    private const float FallbackRadius = 1.5f;

    private static readonly Color GoldColor = new Color(1f, 0.82f, 0.15f);

    private Material material;

    public static ResourceHighlightRing Attach(Transform target)
    {
        Collider collider = target.GetComponent<Collider>();

        GameObject go;
        if (collider is SphereCollider sphere) {
            go = BuildSphereShape(target, sphere);
        } else if (collider is CapsuleCollider capsule) {
            go = BuildCapsuleShape(target, capsule);
        } else if (collider is BoxCollider box) {
            go = BuildBoxShape(target, box);
        } else {
            go = BuildFallbackShape(target);
        }

        ResourceHighlightRing ring = go.AddComponent<ResourceHighlightRing>();
        ring.Init();
        return ring;
    }

    private static GameObject BuildSphereShape(Transform target, SphereCollider collider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "HighlightHitbox";
        StripCollider(go);
        go.transform.SetParent(target, false);

        go.transform.localPosition = collider.center;
        go.transform.localRotation = Quaternion.identity;
        float diameter = collider.radius * 2f * ShapePadding;
        go.transform.localScale = new Vector3(diameter, diameter, diameter);
        return go;
    }

    private static GameObject BuildCapsuleShape(Transform target, CapsuleCollider collider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "HighlightHitbox";
        StripCollider(go);
        go.transform.SetParent(target, false);

        go.transform.localPosition = collider.center;
        // Unity's capsule primitive runs along its own local Y axis; rotate it to match
        // whichever axis the collider is configured for (0 = X, 1 = Y, 2 = Z). The
        // rotation direction doesn't matter since a capsule is radially symmetric.
        go.transform.localRotation = collider.direction switch {
            0 => Quaternion.Euler(0f, 0f, 90f),
            2 => Quaternion.Euler(90f, 0f, 0f),
            _ => Quaternion.identity,
        };

        float diameter = collider.radius * 2f * ShapePadding;
        float height = Mathf.Max(collider.height, collider.radius * 2f) * ShapePadding;
        go.transform.localScale = new Vector3(diameter, height * 0.5f, diameter);
        return go;
    }

    private static GameObject BuildBoxShape(Transform target, BoxCollider collider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "HighlightHitbox";
        StripCollider(go);
        go.transform.SetParent(target, false);

        go.transform.localPosition = collider.center;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = collider.size * ShapePadding;
        return go;
    }

    // Safety net for resources with a MeshCollider or no collider at all: fall back to the
    // old renderer-bounds ring so Highlight never silently does nothing.
    private static GameObject BuildFallbackShape(Transform target)
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

        float radius = hasBounds ? Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) : FallbackRadius;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "HighlightHitbox";
        StripCollider(go);
        go.transform.SetParent(target, false);

        Vector3 parentScale = target.lossyScale;
        Vector3 center = hasBounds ? bounds.center : target.position;
        go.transform.position = center;
        // Fallback radius is computed in world space, so cancel out any non-uniform scale
        // on the resource itself before applying it as a local scale.
        float diameter = radius * 2f * ShapePadding;
        go.transform.localScale = new Vector3(
            parentScale.x != 0f ? diameter / parentScale.x : diameter,
            parentScale.y != 0f ? diameter / parentScale.y : diameter,
            parentScale.z != 0f ? diameter / parentScale.z : diameter);
        return go;
    }

    private static void StripCollider(GameObject go)
    {
        Collider primitiveCollider = go.GetComponent<Collider>();
        if (primitiveCollider != null) {
            primitiveCollider.enabled = false;
            Destroy(primitiveCollider);
        }
    }

    private void Init()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        // The X-ray shader draws through walls and the resource's own mesh, which is the
        // point of this ability. Fall back to a normal occludable transparent material if
        // it's ever missing so Highlight still does something rather than erroring out.
        Shader shader = Shader.Find("Custom/HighlightXRay");
        if (shader != null) {
            material = new Material(shader);
        } else {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            ConfigureTransparent(material);
        }
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
}
