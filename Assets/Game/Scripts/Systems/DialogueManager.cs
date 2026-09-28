using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public enum Falante { Polo, Orion, Outro }

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string texto;
    public string nome;
    public Falante falante;
}

public class DialogueManager : MonoBehaviour
{
    [Header("Referências da UI")]
    public GameObject dialogueRoot;
    public TMP_Text speakerName;
    public TMP_Text dialogueText;

    [Header("Imagem da Caixa")]
    public Image caixaImagem;
    public Sprite caixaPolo;
    public Sprite caixaOrion;
    public Sprite caixaOutro;

    [Header("Definições")]
    public float velocidadeTexto = 0.03f;

    [Tooltip("Segundos que cada linha fica visível depois de terminar de escrever antes de fechar/avançar sozinha. 0 = não fecha por tempo.")]
    public float tempoParaFechar = 4f;

    private DialogueLine[] linhasAtuais;
    private int indiceLinha;
    private bool aEscrever;
    private bool dialogoAtivo;
    private bool estaPausado;          // novo
    private Coroutine escritaCoroutine;
    private Coroutine fecharCoroutine;

    void Awake()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }

    void Update()
    {
        if (!dialogoAtivo) return;
        if (estaPausado) return;       // novo: ignora input enquanto pausado

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (aEscrever)
                CompletarTexto();
            else
                ProximaLinha();
        }
    }

    // novo: chamado pelo PauseMenu
    public void SetPausado(bool pausado)
    {
        estaPausado = pausado;

        // Esconde o diálogo enquanto está em pausa, mostra ao retomar
        if (dialogoAtivo && dialogueRoot != null)
            dialogueRoot.SetActive(!pausado);
    }

    public void IniciarDialogo(DialogueLine[] linhas)
    {
        if (linhas == null || linhas.Length == 0) return;

        PararTudo();

        linhasAtuais = linhas;
        indiceLinha = 0;
        dialogoAtivo = true;

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        MostrarLinha();
    }

    void MostrarLinha()
    {
        DialogueLine linha = linhasAtuais[indiceLinha];

        if (speakerName != null)
            speakerName.text = linha.nome;

        if (caixaImagem != null)
        {
            switch (linha.falante)
            {
                case Falante.Polo: caixaImagem.sprite = caixaPolo; break;
                case Falante.Orion: caixaImagem.sprite = caixaOrion; break;
                default: caixaImagem.sprite = caixaOutro; break;
            }
        }

        if (escritaCoroutine != null)
            StopCoroutine(escritaCoroutine);
        escritaCoroutine = StartCoroutine(EscreverTexto(linha.texto));
    }

    IEnumerator EscreverTexto(string texto)
    {
        aEscrever = true;
        dialogueText.text = "";
        foreach (char letra in texto)
        {
            // novo: se pausar a meio da escrita, espera sem escrever
            while (estaPausado)
                yield return null;

            dialogueText.text += letra;
            yield return new WaitForSecondsRealtime(velocidadeTexto);
        }
        aEscrever = false;

        IniciarContagemFechar();
    }

    void CompletarTexto()
    {
        if (escritaCoroutine != null)
            StopCoroutine(escritaCoroutine);
        dialogueText.text = linhasAtuais[indiceLinha].texto;
        aEscrever = false;

        IniciarContagemFechar();
    }

    void IniciarContagemFechar()
    {
        if (tempoParaFechar <= 0f) return;

        if (fecharCoroutine != null)
            StopCoroutine(fecharCoroutine);
        fecharCoroutine = StartCoroutine(ContagemFechar());
    }

    IEnumerator ContagemFechar()
    {
        // novo: conta o tempo respeitando a pausa
        float tempo = 0f;
        while (tempo < tempoParaFechar)
        {
            if (!estaPausado)
                tempo += Time.unscaledDeltaTime;
            yield return null;
        }
        ProximaLinha();
    }

    void ProximaLinha()
    {
        if (fecharCoroutine != null)
            StopCoroutine(fecharCoroutine);

        indiceLinha++;
        if (indiceLinha < linhasAtuais.Length)
            MostrarLinha();
        else
            FecharDialogo();
    }

    void PararTudo()
    {
        if (escritaCoroutine != null)
            StopCoroutine(escritaCoroutine);
        if (fecharCoroutine != null)
            StopCoroutine(fecharCoroutine);
        aEscrever = false;
    }

    void FecharDialogo()
    {
        PararTudo();
        dialogoAtivo = false;
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);
    }

    public bool DialogoAtivo => dialogoAtivo;
}
