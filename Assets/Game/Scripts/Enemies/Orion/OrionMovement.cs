using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrionMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float keepDistance = 4f;
    [SerializeField] private float tooCloseDistance = 2f;
    [SerializeField] private float turnSpeed = 12f;

    private Transform player;
    private Rigidbody rb;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    private void Start()
    {
        FindPlayer();
    }

    public void Move()
    {
        FindPlayer();

        if (player == null)
        {
            Stop();
            return;
        }

        FacePlayer();

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= 0.01f)
        {
            Stop();
            return;
        }

        direction.Normalize();

        Vector3 velocity = Vector3.zero;

        if (distance > keepDistance)
            velocity = direction * moveSpeed;
        else if (distance < tooCloseDistance)
            velocity = -direction * moveSpeed;

        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        IsMoving = Mathf.Abs(velocity.x) > 0.01f || Mathf.Abs(velocity.z) > 0.01f;
    }

    public void Stop()
    {
        if (rb != null)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        IsMoving = false;
        FacePlayer();
    }

    private void FacePlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void FindPlayer()
    {
        if (player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
}