using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class VictorySequence : MonoBehaviour
{
    [Header("Cena de Vitória")]
    [SerializeField] private string vitoriaSceneName = "Vitória";

    [Header("Timing")]
    [SerializeField] private float delayAntesDeMudar = 1.5f;

    [Header("Player (para bloquear input)")]
    [SerializeField] private GameObject player;

    private bool jaIniciou = false;

    public void StartVictory()
    {
        if (jaIniciou) return;
        jaIniciou = true;

        BloquearPlayer();

        Invoke(nameof(IrParaVitoria), delayAntesDeMudar);
    }

    private void BloquearPlayer()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        WeaponSystem weapon = player.GetComponent<WeaponSystem>();
        if (weapon != null) weapon.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;
    }

    private void IrParaVitoria()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(vitoriaSceneName);
    }
}
