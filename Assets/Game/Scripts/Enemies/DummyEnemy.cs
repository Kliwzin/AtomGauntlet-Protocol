using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    [SerializeField] private float health = 30f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log(name + " levou " + damage + " de dano. Vida restante: " + health);

        if (health <= 0f)
        {
            Debug.Log(name + " morreu.");
            Destroy(gameObject);
        }
    }
}
