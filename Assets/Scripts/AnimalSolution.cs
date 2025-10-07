using UnityEngine;
using System.Collections.Generic;

public class AnimalSolution : MonoBehaviour
{
    private static AnimalSolution _instance; // Singleton para acesso global
    public static AnimalSolution Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<AnimalSolution>();
            return _instance;
        }
    }

    [Header("Configurações do Puzzle")]
    public TileData[] expectedTiles; // Array de tiles esperadas (configure no Inspector!)
    [Header("Tracking de IDs Usados (Auto-gerenciado)")]
    public List<int> usedIds = new List<int>(); // IDs já usados (para evitar duplicatas)

    private GridManager gridManager;

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

        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("❌ GridManager não encontrado para AnimalSolution!");
        }

        // Inicializa usedIds vazia
        usedIds.Clear();
        Debug.Log("✅ AnimalSolution inicializado com " + (expectedTiles != null ? expectedTiles.Length : 0) + " tiles esperadas. UsedIds vazia.");

        // **DEBUG SCORE: Log das expectedTiles para verificação**
        if (expectedTiles != null && expectedTiles.Length > 0)
        {
            for (int i = 0; i < expectedTiles.Length; i++)
            {
                TileData td = expectedTiles[i];
                Debug.Log("🔍 [DEBUG SCORE] ExpectedTile[" + i + "]: ID=" + td.requiredTileId + ", Cell=" + td.correctCell +
                          ", Flip=" + td.isFlipped + ", Rot=" + td.correctRotation + ", IgnoreFlip=" + td.ignoreFlip +
                          ", AllowDup=" + td.allowDuplicates);
            }
        }
        else
        {
            Debug.LogError("❌ [DEBUG SCORE] expectedTiles é NULL ou vazio! Configure no Inspector para ativar score.");
        }
    }

    // Método chamado pelo Tile.MoveTile(): Verifica se esta tile está correta na posição atual
    public bool IsCorrectPlacement(Tile tile)
    {
        if (tile == null || expectedTiles == null || expectedTiles.Length == 0)
        {
            Debug.LogError("❌ [DEBUG SCORE] Tile NULL ou expectedTiles vazio/inválido em IsCorrectPlacement!");
            return false;
        }

        // **DEBUG SCORE: Log entrada**
        Debug.Log("🔍 [DEBUG SCORE] IsCorrectPlacement chamado para Tile " + tile.tileId + " (Instância #" + tile.GetInstanceId() +
                  ", ID atual=" + tile.GetCurrentId() + ", Cell atual=" + tile.currentCell + ", Flip atual=" + tile.isFlipped +
                  ", Rot atual=" + tile.currentRotation + ")");

        // Encontra o TileData esperado para a célula atual da tile
        TileData matchingExpected = null;
        foreach (TileData expected in expectedTiles)
        {
            if (expected.correctCell == tile.currentCell)
            {
                matchingExpected = expected;
                break;
            }
        }

        if (matchingExpected == null)
        {
            Debug.LogError("❌ [DEBUG SCORE] Nenhuma TileData encontrada para célula " + tile.currentCell + "! ExpectedTiles tem " + expectedTiles.Length + " itens. Células esperadas: " +
                           string.Join(", ", System.Array.ConvertAll(expectedTiles, e => e.correctCell.ToString())));
            return false;
        }

        // **DEBUG SCORE: Log matching**
        Debug.Log("🔍 [DEBUG SCORE] Matching encontrado para célula " + tile.currentCell + ": Expected ID=" + matchingExpected.requiredTileId +
                  ", Flip=" + matchingExpected.isFlipped + ", Rot=" + matchingExpected.correctRotation + ", IgnoreFlip=" + matchingExpected.ignoreFlip +
                  ", AllowDup=" + matchingExpected.allowDuplicates);

        // Usa IsCorrect da tile para validar (ID, rotação, flip, duplicatas)
        bool isCorrect = tile.IsCorrect(matchingExpected);

        // **DEBUG SCORE: Log saída**
        Debug.Log("🔍 [DEBUG SCORE] IsCorrect da tile retornou: " + isCorrect + " para Tile " + tile.tileId + " (Instância #" + tile.GetInstanceId() + ")");

        // Se correto e trackUsedId, marca ID como usado (evita duplicatas)
        if (isCorrect && tile.trackUsedId)
        {
            if (!usedIds.Contains(tile.GetCurrentId()))
            {
                usedIds.Add(tile.GetCurrentId());
                Debug.Log("🔒 [DEBUG SCORE] ID " + tile.GetCurrentId() + " marcado como usado para Tile " + tile.tileId + " (Instância #" + tile.GetInstanceId() + "). UsedIds agora: " + usedIds.Count + " itens.");
            }
            else
            {
                Debug.LogWarning("⚠️ [DEBUG SCORE] ID " + tile.GetCurrentId() + " já estava usado - não adicionado novamente.");
            }
        }

        return isCorrect;
    }

    // Método para checar se o puzzle inteiro está completo (chamado por GameManager)
    public bool IsPuzzleComplete()
    {
        if (expectedTiles == null || expectedTiles.Length == 0)
        {
            Debug.LogWarning("⚠️ [DEBUG SCORE] expectedTiles vazio. Puzzle não pode ser completo.");
            return false;
        }

        bool allCorrect = true;
        int checkedTiles = 0;

        Debug.Log("🔍 [DEBUG SCORE] Verificando puzzle completo: " + expectedTiles.Length + " tiles esperadas.");

        foreach (TileData expected in expectedTiles)
        {
            Tile tileInCell = gridManager.GetTileInCell(expected.correctCell);
            if (tileInCell == null)
            {
                Debug.Log("❌ [DEBUG SCORE] Célula " + expected.correctCell + " vazia. Puzzle incompleto.");
                allCorrect = false;
                break;
            }

            // Verifica se a tile na célula é correta para esta expectativa
            bool cellCorrect = tileInCell.IsCorrect(expected);
            if (!cellCorrect)
            {
                Debug.Log("❌ [DEBUG SCORE] Tile em célula " + expected.correctCell + " (ID=" + tileInCell.GetCurrentId() +
                          ", Flip=" + tileInCell.isFlipped + ") não é correta para expectativa ID=" + expected.requiredTileId +
                          ", Cell=" + expected.correctCell + ", Flip=" + expected.isFlipped);
                allCorrect = false;
                break;
            }

            checkedTiles++;
            Debug.Log("✅ [DEBUG SCORE] Célula " + expected.correctCell + " verificada: Correta (ID=" + expected.requiredTileId + ", Tile ID atual=" + tileInCell.GetCurrentId() + ")");
        }

        if (allCorrect)
        {
            Debug.Log("🎉 [DEBUG SCORE] Todas as " + checkedTiles + " tiles verificadas: Puzzle COMPLETO!");
        }
        else
        {
            Debug.Log("❌ [DEBUG SCORE] Puzzle incompleto: " + checkedTiles + "/" + expectedTiles.Length + " tiles OK.");
        }

        return allCorrect;
    }

    // Método para checar se um ID já foi usado (para duplicatas)
    public bool IsIdUsed(int id)
    {
        bool used = usedIds.Contains(id);
        Debug.Log("🔍 [DEBUG SCORE] Verificando ID " + id + ": Já usado? " + used + " (UsedIds: " + usedIds.Count + " itens).");
        return used;
    }

    // Método para resetar IDs usados (chamado em LoadLevel ou Reset)
    public void ResetUsedIds()
    {
        usedIds.Clear();
        Debug.Log("🔄 [DEBUG SCORE] IDs usados resetados (lista vazia agora).");
    }

    void OnDestroy()
    {
        if (gridManager != null)
        {
            // Opcional: Libera grid ao destruir
            gridManager.ResetAllCells();
            Debug.Log("🧹 [DEBUG SCORE] Grid resetado ao destruir AnimalSolution.");
        }
    }
}