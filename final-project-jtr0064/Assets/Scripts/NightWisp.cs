using UnityEngine;

// Night-only hazard: a glowing wisp that wanders near resources after dark, chases the
// player within detectionRadius, and steals a point on contact. Deactivates (and resets
// back to its spawn point) during the day. The player can't be chased or hit within
// safeZoneRadius of any DropoffLocation.
public class NightWisp : MonoBehaviour
{
    [Tooltip("Assigned by the setup tool. Wisp is only active while this reports IsNight.")]
    public DayNightCycle dayNightCycle;
    [Tooltip("Assigned by the setup tool. Falls back to the scene's PlayerPoints if left empty.")]
    public Transform player;
    [Tooltip("Particle system + light child, shown only at night.")]
    public GameObject visualRoot;

    public float wanderSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public float wanderRadius = 6f;
    public float detectionRadius = 8f;
    [Tooltip("Player can't be chased or hit within this distance of any DropoffLocation.")]
    public float safeZoneRadius = 6f;

    public float bobHeight = 0.3f;
    public float bobSpeed = 1.5f;

    public int pointsStolen = 1;
    [Tooltip("Seconds after a hit before this wisp can chase/hit again.")]
    public float hitCooldown = 3f;

    private Collider wispCollider;
    private DropoffLocation[] dropoffs;

    private Vector3 spawnPosition;
    private Vector3 wanderTarget;
    private float visualBaseLocalY;

    private bool isActive;
    private bool onCooldown;
    private float cooldownTimer;

    void Start()
    {
        spawnPosition = transform.position;
        wanderTarget = spawnPosition;
        wispCollider = GetComponent<Collider>();

        if (visualRoot != null) {
            visualBaseLocalY = visualRoot.transform.localPosition.y;
        }

        if (dayNightCycle == null) {
            dayNightCycle = FindObjectOfType<DayNightCycle>();
        }

        if (player == null) {
            PlayerPoints playerPoints = FindObjectOfType<PlayerPoints>();
            if (playerPoints != null) {
                player = playerPoints.transform;
            }
        }

        dropoffs = FindObjectsOfType<DropoffLocation>();

        SetActive(false);
    }

    void Update()
    {
        bool isNight = dayNightCycle != null && dayNightCycle.IsNight;
        if (isNight != isActive) {
            SetActive(isNight);
        }

        if (!isActive) {
            return;
        }

        if (onCooldown) {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f) {
                onCooldown = false;
            }
        }

        UpdateMovement();
        UpdateBob();
    }

    private void SetActive(bool nowActive)
    {
        isActive = nowActive;

        if (visualRoot != null) {
            visualRoot.SetActive(nowActive);
        }
        if (wispCollider != null) {
            wispCollider.enabled = nowActive;
        }

        if (!nowActive) {
            // Reset so every night starts a fresh wander from the original spawn point.
            transform.position = spawnPosition;
            wanderTarget = spawnPosition;
            onCooldown = false;
        }
    }

    private void UpdateMovement()
    {
        if (!onCooldown && player != null && !IsPlayerInSafeZone()
            && Vector3.Distance(transform.position, player.position) <= detectionRadius) {
            Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);
            return;
        }

        if (Vector3.Distance(transform.position, wanderTarget) < 0.5f) {
            Vector3 offset = Random.insideUnitSphere * wanderRadius;
            wanderTarget = new Vector3(spawnPosition.x + offset.x, spawnPosition.y, spawnPosition.z + offset.z);
        }

        transform.position = Vector3.MoveTowards(transform.position, wanderTarget, wanderSpeed * Time.deltaTime);
    }

    private bool IsPlayerInSafeZone()
    {
        if (player == null || dropoffs == null) {
            return false;
        }

        foreach (DropoffLocation dropoff in dropoffs) {
            if (dropoff != null && Vector3.Distance(player.position, dropoff.transform.position) <= safeZoneRadius) {
                return true;
            }
        }

        return false;
    }

    private void UpdateBob()
    {
        if (visualRoot == null) {
            return;
        }

        float offsetY = visualBaseLocalY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        Vector3 localPos = visualRoot.transform.localPosition;
        visualRoot.transform.localPosition = new Vector3(localPos.x, offsetY, localPos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onCooldown) {
            return;
        }

        PlayerPoints hitPoints = other.GetComponentInParent<PlayerPoints>();
        if (hitPoints == null) {
            return;
        }

        if (IsPlayerInSafeZone()) {
            return;
        }

        hitPoints.RemovePoints(pointsStolen);
        onCooldown = true;
        cooldownTimer = hitCooldown;
    }
}
