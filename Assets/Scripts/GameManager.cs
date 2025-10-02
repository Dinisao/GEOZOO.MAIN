using UnityEngine;
using TMPro; // TextMeshPro

public class GameManager : MonoBehaviour
{
    [Header("Animais")]
    public GameObject[] animalCards;
    public Transform cardSpawnPoint;

    [Header("Tiles do Jogador")]
    public Tile[] playerTiles;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    private float timer;
    private bool gameRunning;
    private int score;
    private AnimalSolution currentSolution;

    void Start()
    {
        // Verificações de segurança
        if (animalCards.Length == 0)
        {
            Debug.LogError("❌ Nenhum animal configurado! Adiciona prefabs no campo AnimalCards.");
            return;
        }

        if (playerTiles.Length != 4)
        {
            Debug.LogError("❌ Precisas de exatamente 4 tiles no campo PlayerTiles!");
            return;
        }

        StartNewRound();
    }

    void Update()
    {
        if (gameRunning)
        {
            timer += Time.deltaTime;
            if (timerText != null)
                timerText.text = "Tempo: " + timer.ToString("F2") + "s";
            CheckSolution();
        }
    }

    void StartNewRound()
    {
        timer = 0;
        gameRunning = true;

        // Destruir carta anterior
        if (cardSpawnPoint != null)
        {
            foreach (Transform child in cardSpawnPoint)
            {
                Destroy(child.gameObject);
            }
        }

        // Verificar se há animais disponíveis
        if (animalCards.Length == 0)
        {
            Debug.LogError("❌ Sem animais para spawnar!");
            return;
        }

        // Escolher carta aleatória
        int index = Random.Range(0, animalCards.Length);
        GameObject card = Instantiate(animalCards[index], cardSpawnPoint.position, Quaternion.identity, cardSpawnPoint);

        currentSolution = card.GetComponent<AnimalSolution>();
        if (currentSolution == null)
        {
            Debug.LogError("❌ O prefab " + animalCards[index].name + " não tem AnimalSolution!");
        }
    }

    void CheckSolution()
    {
        if (currentSolution == null || currentSolution.expectedTiles == null) return;

        if (currentSolution.expectedTiles.Length != playerTiles.Length)
        {
            Debug.LogError("❌ O número de expectedTiles não corresponde ao número de playerTiles!");
            return;
        }

        bool correct = true;
        for (int i = 0; i < playerTiles.Length; i++)
        {
            if (!playerTiles[i].IsCorrect(currentSolution.expectedTiles[i]))
            {
                correct = false;
                break;
            }
        }

        if (correct)
        {
            gameRunning = false;
            int points = Mathf.Max(100 - Mathf.RoundToInt(timer * 10), 10);
            score += points;

            if (scoreText != null)
                scoreText.text = "Score: " + score;

            Debug.Log("✅ Puzzle completo! +" + points + " pontos");
            Invoke("StartNewRound", 2f);
        }
    }
}
