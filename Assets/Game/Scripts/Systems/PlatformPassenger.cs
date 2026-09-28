using UnityEngine;

public class PlatformPassenger : MonoBehaviour
{
    private MovingPlatform currentPlatform;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (currentPlatform == null || rb == null) return;

        rb.MovePosition(rb.position + currentPlatform.PlatformDelta);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<MovingPlatform>(out MovingPlatform platform))
        {
            currentPlatform = platform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<MovingPlatform>(out MovingPlatform platform))
        {
            if (currentPlatform == platform)
                currentPlatform = null;
        }
    }
}