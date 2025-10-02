using UnityEngine;

public class Tile : MonoBehaviour
{
    private GridManager gridManager;
    public int currentRotation; // 0, 90, 180, 270
    public int currentCell = -1; // índice da célula (-1 = fora da grid)
    public bool isFlipped = false; // false = frente, true = verso

    [Header("Sprites da Tile")]
    public Sprite frontSprite; // Imagem da frente
    public Sprite backSprite;  // Imagem do verso

    private SpriteRenderer spriteRenderer;
    private Vector3 previousPosition;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        previousPosition = transform.position;

        // Define a imagem inicial (frente)
        if (spriteRenderer != null && frontSprite != null)
        {
            spriteRenderer.sprite = frontSprite;
        }
        else if (spriteRenderer == null)
        {
            Debug.LogError("❌ " + gameObject.name + " não tem SpriteRenderer!");
        }
    }

    public void RotateTile()
    {
        transform.Rotate(0, 0, 90);
        currentRotation = (currentRotation + 90) % 360;
        Debug.Log("🔄 Rotação: " + currentRotation + "°");
    }

    public void FlipTile()
    {
        isFlipped = !isFlipped;

        // Troca a imagem entre frente e verso
        if (spriteRenderer != null)
        {
            if (isFlipped && backSprite != null)
            {
                spriteRenderer.sprite = backSprite;
                Debug.Log("🔄 " + gameObject.name + " virada (verso)");
            }
            else if (!isFlipped && frontSprite != null)
            {
                spriteRenderer.sprite = frontSprite;
                Debug.Log("🔄 " + gameObject.name + " normal (frente)");
            }
        }
    }

    public bool MoveTile(Vector2 position)
    {
        if (gridManager == null) return false;

        Vector2 snapPos = gridManager.GetNearestCell(position);
        int newCell = gridManager.GetCellIndex(snapPos);

        // Se a nova célula é a mesma que a atual, não faz nada
        if (newCell == currentCell)
        {
            transform.position = snapPos;
            return true;
        }

        // Verifica se a nova célula está ocupada por OUTRA peça
        if (gridManager.IsCellOccupied(newCell))
        {
            Debug.Log("❌ Não pode colocar aqui! Célula " + newCell + " ocupada.");
            // Volta para a posição anterior
            transform.position = previousPosition;
            return false;
        }

        // PRIMEIRO: Libera a célula antiga (se houver)
        if (currentCell >= 0)
        {
            gridManager.FreeCell(currentCell);
            Debug.Log("🔓 Célula " + currentCell + " liberada");
        }

        // DEPOIS: Ocupa a nova célula
        if (gridManager.OccupyCell(newCell, this))
        {
            transform.position = snapPos;
            previousPosition = snapPos;
            currentCell = newCell;
            Debug.Log("✅ Tile colocada na célula " + currentCell);
            return true;
        }

        return false;
    }

    public bool IsCorrect(TileData expected)
    {
        return currentCell == expected.correctCell &&
               currentRotation == expected.correctRotation &&
               isFlipped == expected.isFlipped;
    }

    // Liberar célula quando a peça é destruída
    void OnDestroy()
    {
        if (gridManager != null && currentCell >= 0)
        {
            gridManager.FreeCell(currentCell);
        }
    }
}