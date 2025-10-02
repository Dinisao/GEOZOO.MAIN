using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int columns = 3;
    public int rows = 4;
    public float cellSize = 2f;
    public Vector2 origin = new Vector2(-2f, -3f);

    // Array para controlar quais células estão ocupadas
    private Tile[] occupiedCells;

    void Awake()
    {
        occupiedCells = new Tile[columns * rows];
        Debug.Log("✅ GridManager iniciado: " + occupiedCells.Length + " células disponíveis");
    }

    public Vector2 GetNearestCell(Vector2 position)
    {
        int col = Mathf.RoundToInt((position.x - origin.x) / cellSize);
        int row = Mathf.RoundToInt((position.y - origin.y) / cellSize);

        col = Mathf.Clamp(col, 0, columns - 1);
        row = Mathf.Clamp(row, 0, rows - 1);

        float x = origin.x + col * cellSize;
        float y = origin.y + row * cellSize;

        return new Vector2(x, y);
    }

    public int GetCellIndex(Vector2 position)
    {
        int col = Mathf.RoundToInt((position.x - origin.x) / cellSize);
        int row = Mathf.RoundToInt((position.y - origin.y) / cellSize);

        col = Mathf.Clamp(col, 0, columns - 1);
        row = Mathf.Clamp(row, 0, rows - 1);

        return row * columns + col;
    }

    // Verifica se uma célula está ocupada
    public bool IsCellOccupied(int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= occupiedCells.Length) return false;

        bool occupied = occupiedCells[cellIndex] != null;

        if (occupied)
        {
            Debug.Log("🔒 Célula " + cellIndex + " está ocupada por: " + occupiedCells[cellIndex].name);
        }
        else
        {
            Debug.Log("🔓 Célula " + cellIndex + " está livre");
        }

        return occupied;
    }

    // Ocupa uma célula
    public bool OccupyCell(int cellIndex, Tile tile)
    {
        if (cellIndex < 0 || cellIndex >= occupiedCells.Length)
        {
            Debug.LogError("❌ Índice de célula inválido: " + cellIndex);
            return false;
        }

        // Se já está ocupada por OUTRA tile, retorna falso
        if (occupiedCells[cellIndex] != null && occupiedCells[cellIndex] != tile)
        {
            Debug.Log("❌ Célula " + cellIndex + " já está ocupada por " + occupiedCells[cellIndex].name);
            return false;
        }

        occupiedCells[cellIndex] = tile;
        Debug.Log("✅ Célula " + cellIndex + " agora ocupada por " + tile.name);
        return true;
    }

    // Libera uma célula
    public void FreeCell(int cellIndex)
    {
        if (cellIndex >= 0 && cellIndex < occupiedCells.Length)
        {
            if (occupiedCells[cellIndex] != null)
            {
                Debug.Log("🔓 Liberando célula " + cellIndex + " (era " + occupiedCells[cellIndex].name + ")");
            }
            occupiedCells[cellIndex] = null;
        }
    }
}
