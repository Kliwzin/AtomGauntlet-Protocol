using UnityEngine;

public class OrionLineAttack : MonoBehaviour
{
    public enum PreparedLineAttack
    {
        None,
        Cardinal,
        Diagonal,
        Universal
    }

    [SerializeField] private OrionCardinalWarning warningPrefab;
    [SerializeField] private OrionAttackManager attackManager;

    [Header("Damage")]
    [SerializeField] private float baseDamage = 12f;
    [SerializeField] private bool allowMultipleHitsPerAttack = false;

    [Header("Universal (todos os lados)")]
    [SerializeField] private float universalProjectileScale = 0.5f;
    [SerializeField] private float universalProjectileHeight = 0.4f;

    private bool playerAlreadyHitThisAttack = false;
    private PreparedLineAttack preparedAttack = PreparedLineAttack.None;
    private PreparedLineAttack firedAttack = PreparedLineAttack.None;

    public bool IsUniversalActive => firedAttack == PreparedLineAttack.Universal;
    public float UniversalProjectileScale => universalProjectileScale;
    public float UniversalProjectileHeight => universalProjectileHeight;

    public void PrepareCardinal()
    {
        BeginAttack();
        preparedAttack = PreparedLineAttack.Cardinal;
        Debug.Log("Orion preparou ataque cardinal.");
    }

    public void PrepareDiagonal()
    {
        BeginAttack();
        preparedAttack = PreparedLineAttack.Diagonal;
        Debug.Log("Orion preparou ataque diagonal.");
    }

    public void PrepareUniversal()
    {
        BeginAttack();
        preparedAttack = PreparedLineAttack.Universal;
        Debug.Log("Orion preparou ataque universal.");
    }

    public void FirePreparedAttack()
    {
        firedAttack = preparedAttack;

        switch (preparedAttack)
        {
            case PreparedLineAttack.Cardinal:
                SpawnCardinalWarnings();
                break;

            case PreparedLineAttack.Diagonal:
                SpawnDiagonalWarnings();
                break;

            case PreparedLineAttack.Universal:
                SpawnCardinalWarnings();
                SpawnDiagonalWarnings();
                break;
        }

        preparedAttack = PreparedLineAttack.None;
    }

    private void BeginAttack()
    {
        playerAlreadyHitThisAttack = false;
    }

    private void SpawnCardinalWarnings()
    {
        SpawnWarning(Vector3.forward);
        SpawnWarning(Vector3.back);
        SpawnWarning(Vector3.left);
        SpawnWarning(Vector3.right);
    }

    private void SpawnDiagonalWarnings()
    {
        SpawnWarning((Vector3.forward + Vector3.right).normalized);
        SpawnWarning((Vector3.forward + Vector3.left).normalized);
        SpawnWarning((Vector3.back + Vector3.right).normalized);
        SpawnWarning((Vector3.back + Vector3.left).normalized);
    }

    private void SpawnWarning(Vector3 direction)
    {
        if (warningPrefab == null) return;

        float finalDamage = attackManager != null
            ? attackManager.GetAttackDamage(baseDamage)
            : baseDamage;

        OrionCardinalWarning warning = Instantiate(
            warningPrefab,
            transform.position,
            Quaternion.identity
        );

        warning.Initialize(transform, direction, finalDamage, this);
    }

    public bool CanThisAttackDamagePlayer()
    {
        if (allowMultipleHitsPerAttack)
            return true;

        return !playerAlreadyHitThisAttack;
    }

    public void RegisterPlayerHit()
    {
        if (!allowMultipleHitsPerAttack)
            playerAlreadyHitThisAttack = true;
    }
}