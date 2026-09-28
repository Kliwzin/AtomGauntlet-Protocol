using UnityEngine;
using UnityEngine.UI;

public class MenuImageAnimator : MonoBehaviour
{
    public Image imagemDoPanel;
    public Sprite[] frames;
    public float framesPorSegundo = 4f;

    private int frameAtual = 0;
    private float timer = 0f;

    void Start()
    {
        if (imagemDoPanel != null && frames.Length > 0)
        {
            imagemDoPanel.sprite = frames[0];
        }
    }

    void Update()
    {
        if (imagemDoPanel == null || frames.Length == 0)
            return;

        timer += Time.deltaTime;

        if (timer >= 1f / framesPorSegundo)
        {
            timer = 0f;

            frameAtual++;

            if (frameAtual >= frames.Length)
                frameAtual = 0;

            imagemDoPanel.sprite = frames[frameAtual];
        }
    }
}
