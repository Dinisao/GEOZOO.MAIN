using System.Collections.Generic;
using UnityEngine;

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

    [Header("Configuração do Puzzle")]
    public TileData[] expectedTiles; // Array de expectativas por célula - ACESSÍVEL NO INSPECTOR E GAME MANAGER

    private Dictionary<int, bool> usedIds = new Dictionary<int, bool>(); // Rastreia IDs usados para evitar duplicatas

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

        // Validação: Garante que expectedTiles tenha tamanho válido
        if (expectedTiles == null || expectedTiles.Length == 0)
        {
            Debug.LogWarning("⚠️ expectedTiles vazio em AnimalSolution. Configure no Inspector!");
        }
        else
        {
            Debug.Log("✅ AnimalSolution inicializado com " + expectedTiles.Length + " tiles esperadas.");
            foreach (var data in expectedTiles)
            {
                Debug.Log("🔍 Esperado: Célula " + data.correctCell + " com ID " + data.requiredTileId +
                          ", Rotação " + data.correctRotation + ", Flip " + data.isFlipped +
                          (data.allowDuplicates ? " (duplicatas OK)" : " (ID único)"));
            }
        }
    }

    // Verifica se a colocação da tile é correta baseada na sua currentCell
    public bool IsCorrectPlacement(Tile tile)
    {
        if (tile == null || expectedTiles == null) return false;

        // Encontra o TileData esperado para a currentCell da tile
        TileData expected = GetExpectedForCell(tile.currentCell);
        if (expected == null)
        {
            Debug.LogWarning("⚠️ Nenhuma expectativa para célula " + tile.currentCell + ". Tile " + tile.tileId + " considerada incorreta.");
            return false;
        }

        // Usa o método IsCorrect da Tile (com suporte a IDs por face, duplicatas, etc.)
        bool isCorrect = tile.IsCorrect(expected);

        // Se correto e rastreia uso, marca o ID como usado
        if (isCorrect && tile.trackUsedId && !usedIds.ContainsKey(tile.GetCurrentId()))
        {
            usedIds[tile.GetCurrentId()] = true;
            Debug.Log("✅ ID " + tile.GetCurrentId() + " marcado como usado após colocação correta.");
        }

        return isCorrect;
    }

    // Método auxiliar: Retorna o TileData esperado para uma célula específica
    public TileData GetExpectedForCell(int cellIndex)
    {
        foreach (var data in expectedTiles)
        {
            if (data.correctCell == cellIndex)
                return data;
        }
        return null; // Nenhuma expectativa para essa célula (ex.: célula vazia no puzzle)
    }

    // Checa se um ID já foi usado (para duplicatas)
    public bool IsIdUsed(int id)
    {
        return usedIds.ContainsKey(id) && usedIds[id];
    }

    // Marca um ID como usado (chamado após colocação correta)
    public void MarkIdAsUsed(int id)
    {
        if (!usedIds.ContainsKey(id))
        {
            usedIds[id] = true;
            Debug.Log("✅ ID " + id + " marcado como usado (não pode ser reutilizado em duplicatas).");
        }
    }

    // Opcional: Verifica se o puzzle inteiro está completo (agora usa GetTileInCell do GridManager)
    public bool IsPuzzleComplete()
    {
        if (expectedTiles == null || expectedTiles.Length == 0) return false;

        int correctCount = 0;
        foreach (var data in expectedTiles)
        {
            // CORRIGIDO: Usa GridManager.Instance.GetTileInCell (linha ~109)
            Tile tileInCell = GridManager.Instance.GetTileInCell(data.correctCell);
            if (tileInCell != null && tileInCell.IsCorrect(data))
                correctCount++;
        }
        bool complete = correctCount == expectedTiles.Length;
        if (complete) Debug.Log("🎉 Puzzle completo! Todas as " + expectedTiles.Length + " tiles corretas.");
        return complete;
    }

    // Opcional: Resetar tracking para novo nível
    public void ResetUsedIds()
    {
        usedIds.Clear();
        Debug.Log("🔄 IDs usados resetados para novo puzzle.");
    }

    // Opcional: Getter para expectedTiles (se precisar acessar de fora)
    public TileData[] GetExpectedTiles()
    {
        return expectedTiles;
    }
}