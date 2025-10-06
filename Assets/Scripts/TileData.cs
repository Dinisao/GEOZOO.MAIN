using UnityEngine;

[System.Serializable]
public class TileData
{
    public int requiredTileId;   // ID da face esperada (frontId ou backId da tile)
    public int correctCell;      // �ndice da c�lula onde deve ir (ex.: 0, 1, 2...)
    public int correctRotation;  // Rota��o esperada (0, 90, 180, 270)
    public bool isFlipped;       // Estado de flip esperado (true para costas, false para frente)
    public bool ignoreFlip = false; // Para tiles sim�tricas (ignora flip)
    public bool allowDuplicates = true; // Permite m�ltiplas tiles com mesmo ID nesta posi��o?
}