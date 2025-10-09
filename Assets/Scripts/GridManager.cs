using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager _instance; // Singleton para acesso global
    public static GridManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GridManager>();
            return _instance;
        }
    }

    [Header("Configurações do Grid")]
    public int columns = 2;
    public int rows = 2;
    public float cellSize = 2f;
    public Vector2 origin = new Vector2(-2f, -3f);

    // Array para controlar quais células estão ocupadas
    private Tile[] occupiedCells;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: persiste entre cenas
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Evita múltiplas instâncias
        }

        // Validação de dimensões para evitar grids inválidos
        if (columns <= 0 || rows <= 0)
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            Debug.LogWarning("⚠️ Dimensões do grid ajustadas para valores mínimos: " + columns + "x" + rows);
        }

        occupiedCells = new Tile[columns * rows];
        Debug.Log("✅ GridManager iniciado: " + occupiedCells.Length + " células disponíveis (" + columns + "x" + rows + ")");
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
        if (cellIndex < 0 || cellIndex >= occupiedCells.Length)
        {
            Debug.LogError("❌ Índice de célula inválido em IsCellOccupied: " + cellIndex);
            return false; // Tratar como livre para evitar crashes, mas logar erro
        }

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
            Debug.LogError("❌ Índice de célula inválido em OccupyCell: " + cellIndex);
            return false;
        }

        // Se já está ocupada por OUTRA tile, retorna falso (reforçado)
        if (occupiedCells[cellIndex] != null && occupiedCells[cellIndex] != tile)
        {
            Debug.Log("❌ Célula " + cellIndex + " já está ocupada por " + occupiedCells[cellIndex].name + ". Não pode colocar " + tile.name);
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
        else
        {
            Debug.LogWarning("⚠️ Tentativa de liberar célula inválida: " + cellIndex);
        }
    }

    // Método: Retorna a Tile ocupando uma célula específica (usado por AnimalSolution)
    public Tile GetTileInCell(int cellIndex)
    {
        if (cellIndex < 0 || cellIndex >= occupiedCells.Length)
        {
            Debug.LogWarning("⚠️ Índice de célula inválido em GetTileInCell: " + cellIndex);
            return null;
        }

        Tile tile = occupiedCells[cellIndex];
        if (tile != null)
        {
            Debug.Log("🔍 Tile em célula " + cellIndex + ": " + tile.name + " (ID: " + tile.GetCurrentId() + ")");
        }
        else
        {
            Debug.Log("🔍 Célula " + cellIndex + " vazia.");
        }

        return tile;
    }

    // Opcional: Método para resetar todo o grid (libera todas as células)
    public void ResetAllCells()
    {
        for (int i = 0; i < occupiedCells.Length; i++)
        {
            FreeCell(i);
        }
        Debug.Log("🧹 Todo o grid resetado: " + occupiedCells.Length + " células liberadas.");
    }

    // Opcional: Getter para total de células ocupadas (para debug)
    public int GetOccupiedCount()
    {
        int count = 0;
        foreach (var tile in occupiedCells)
        {
            if (tile != null) count++;
        }
        return count;
    }
}