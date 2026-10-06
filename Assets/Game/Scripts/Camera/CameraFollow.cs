using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody targetRb;

    [Header("═══ POSIÇÃO BASE ═══")]
    [SerializeField] private Vector3 basePosition = new Vector3(0f, 8f, -6f);
    [SerializeField] private float rotationX = 15f;

    [Header("═══ SEGUIMENTO HORIZONTAL ═══")]
    [SerializeField] private float horizontalDeadZone = 3f;
    [SerializeField] private float horizontalMaxDistance = 0.5f;
    [SerializeField] private float horizontalSmoothSpeed = 0.03f;

    [Header("═══ SEGUIMENTO VERTICAL ═══")]
    [SerializeField] private float verticalDeadZone = 2f;
    [SerializeField] private float verticalMaxDistance = 0.3f;
    [SerializeField] private float verticalSmoothSpeed = 0.03f;

    [Header("═══ ZOOM DINÂMICO ═══")]
    [SerializeField] private bool enableDynamicZoom = false;
    [SerializeField] private float minZoom = -4f;
    [SerializeField] private float maxZoom = -10f;
    [SerializeField] private float zoomDistance = 6f;
    [SerializeField] private float zoomSmoothSpeed = 0.2f;

    [Header("═══ JUMP EFFECT ═══")]
    [SerializeField] private bool enableJumpEffect = true;
    [SerializeField] private float jumpCameraLift = 0.5f;

    [Header("═══ HIT EFFECT ═══")]
    [SerializeField] private bool enableHitShake = true;
    [SerializeField] private float shakeAmount = 0.15f;
    [SerializeField] private float shakeDuration = 0.1f;

    [Header("═══ LIMITES ═══")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 boundsX = new Vector2(-10f, 10f); // mínimo, máximo
    [SerializeField] private Vector2 boundsZ = new Vector2(-10f, 10f); // mínimo, máximo

    private Vector3 velocityXZ = Vector3.zero;
    private Vector3 velocityY = Vector3.zero;
    private float currentZoom = 0f;
    private float zoomVelocity = 0f;
    private float shakeTimer = 0f;
    private Vector3 shakeOffset = Vector3.zero;
    private float jumpOffsetY = 0f;

    private bool isFrozen = false;
    private Vector3 followPosition;

    private void Start()
    {
        if (target == null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                target = player.transform;
                targetRb = player.GetComponent<Rigidbody>();
            }
        }

        SnapToTarget();
    }

    // Posiciona a câmara instantaneamente sobre o jogador, sem suavização
    private void SnapToTarget()
    {
        if (target == null) return;

        Vector3 snapPos = new Vector3(
            target.position.x + basePosition.x,
            target.position.y + basePosition.y,
            target.position.z + basePosition.z
        );

        snapPos = ClampToBounds(snapPos);
        followPosition = snapPos;
        transform.position = snapPos;
        transform.rotation = Quaternion.Euler(rotationX, 0f, 0f);

        // zera as velocidades do SmoothDamp para não "arrastar" do sítio antigo
        velocityXZ = Vector3.zero;
        velocityY = Vector3.zero;
        currentZoom = 0f;
        zoomVelocity = 0f;
    }

    private void LateUpdate()
    {
        if (isFrozen) return;

        if (target == null)
        {
            // tenta encontrar o jogador caso ainda não existisse no Start
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
            if (player != null)
            {
                target = player.transform;
                targetRb = player.GetComponent<Rigidbody>();
                SnapToTarget();
            }
            return;
        }

        UpdateShake();

        float targetX = target.position.x + basePosition.x;
        float targetY = target.position.y + basePosition.y;
        float desiredZ = target.position.z + basePosition.z;

        if (enableDynamicZoom)
        {
            float distanceFromCenter = Mathf.Abs(target.position.x - transform.position.x);
            float targetZoom = Mathf.Lerp(
                minZoom,
                maxZoom,
                Mathf.Clamp01(distanceFromCenter / zoomDistance)
            );

            currentZoom = Mathf.SmoothDamp(
                currentZoom,
                targetZoom,
                ref zoomVelocity,
                zoomSmoothSpeed
            );
            desiredZ = target.position.z + basePosition.z + currentZoom;
        }

        Vector3 desiredPos = new Vector3(targetX, targetY + jumpOffsetY, desiredZ);
        desiredPos = ClampToBounds(desiredPos);

        Vector3 smoothedPos = followPosition;
        smoothedPos.x = Mathf.SmoothDamp(followPosition.x, desiredPos.x, ref velocityXZ.x, horizontalSmoothSpeed);
        smoothedPos.z = Mathf.SmoothDamp(followPosition.z, desiredPos.z, ref velocityXZ.z, horizontalSmoothSpeed);
        smoothedPos.y = Mathf.SmoothDamp(followPosition.y, desiredPos.y, ref velocityY.y, verticalSmoothSpeed);

        followPosition = smoothedPos;

        transform.position = followPosition + shakeOffset;
        transform.rotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    private Vector3 ClampToBounds(Vector3 pos)
    {
        if (!useBounds) return pos;

        pos.x = Mathf.Clamp(pos.x, boundsX.x, boundsX.y);
        pos.z = Mathf.Clamp(pos.z, boundsZ.x, boundsZ.y);
        return pos;
    }

    private float CalculateHorizontalPosition()
    {
        float playerX = target.position.x;
        float cameraX = transform.position.x;
        float difference = playerX - cameraX;

        if (Mathf.Abs(difference) < horizontalDeadZone)
        {
            return cameraX;
        }

        if (difference > 0)
        {
            return Mathf.Min(playerX, cameraX + horizontalMaxDistance);
        }
        else
        {
            return Mathf.Max(playerX, cameraX - horizontalMaxDistance);
        }
    }

    private float CalculateVerticalPosition()
    {
        float playerY = target.position.y;
        float cameraY = transform.position.y - jumpOffsetY;
        float difference = playerY - cameraY;

        if (Mathf.Abs(difference) < verticalDeadZone)
        {
            return cameraY;
        }

        if (difference > 0)
        {
            return Mathf.Min(playerY, cameraY + verticalMaxDistance);
        }
        else
        {
            return Mathf.Max(playerY, cameraY - verticalMaxDistance);
        }
    }

    private float CalculateZPosition()
    {
        return basePosition.z + currentZoom;
    }

    private void UpdateShake()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            shakeOffset = Random.insideUnitSphere * shakeAmount;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }

    public void OnPlayerJump()
    {
        if (!enableJumpEffect || targetRb == null) return;
        StartCoroutine(JumpEffectCoroutine());
    }

    private System.Collections.IEnumerator JumpEffectCoroutine()
    {
        float timer = 0f;
        float jumpDuration = 0.3f;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            jumpOffsetY = Mathf.Sin(timer / jumpDuration * Mathf.PI) * jumpCameraLift;
            yield return null;
        }

        jumpOffsetY = 0f;
    }

    public void Shake()
    {
        if (!enableHitShake) return;
        shakeTimer = shakeDuration;
    }

    public void FreezeCamera()
    {
        isFrozen = true;
        shakeTimer = 0f;
        shakeOffset = Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.cyan;

            Vector3 center = new Vector3(
                (boundsX.x + boundsX.y) * 0.5f,
                transform.position.y,
                (boundsZ.x + boundsZ.y) * 0.5f
            );

            Vector3 size = new Vector3(boundsX.y - boundsX.x, 0.1f, boundsZ.y - boundsZ.x);
            Gizmos.DrawWireCube(center, size);
        }

        if (target == null) return;

        Vector3 playerPos = target.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(playerPos, new Vector3(horizontalDeadZone * 2, 1f, 1f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(playerPos, new Vector3(horizontalMaxDistance * 2, 1f, 1f));

        Gizmos.color = new Color(0f, 1f, 0.5f);
        Gizmos.DrawWireCube(playerPos, new Vector3(1f, verticalDeadZone * 2, 1f));

        Gizmos.color = new Color(1f, 1f, 0.5f);
        Gizmos.DrawWireCube(playerPos, new Vector3(1f, verticalMaxDistance * 2, 1f));
    }
}
