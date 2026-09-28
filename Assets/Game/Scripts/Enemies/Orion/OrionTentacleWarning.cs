using UnityEngine;

public class OrionTentacleWarning : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float trackingTime = 2f;
    [SerializeField] private float lockDelay = 0.4f;
    [SerializeField] private float tentacleLifetime = 1f;

    [Header("Position")]
    [SerializeField] private float groundY = 0.03f;
    [SerializeField] private float tentacleSpawnY = 0f;

    [Header("Movement")]
    [SerializeField] private float followSpeed = 8f;

    [Header("Damage")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float radius = 1.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual")]
    [SerializeField] private GameObject tentacleVisual;

    private Transform player;
    private OrionAnimationController animationController;

    private bool isTracking = false;
    private bool locked = false;
    private float trackingTimer = 0f;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        animationController = FindFirstObjectByType<OrionAnimationController>();

        SnapToGround();
    }

    private void Update()
    {
        if (!isTracking || locked) return;

        trackingTimer += Time.deltaTime;
        FollowPlayerOnGround();

        if (trackingTimer >= trackingTime)
            LockAndStrikeSoon();
    }

    public void BeginTracking()
    {
        isTracking = true;
        locked = false;
        trackingTimer = 0f;
    }

    public void LockAndStrikeSoon()
    {
        if (locked) return;

        locked = true;
        isTracking = false;

        SnapToGround();

        if (animationController != null)
            animationController.PlayTentacleSlam();

        Invoke(nameof(Strike), lockDelay);

        Debug.Log("Mira do tentáculo travou.");
    }

    private void FollowPlayerOnGround()
    {
        if (player == null) return;

        Vector3 targetPos = player.position;
        targetPos.y = groundY;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            followSpeed * Time.deltaTime
        );
    }

    private void SnapToGround()
    {
        Vector3 pos = transform.position;
        pos.y = groundY;
        transform.position = pos;
    }

    private void Strike()
    {
        if (tentacleVisual != null)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.y = tentacleSpawnY;

            GameObject tentacle = Instantiate(
                tentacleVisual,
                spawnPos,
                Quaternion.identity
            );

            Destroy(tentacle, tentacleLifetime);
        }

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out IDamageable damageable))
                damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}