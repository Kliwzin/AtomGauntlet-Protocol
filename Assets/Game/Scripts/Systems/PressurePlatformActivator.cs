using UnityEngine;

public class PressurePlatformActivator : MonoBehaviour
{
    [SerializeField] private MovingPlatform targetPlatform;
    [SerializeField] private float activationDelay = 2f;

    private float timer = 0f;
    private bool playerOnTop = false;

    private void Update()
    {
        if (!playerOnTop)
            return;

        timer += Time.deltaTime;

        if (timer >= activationDelay)
        {
            targetPlatform.ForceRaise();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.transform.CompareTag("Player"))
            return;

        playerOnTop = true;
        timer = 0f;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.transform.CompareTag("Player"))
            return;

        playerOnTop = false;
        timer = 0f;

        if (targetPlatform != null)
            targetPlatform.ForceLower();
    }
}