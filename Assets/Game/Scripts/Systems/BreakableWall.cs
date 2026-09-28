using UnityEngine;

public class BreakableWall : MonoBehaviour, IWeaponResistanceHint
{
    [Header("Break Settings")]
    [SerializeField] private int hitsToBreak = 3;
    [SerializeField] private int requiredWeaponIndex = 1;
    [SerializeField] private GameObject destroyedVersion;

    [Header("Save / Estado do Mundo")]
    [SerializeField] private string eventoId = "";

    private int currentHits;

    private void Awake()
    {
        currentHits = hitsToBreak;

        // Se a grade já tinha sido partida num save anterior, não aparece (já nasce partida).
        if (!string.IsNullOrEmpty(eventoId) &&
            GameSession.Instance != null &&
            GameSession.Instance.EventoJaConcluido(eventoId))
        {
            if (destroyedVersion != null)
                Instantiate(destroyedVersion, transform.position, transform.rotation);

            Destroy(gameObject);
        }
    }

    public void TakeWeaponHit(int weaponIndex)
    {
        if (weaponIndex != requiredWeaponIndex)
        {
            Debug.Log("O impacto não é forte o suficiente.");
            return;
        }

        currentHits--;

        Debug.Log($"Parede atingida. Faltam {currentHits} hits.");

        if (currentHits <= 0)
            Break();
    }

    public bool ShouldShowResistanceIcon(int weaponIndex)
    {
        return weaponIndex != requiredWeaponIndex;
    }

    private void Break()
    {
        if (!string.IsNullOrEmpty(eventoId) && GameSession.Instance != null)
            GameSession.Instance.MarcarEventoConcluido(eventoId);

        if (destroyedVersion != null)
            Instantiate(destroyedVersion, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
