using UnityEngine;
using UnityEngine.SceneManagement; // Para detectar nova cena
using TMPro; // Para TextMeshProUGUI

public class GameManager : MonoBehaviour
{
    private static GameManager _instance; // Singleton para acesso global (mas sem persistência entre cenas)
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<GameManager>();
            return _instance;
        }
    }

    [Header("Configurações do Jogo")]
    public int currentLevel = 0; // Nível atual (defina no Inspector)
    public int totalScore = 0;   // Score total do jogador (inicia em 0)
    public int scorePerCorrect = 1; // Default: 1 ponto por colocação correta

    [Header("UI (Score Display - TextMeshPro)")]
    public TextMeshProUGUI scoreTextTMP; // Arraste o GameObject do TextMeshPro aqui

    [Header("Spawn Inicial (Opcional)")]
    public GameObject tilePrefab; // Prefab da tile (arraste no Inspector para spawn automático)
    public Vector3[] initialSpawnPositions; // Posições iniciais para spawn (ex.: fora do grid)

    [Header("Configurações de Níveis (Opcional)")]
    public TileData[][] puzzleConfigs; // Array de puzzles: puzzleConfigs[0] = TileData[] para nível 1, etc.

    private AnimalSolution animalSolution;
    private GridManager gridManager;

    // FLAG DE PROTEÇÃO CONTRA RECURSÃO INFINITA
    private bool isCheckingPuzzle = false;
    string scoreDisplay;

    void Awake()
    {
        // Singleton sem persistência: Destroi se múltiplas instâncias
        if (_instance == null)
        {
            _instance = this;
            // REMOVIDO: DontDestroyOnLoad(gameObject); // Não persiste entre cenas para resetar score sempre
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // Evita múltiplas instâncias
            return;
        }
    }

    void Start()
    {
        // RESET AGRESSIVO: Força score a 0 sempre (novo jogo ou nova cena)
        totalScore = 0;
        Debug.Log("🔄 [RESET] TotalScore forçado para 0 no Start() - Novo jogo iniciado.");

        // Listener para reset ao carregar cena (ex.: se reload)
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Inicializa referências
        animalSolution = AnimalSolution.Instance;
        gridManager = GridManager.Instance;

        if (animalSolution == null)
        {
            Debug.LogError("❌ AnimalSolution não encontrado! Certifique-se de que existe na cena.");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("❌ GridManager não encontrado! Certifique-se de que existe na cena.");
            return;
        }

        // Acessa expectedTiles
        TileData[] expectedTiles = animalSolution.expectedTiles;
        if (expectedTiles != null && expectedTiles.Length > 0)
        {
            Debug.Log("✅ GameManager inicializado para Nível " + currentLevel +
                      ". Carregado " + expectedTiles.Length + " tiles esperadas do puzzle. Pontos por correta: " + scorePerCorrect);

            // Opcional: Carrega configuração específica do nível se puzzleConfigs estiver setado
            if (puzzleConfigs != null && currentLevel < puzzleConfigs.Length)
            {
                animalSolution.expectedTiles = puzzleConfigs[currentLevel];
                Debug.Log("🔄 Configuração do Nível " + currentLevel + " aplicada (" +
                          puzzleConfigs[currentLevel].Length + " tiles).");
            }

            // Verifica se puzzle já está completo no início (raro, mas útil para testes)
            if (animalSolution.IsPuzzleComplete())
            {
                Debug.Log("🎉 Puzzle já completo no início do nível!");
            }
        }
        else
        {
            Debug.LogError("❌ expectedTiles não configurado em AnimalSolution! Configure no Inspector.");
        }

        // Inicializa UI de score
        UpdateScoreUI();

        Debug.Log("💰 Score inicial: " + totalScore + " (confirmado em 0 - sem carry-over de jogo anterior)");

        // Opcional: Spawn tiles iniciais (descomente para teste)
        SpawnInitialTiles();
    }

    // Método chamado ao carregar cena (reset extra)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        totalScore = 0;
        Debug.Log("🔄 [RESET CENA] Nova cena carregada: TotalScore resetado para 0.");
        UpdateScoreUI();
    }

    // Método público: Adiciona pontos quando uma tile é colocada corretamente
    public void AddScore(int points)
    {
        if (points <= 0) return;

        totalScore += points;
        Debug.Log("💰 +" + points + " ponto(s) adicionado(s)! Total: " + totalScore + " (era " + (totalScore - points) + ")");

        // Atualiza UI imediatamente após adicionar pontos
        UpdateScoreUI();

        // Opcional: Verifica se puzzle está completo após adicionar score
        CheckPuzzleComplete();
    }

    // Método para atualizar o texto de score na UI (foco em TMP)
    private void UpdateScoreUI()
    {
        if (totalScore == 4)
        {
            scoreDisplay = "Score: " + 1;
        }
        else
        {
            scoreDisplay = "Score: " + 0;
        }
        //string scoreDisplay = "Score: " + 1; // Formato: "Score: 0", "Score: 1", etc.

        // Suporte para TextMeshPro
        if (scoreTextTMP != null)
        {
            scoreTextTMP.text = scoreDisplay;
            Debug.Log("📱 ScoreTextTMP atualizado para: " + scoreDisplay);
        }
        else
        {
            Debug.LogWarning("⚠️ Score Text TMP não configurado no GameManager. UI não será atualizada. Arraste o GameObject do TextMeshPro para o campo!");
        }
    }

    // Verifica se o puzzle está completo e trata vitória
    public void CheckPuzzleComplete()
    {
        // PROTEÇÃO CONTRA RECURSÃO INFINITA
        if (isCheckingPuzzle)
        {
            Debug.LogWarning("⚠️ CheckPuzzleComplete já está em execução. Ignorando chamada recursiva.");
            return;
        }

        if (animalSolution == null) return;

        isCheckingPuzzle = true; // Ativa flag

    }

    // Opcional: Carrega um nível específico (atualiza expectedTiles)
    public void LoadLevel(int levelIndex)
    {
        if (puzzleConfigs == null || levelIndex >= puzzleConfigs.Length)
        {
            Debug.LogWarning("⚠️ Configuração para Nível " + levelIndex + " não encontrada!");
            return;
        }

        currentLevel = levelIndex;
        animalSolution.expectedTiles = puzzleConfigs[levelIndex];
        animalSolution.ResetUsedIds(); // Reseta tracking de IDs
        gridManager = GridManager.Instance; // Re-refresca se necessário

        // Opcional: Limpa o grid (libera todas as células)
        ResetGrid();

        // Reseta score para 0 no novo nível (opcional - comente se quiser manter score cumulativo)
        totalScore = 0;
        UpdateScoreUI();

        Debug.Log("🔄 Nível " + currentLevel + " carregado com " + puzzleConfigs[levelIndex].Length + " tiles esperadas. Score resetado para 0.");

        // Opcional: Spawn tiles iniciais aqui
        SpawnInitialTiles();
    }

    // Opcional: Carrega próximo nível
    public void LoadNextLevel()
    {
        LoadLevel(currentLevel + 1);
    }

    // Opcional: Reseta o jogo atual (libera grid e reseta IDs e score)
    public void ResetGame()
    {
        totalScore = 0;
        animalSolution.ResetUsedIds();
        ResetGrid();
        UpdateScoreUI(); // Atualiza UI para 0
        Debug.Log("🔄 Jogo resetado. Score zerado e UI atualizada.");
    }

    // Método auxiliar: Libera todas as células do grid (chamado em reset/load)
    private void ResetGrid()
    {
        if (gridManager != null)
        {
            int totalCells = gridManager.columns * gridManager.rows;
            for (int i = 0; i < totalCells; i++)
            {
                gridManager.FreeCell(i);
            }
            Debug.Log("🧹 Grid resetado: " + totalCells + " células liberadas.");
        }
    }

    // Opcional: Método para spawn inicial de tiles (ex.: fora do grid)
    public void SpawnInitialTiles()
    {
        if (tilePrefab == null)
        {
            Debug.LogWarning("⚠️ tilePrefab não setado no GameManager. Não spawnando tiles.");
            return;
        }

        // Exemplo: Spawn número de tiles igual ao número de expectedTiles
        int numTiles = animalSolution.expectedTiles.Length;
        if (initialSpawnPositions == null || initialSpawnPositions.Length < numTiles)
        {
            Debug.LogWarning("⚠️ initialSpawnPositions insuficiente. Usando posições default.");
            initialSpawnPositions = new Vector3[numTiles];
            for (int i = 0; i < numTiles; i++)
            {
                initialSpawnPositions[i] = new Vector3(3f + i * 2f, 3f, 0); // Linha fora do grid (ajuste para sua grid)
            }
        }

        for (int i = 0; i < numTiles; i++)
        {
            GameObject tileObj = Instantiate(tilePrefab, initialSpawnPositions[i], Quaternion.identity);
            Tile tile = tileObj.GetComponent<Tile>();
            if (tile != null)
            {
                tile.tileId = 100 + i; // ID base único para esta instância
                // frontId/backId já vêm do prefab
                Debug.Log("🆕 Tile " + (100 + i) + " spawnada em " + initialSpawnPositions[i] + " (Front ID=" + tile.frontId + ")");
            }
        }
        Debug.Log("🆕 " + numTiles + " tiles spawnadas inicialmente!");
    }

    // Atualiza UI a cada frame (eficiente para mudanças dinâmicas)
    void Update()
    {
        UpdateScoreUI(); // Garante que UI esteja sempre sincronizada
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Remove listener
        // Opcional: Salva score ao destruir (ex.: fim da cena)
        // PlayerPrefs.SetInt("TotalScore", totalScore);
    }
}