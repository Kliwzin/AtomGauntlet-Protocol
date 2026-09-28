using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform loweredPoint;
    [SerializeField] private Transform raisedPoint;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Initial State")]
    [SerializeField] private bool startRaised = false;

    private Vector3 targetPosition;
    private bool isRaised = false;
    private Vector3 lastPosition;
    private Vector3 platformDelta;

    public Vector3 PlatformDelta => platformDelta;

    private void Start()
    {
        isRaised = startRaised;

        transform.position = isRaised ? raisedPoint.position : loweredPoint.position;
        targetPosition = transform.position;
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        platformDelta = newPosition - transform.position;
        transform.position = newPosition;
        lastPosition = transform.position;
    }

    public void Toggle()
    {
        isRaised = !isRaised;
        targetPosition = isRaised ? raisedPoint.position : loweredPoint.position;
    }

    public bool IsRaised()
    {
        return isRaised;
    }

    public void ForceRaise()
    {
        isRaised = true;
        targetPosition = raisedPoint.position;
    }

    public void ForceLower()
    {
        isRaised = false;
        targetPosition = loweredPoint.position;
    }
}