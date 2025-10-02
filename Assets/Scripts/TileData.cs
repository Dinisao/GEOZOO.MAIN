using UnityEngine;

[System.Serializable]
public class TileData
{
    public int correctCell;     // índice da célula (0-11)
    public int correctRotation; // rotação (0, 90, 180, 270)
    public bool isFlipped;      // false = frente, true = verso
}
