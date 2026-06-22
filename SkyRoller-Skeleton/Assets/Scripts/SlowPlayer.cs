using UnityEngine;

public class SlowPlayer : MonoBehaviour
{
    [SerializeField] float slowSpeed = 4f;
    [SerializeField] float slowDuration = 1f;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collider.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ActivateSlow(slowSpeed, slowDuration);
            }
        }
    }
}
