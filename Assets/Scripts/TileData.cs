using UnityEngine;

[System.Serializable]
public class TileData
{
    public int correctCell;     // �ndice da c�lula (0-11)
    public int correctRotation; // rota��o (0, 90, 180, 270)
    public bool isFlipped;      // false = frente, true = verso
}
