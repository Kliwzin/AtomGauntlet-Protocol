using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LadderTrigger : MonoBehaviour
{
    [SerializeField] private Ladder ladder;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMovement movement = other.GetComponent<PlayerMovement>();
        if (movement != null && ladder != null)
        {
            movement.SetLadderZone(true, ladder);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerMovement movement = other.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetLadderZone(false, ladder);
        }
    }
}
