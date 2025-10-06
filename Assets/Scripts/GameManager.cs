using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }

    [Header("Configurações do Jogo")]
    public int currentLevel = 0;
    public int totalScore = 0;
    public int scorePerCorrect = 10;

    [Header("Configurações de Níveis (Opcional)")]
    public TileData[][] puzzleConfigs;

    [Header("UI")]
    public TMPro.TextMeshProUGUI scoreText;

    private int lastScore = -1;
    private AnimalSolution animalSolution;
    private GridManager gridManager;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        animalSolution = AnimalSolution.Instance;
        gridManager = GridManager.Instance;

        if (animalSolution == null || gridManager == null)
        {
            Debug.LogError("❌ AnimalSolution ou GridManager não encontrado!");
            return;
        }

        if (puzzleConfigs != null && currentLevel < puzzleConfigs.Length)
        {
            animalSolution.expectedTiles = puzzleConfigs[currentLevel];
        }

        totalScore = PlayerPrefs.GetInt("TotalScore", 0);
        UpdateScoreUI();

    }

    private void UpdateScoreUI()
    {
        if (scoreText != null && lastScore != totalScore)
        {
            scoreText.text = totalScore.ToString();
            lastScore = totalScore;
        }
    }

    public void AddScore(int points)
    {
        if (points <= 0) return;

        totalScore += points;
        UpdateScoreUI();
        PlayerPrefs.SetInt("TotalScore", totalScore);

        CheckPuzzleComplete();
    }

    public void CheckPuzzleComplete()
    {
        if (animalSolution == null) return;

        if (animalSolution.IsPuzzleComplete())
        {
            Debug.Log("🏆 VITÓRIA! Puzzle do Nível " + currentLevel + " completo. Score final: " + totalScore);

            int bonus = scorePerCorrect * animalSolution.expectedTiles.Length;
            totalScore += bonus;
            UpdateScoreUI();
            Debug.Log("🎁 Bônus de conclusão: +" + bonus);

            Invoke(nameof(LoadNextLevel), 2f);
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (puzzleConfigs == null || levelIndex >= puzzleConfigs.Length)
        {
            Debug.LogWarning("⚠️ Configuração para Nível " + levelIndex + " não encontrada!");
            return;
        }

        currentLevel = levelIndex;
        animalSolution.expectedTiles = puzzleConfigs[levelIndex];
        animalSolution.ResetUsedIds();
        ResetGrid();

        Debug.Log("🔄 Nível " + currentLevel + " carregado com " + puzzleConfigs[levelIndex].Length + " tiles esperadas.");
    }

    public void LoadNextLevel()
    {
        LoadLevel(currentLevel + 1);
    }

    public void ResetGame()
    {
        totalScore = 0;
        animalSolution.ResetUsedIds();
        ResetGrid();
        UpdateScoreUI();
        Debug.Log("🔄 Jogo resetado. Score zerado.");
    }

    private void ResetGrid()
    {
        if (gridManager != null)
        {
            int totalCells = gridManager.columns * gridManager.rows;
            for (int i = 0; i < totalCells; i++)
            {
                gridManager.FreeCell(i);
            }
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
            PlayerPrefs.SetInt("TotalScore", totalScore);
    }
}
