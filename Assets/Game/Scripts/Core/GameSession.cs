using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Player Progress")]
    [SerializeField] private bool hasSwordAddon = true;
    [SerializeField] private bool hasHammerAddon = false;
    [SerializeField] private bool hasAxeAddon = false;
    [SerializeField] private int currentWeaponIndex = 0;

    [Header("Gauntlet Energy")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;

    [Header("Player Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Spawn / Checkpoint")]
    [SerializeField] private string targetSpawnId = "";
    [SerializeField] private CheckpointSaveData checkpointSaveData;

    private List<string> eventosConcluidos = new List<string>();

    // Inimigos mortos na sessão atual (em memória).
    private List<string> inimigosMortosSessao = new List<string>();

    // Foto dos inimigos mortos no último checkpoint FÍSICO (a que voltamos ao morrer).
    private List<string> inimigosMortosNoCheckpoint = new List<string>();

    public float MaxEnergy => maxEnergy;
    public float CurrentEnergy => currentEnergy;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public int CurrentWeaponIndex
    {
        get => currentWeaponIndex;
        set => currentWeaponIndex = value;
    }

    public string TargetSpawnId => targetSpawnId;
    public CheckpointSaveData CheckpointData => checkpointSaveData;

    public bool HasCheckpointSaved =>
        checkpointSaveData != null &&
        !string.IsNullOrEmpty(checkpointSaveData.sceneName) &&
        !string.IsNullOrEmpty(checkpointSaveData.checkpointId);

    public bool HasSwordAddon => hasSwordAddon;
    public bool HasHammerAddon => hasHammerAddon;
    public bool HasAxeAddon => hasAxeAddon;

    public bool UsarPosicaoLivre => checkpointSaveData != null && checkpointSaveData.usarPosicaoLivre;
    public Vector3 PosicaoLivre => checkpointSaveData != null
        ? new Vector3(checkpointSaveData.posX, checkpointSaveData.posY, checkpointSaveData.posZ)
        : Vector3.zero;
    public float RotacaoLivreY => checkpointSaveData != null ? checkpointSaveData.rotY : 0f;

    private const string SaveKey = "CheckpointSave";
    private const string HasSaveKey = "TemSave";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void EnsureExists()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("GameSession");
        go.AddComponent<GameSession>();
    }

    public void SetTargetSpawn(string spawnId) => targetSpawnId = spawnId;
    public void ClearTargetSpawn() => targetSpawnId = "";

    public void RestoreFullEnergy() => currentEnergy = maxEnergy;
    public void ConsumeEnergy(float amount) => currentEnergy = Mathf.Max(0f, currentEnergy - amount);
    public bool HasEnoughEnergy(float amount) => currentEnergy >= amount;
    public void SetEnergy(float value) => currentEnergy = Mathf.Clamp(value, 0f, maxEnergy);

    public void SetHealth(float value) => currentHealth = Mathf.Clamp(value, 0f, maxHealth);
    public void RestoreFullHealth() => currentHealth = maxHealth;

    // ---------- Eventos de mundo permanentes ----------
    public void MarcarEventoConcluido(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!eventosConcluidos.Contains(id))
            eventosConcluidos.Add(id);
    }

    public bool EventoJaConcluido(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return eventosConcluidos.Contains(id);
    }

    // ---------- Inimigos mortos ----------
    public void MarcarInimigoMorto(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!inimigosMortosSessao.Contains(id))
            inimigosMortosSessao.Add(id);
    }

    public bool InimigoEstaMorto(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return inimigosMortosSessao.Contains(id);
    }

    public void ResetForNewGame()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.Save();

        checkpointSaveData = null;
        eventosConcluidos = new List<string>();
        inimigosMortosSessao = new List<string>();
        inimigosMortosNoCheckpoint = new List<string>();
        targetSpawnId = "";

        hasSwordAddon = true;
        hasHammerAddon = false;
        hasAxeAddon = false;
        currentWeaponIndex = 0;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        Debug.Log("Save limpo. Novo jogo iniciado.");
    }

    public bool HasWeaponAddon(int weaponIndex)
    {
        return weaponIndex switch
        {
            0 => hasSwordAddon,
            1 => hasHammerAddon,
            2 => hasAxeAddon,
            _ => false
        };
    }

    public void UnlockWeaponAddon(int weaponIndex)
    {
        switch (weaponIndex)
        {
            case 0: hasSwordAddon = true; break;
            case 1: hasHammerAddon = true; break;
            case 2: hasAxeAddon = true; break;
        }
    }

    public void SaveCheckpoint(string checkpointId)
    {
        SaveCheckpointInterno(checkpointId, false, Vector3.zero, 0f);
    }

    public void SaveCheckpointComPosicao(string checkpointId, Vector3 pos, float rotY)
    {
        SaveCheckpointInterno(checkpointId, true, pos, rotY);
    }

    private void SaveCheckpointInterno(string checkpointId, bool usarPosLivre, Vector3 pos, float rotY)
    {
        // Checkpoint FÍSICO: atualiza a foto a que voltamos ao morrer.
        if (!usarPosLivre)
        {
            inimigosMortosNoCheckpoint = new List<string>(inimigosMortosSessao);
        }

        checkpointSaveData = new CheckpointSaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            checkpointId = checkpointId,
            playerHealth = currentHealth,
            playerEnergy = currentEnergy,
            hasSwordAddon = hasSwordAddon,
            hasHammerAddon = hasHammerAddon,
            hasAxeAddon = hasAxeAddon,
            currentWeaponIndex = currentWeaponIndex,
            eventosConcluidos = new List<string>(eventosConcluidos),
            // Para o LOAD: sessão completa (tudo o que mataste até agora).
            inimigosMortos = new List<string>(inimigosMortosSessao),
            // Para o respawn por MORTE: foto do checkpoint físico.
            inimigosMortosCheckpoint = new List<string>(inimigosMortosNoCheckpoint),
            usarPosicaoLivre = usarPosLivre,
            posX = pos.x,
            posY = pos.y,
            posZ = pos.z,
            rotY = rotY
        };
    }

    public void LimparPosicaoLivre()
    {
        if (checkpointSaveData != null)
        {
            checkpointSaveData.usarPosicaoLivre = false;
        }
    }

    // Carrega os campos comuns (vida, armas, eventos) do checkpointSaveData.
    private void CarregarCamposComuns()
    {
        hasSwordAddon = checkpointSaveData.hasSwordAddon;
        hasHammerAddon = checkpointSaveData.hasHammerAddon;
        hasAxeAddon = checkpointSaveData.hasAxeAddon;

        currentWeaponIndex = checkpointSaveData.currentWeaponIndex;
        currentHealth = checkpointSaveData.playerHealth;
        currentEnergy = checkpointSaveData.playerEnergy;

        targetSpawnId = checkpointSaveData.checkpointId;

        eventosConcluidos = checkpointSaveData.eventosConcluidos != null
            ? new List<string>(checkpointSaveData.eventosConcluidos)
            : new List<string>();

        inimigosMortosNoCheckpoint = checkpointSaveData.inimigosMortosCheckpoint != null
            ? new List<string>(checkpointSaveData.inimigosMortosCheckpoint)
            : new List<string>();
    }

    // Usado ao dar LOAD pelo menu principal: restaura a sessão completa.
    // Os inimigos que mataste antes de guardar continuam mortos.
    public void RestoreFromDisk()
    {
        if (!HasCheckpointSaved) return;

        CarregarCamposComuns();

        inimigosMortosSessao = checkpointSaveData.inimigosMortos != null
            ? new List<string>(checkpointSaveData.inimigosMortos)
            : new List<string>();
    }

    // Usado ao MORRER: volta à foto do checkpoint físico.
    // Inimigos mortos depois do checkpoint ressuscitam.
    public void RestoreFromCheckpointDeath()
    {
        if (!HasCheckpointSaved)
        {
            Debug.LogWarning("Nenhum checkpoint salvo para restaurar.");
            return;
        }

        CarregarCamposComuns();

        inimigosMortosSessao = new List<string>(inimigosMortosNoCheckpoint);
    }

    // Mantido por compatibilidade — o LoadFromDisk usa o caminho de Load.
    public void RestoreCheckpointState()
    {
        RestoreFromDisk();
    }

    public string GetCheckpointSceneName()
    {
        if (!HasCheckpointSaved) return "";
        return checkpointSaveData.sceneName;
    }

    public void SaveToDisk()
    {
        if (!HasCheckpointSaved)
        {
            Debug.LogWarning("Nada para salvar em disco (sem checkpoint).");
            return;
        }

        string json = JsonUtility.ToJson(checkpointSaveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Jogo guardado em disco.");
    }

    public bool LoadFromDisk()
    {
        if (PlayerPrefs.GetInt(HasSaveKey, 0) != 1) return false;

        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json)) return false;

        checkpointSaveData = JsonUtility.FromJson<CheckpointSaveData>(json);
        RestoreFromDisk();
        return true;
    }
}
