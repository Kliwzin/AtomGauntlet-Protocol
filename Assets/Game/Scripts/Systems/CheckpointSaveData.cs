using System.Collections.Generic;

[System.Serializable]
public class CheckpointSaveData
{
    public string sceneName;
    public string checkpointId;

    public float playerHealth;
    public float playerEnergy;

    public bool hasSwordAddon;
    public bool hasHammerAddon;
    public bool hasAxeAddon;

    public int currentWeaponIndex;

    public List<string> eventosConcluidos = new List<string>();

    public List<string> inimigosMortos = new List<string>();

    public List<string> inimigosMortosCheckpoint = new List<string>();

    public bool usarPosicaoLivre;
    public float posX;
    public float posY;
    public float posZ;
    public float rotY;
}
