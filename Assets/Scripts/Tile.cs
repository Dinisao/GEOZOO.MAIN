using UnityEngine;

public class Tile : MonoBehaviour
{
    private GridManager gridManager;
    public int tileId;           // ID base da peça física (opcional, para tracking geral)
    public int frontId;          // ID da face da frente (não flipada) - pode ser igual em múltiplas tiles
    public int backId;           // ID da face das costas (flipada) - pode ser igual em múltiplas tiles
    public int currentRotation;  // 0, 90, 180, 270
    public int currentCell = -1;
    public bool isFlipped = false;
    public bool ignoreFlip = false; // Flag: true para tiles simétricas (ignora flip na validação)
    public bool trackUsedId = false; // Flag: true se este ID deve ser marcado como usado após colocação correta (opcional)
    private bool hasScoredInCurrentCell = false; // Evita pontos múltiplos na mesma célula correta

    [Header("Sprites da Tile")]
    public Sprite frontSprite;
    public Sprite backSprite;

    private SpriteRenderer spriteRenderer;
    private Vector3 previousPosition;
    public bool isMoving = false; // Flag para evitar movimentos simultâneos
    private int currentId;         // ID atual da face visível (muda com flip)
    private int instanceId;        // ID único da instância da tile (para distinguir duplicatas em logs)

    void Start()
    {
        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("❌ GridManager não encontrado na cena!");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("❌ SpriteRenderer não encontrado em " + gameObject.name);
        }

        previousPosition = transform.position; // Inicializa corretamente para evitar posições inválidas

        // Inicializa com face da frente
        if (spriteRenderer != null && frontSprite != null)
            spriteRenderer.sprite = frontSprite;

        currentRotation = 0; // Inicializa rotação se necessário
        currentId = frontId; // Começa com ID da frente
        instanceId = GetInstanceID(); // ID único para esta tile (diferencia duplicatas)

        // Log para debug: confirma configuração de IDs e duplicatas
        string dupNote = (frontId == backId) ? " (IDs iguais nas faces)" : "";
        Debug.Log("🔄 Tile base " + tileId + " (Instância #" + instanceId + ") inicializada: Front ID=" + frontId +
                  ", Back ID=" + backId + ", Current ID=" + currentId + dupNote +
                  (ignoreFlip ? " (simétrica, ignora flip)" : "") +
                  (trackUsedId ? " (rastreia uso de ID)" : ""));

        if (frontId == backId)
        {
            Debug.Log("🔄 Tile " + tileId + " (Instância #" + instanceId + ") tem IDs iguais nas faces (flip não afeta ID)");
        }
    }

    public void RotateTile()
    {
        if (isMoving) return; // Não rotaciona durante movimento

        transform.Rotate(0, 0, 90);
        currentRotation = (currentRotation + 90) % 360;
        Debug.Log("🔄 Tile " + tileId + " (Instância #" + instanceId + ") rotacionada para " + currentRotation +
                  " graus (ID atual: " + currentId + ")");

        // **NOVA LÓGICA: Revalida se já está na grid (currentCell >= 0)**
        if (currentCell >= 0)
        {
            RevalidatePlacement("após rotação");
        }
    }

    public void FlipTile()
    {
        if (isMoving) return; // Não flipa durante movimento

        isFlipped = !isFlipped;
        currentId = isFlipped ? backId : frontId; // Muda o ID baseado na face atual

        if (spriteRenderer != null)
        {
            if (isFlipped && backSprite != null)
                spriteRenderer.sprite = backSprite;
            else if (!isFlipped && frontSprite != null)
                spriteRenderer.sprite = frontSprite;
        }

        string face = isFlipped ? "costas" : "frente";
        Debug.Log("🔄 Tile " + tileId + " (Instância #" + instanceId + ") flipada para " + face +
                  " (ID agora: " + currentId + ")" +
                  (ignoreFlip ? " (mas flip ignorado na validação)" : ""));

        // **NOVA LÓGICA: Revalida se já está na grid (currentCell >= 0)**
        if (currentCell >= 0)
        {
            RevalidatePlacement("após flip");
        }
    }

    // **NOVO MÉTODO: Revalida colocação após rotate/flip (só se na grid e não pontuou ainda)**
    private void RevalidatePlacement(string action)
    {
        AnimalSolution solution = AnimalSolution.Instance;
        if (solution == null || currentCell < 0)
        {
            Debug.LogWarning("⚠️ [DEBUG SCORE] Revalidação ignorada: Solution null ou tile fora da grid (" + currentCell + ").");
            return;
        }

        Debug.Log("🔍 [DEBUG SCORE] Revalidação " + action + " para Tile " + tileId + " (Instância #" + instanceId +
                  ") em cell " + currentCell + " (ID atual=" + currentId + ", Flip=" + isFlipped + ", Rot=" + currentRotation + ")");

        bool isCorrect = solution.IsCorrectPlacement(this);
        Debug.Log("🔍 [DEBUG SCORE] Revalidação " + action + ": IsCorrectPlacement retornou " + isCorrect);

        if (isCorrect && !hasScoredInCurrentCell)
        {
            int points = GameManager.Instance.scorePerCorrect;
            GameManager.Instance.AddScore(points);
            hasScoredInCurrentCell = true;
            Debug.Log("✅ [DEBUG SCORE] Colocação agora correta " + action + "! + " + points + " ponto(s) adicionados para Tile " + tileId + " em cell " + currentCell);

            // Opcional: Checa se puzzle completo após correção
            GameManager.Instance.CheckPuzzleComplete();
        }
        else if (isCorrect)
        {
            Debug.Log("ℹ️ [DEBUG SCORE] Colocação correta " + action + ", mas já pontuou nesta célula (sem +pontos extras).");
        }
        else
        {
            Debug.Log("❌ [DEBUG SCORE] Colocação ainda incorreta após " + action + " - sem pontos.");
            // Opcional: Se quiser resetar flag se virar errado (para permitir re-pontuar se corrigir de novo), descomente:
            // hasScoredInCurrentCell = false;
        }
    }

    public bool MoveTile(Vector2 position)
    {
        if (gridManager == null || isMoving)
        {
            Debug.LogWarning("⚠️ Não é possível mover: GridManager ausente ou tile em movimento");
            return false;
        }

        isMoving = true; // Bloqueia movimentos simultâneos

        Vector2 snapPos = gridManager.GetNearestCell(position);
        int newCell = gridManager.GetCellIndex(snapPos);

        // **DEBUG SCORE: Log inicial do movimento**
        Debug.Log("🔍 [DEBUG SCORE] Início MoveTile: Posição alvo=" + position + ", Snap=" + snapPos + ", NewCell=" + newCell + ", CurrentCell anterior=" + currentCell);

        // Se mesma célula, só atualiza posição (sem checks ou pontos)
        if (newCell == currentCell)
        {
            transform.position = snapPos;
            previousPosition = snapPos;
            isMoving = false;
            Debug.Log("⚠️ [DEBUG SCORE] Mesma célula - sem validação de score.");
            return true;
        }

        // Verifica se nova célula está ocupada
        if (gridManager.IsCellOccupied(newCell))
        {
            transform.position = previousPosition;
            Debug.Log("❌ [DEBUG SCORE] Movimento rejeitado: Célula " + newCell + " ocupada.");
            isMoving = false;
            return false;
        }

        // Libera célula anterior se válida
        if (currentCell >= 0)
            gridManager.FreeCell(currentCell);

        // Tenta ocupar nova célula
        if (gridManager.OccupyCell(newCell, this))
        {
            transform.position = snapPos;
            previousPosition = snapPos;
            currentCell = newCell;
            isMoving = false;

            // **IMPORTANTE: Reset flag de score ao MUDAR de célula (permite pontuar na nova)**
            hasScoredInCurrentCell = false;

            // **DEBUG SCORE: Estado da tile após movimento**
            Debug.Log("✅ [DEBUG SCORE] Movimento OK: Tile " + tileId + " (ID=" + currentId + ", Flip=" + isFlipped + ", Rot=" + currentRotation + ") em célula " + currentCell);

            // Verifica solução (validação inicial na nova célula)
            AnimalSolution solution = AnimalSolution.Instance;
            if (solution != null)
            {
                Debug.Log("🔍 [DEBUG SCORE] Chamando IsCorrectPlacement para célula " + currentCell);
                bool isCorrect = solution.IsCorrectPlacement(this);
                Debug.Log("🔍 [DEBUG SCORE] IsCorrectPlacement retornou: " + isCorrect);

                if (isCorrect && !hasScoredInCurrentCell)
                {
                    int points = GameManager.Instance.scorePerCorrect;
                    GameManager.Instance.AddScore(points);
                    hasScoredInCurrentCell = true;
                    Debug.Log("✅ [DEBUG SCORE] Colocação correta! + " + points + " ponto(s) adicionados.");
                }
                else if (!isCorrect)
                {
                    Debug.Log("❌ [DEBUG SCORE] Validação falhou - sem pontos.");
                }
            }
            else
            {
                Debug.LogError("❌ [DEBUG SCORE] AnimalSolution.Instance é NULL - score não pode ser adicionado!");
            }

            Debug.Log("✅ Tile " + tileId + " (Instância #" + instanceId + ", ID: " + currentId + ") movida para célula " + newCell);
            return true;
        }
        else
        {
            if (currentCell >= 0)
                gridManager.OccupyCell(currentCell, this);
            transform.position = previousPosition;
            Debug.Log("❌ [DEBUG SCORE] Falha na ocupação da célula " + newCell);
            isMoving = false;
            return false;
        }
    }

    // Atualizada: Suporte a duplicatas - aceita se ID bater, mas checa se permite duplicatas ou se já usado
    public bool IsCorrect(TileData expected)
    {
        if (expected == null)
        {
            Debug.LogError("❌ [DEBUG SCORE] Expected é NULL em IsCorrect!");
            return false;
        }

        // Verificações básicas
        bool cellMatch = currentCell == expected.correctCell;
        bool rotationMatch = currentRotation == expected.correctRotation;
        bool idMatch = currentId == expected.requiredTileId; // Aceita duplicatas se ID bater

        // Verificação de flip: só se não for simétrica
        bool flipMatch = true; // Padrão: assume correto
        if (!ignoreFlip)
        {
            flipMatch = isFlipped == expected.isFlipped;
        }
        else
        {
            Debug.Log("🔄 Ignorando flip para Tile simétrica " + tileId + " (Instância #" + instanceId +
                      ", ID atual: " + currentId + ")");
        }

        // Checagem de duplicatas/usado: Se não permite duplicatas e ID já usado, rejeita
        bool duplicateCheck = true;
        if (!expected.allowDuplicates && trackUsedId)
        {
            duplicateCheck = !AnimalSolution.Instance.IsIdUsed(currentId);
            if (!duplicateCheck)
            {
                Debug.Log("❌ ID " + currentId + " já usado em outra tile. Duplicata rejeitada para " +
                          tileId + " (Instância #" + instanceId + ")");
            }
        }

        bool isCorrect = cellMatch && rotationMatch && flipMatch && idMatch && duplicateCheck;

        // **DEBUG SCORE: Log detalhado de cada checagem**
        Debug.Log("🔍 [DEBUG SCORE] IsCorrect para Tile " + tileId + " (ID atual=" + currentId + ", Cell atual=" + currentCell + ", Flip atual=" + isFlipped + ", Rot atual=" + currentRotation +
                  ") vs Expected (ID=" + expected.requiredTileId + ", Cell=" + expected.correctCell + ", Flip=" + expected.isFlipped + ", Rot=" + expected.correctRotation + "): " +
                  "CellMatch=" + cellMatch + ", IDMatch=" + idMatch + ", FlipMatch=" + flipMatch + ", RotMatch=" + rotationMatch + ", DupCheck=" + duplicateCheck +
                  " → RESULTADO=" + isCorrect);

        return isCorrect;
    }

    // Opcional: Sobrecarga para usar ignoreFlip do TileData
    public bool IsCorrect(TileData expected, bool useExpectedIgnoreFlip)
    {
        if (useExpectedIgnoreFlip && expected != null && expected.ignoreFlip)
        {
            bool originalIgnore = ignoreFlip;
            ignoreFlip = true;
            bool result = IsCorrect(expected);
            ignoreFlip = originalIgnore;
            return result;
        }
        return IsCorrect(expected);
    }

    // Getters públicos
    public int GetCurrentId()
    {
        return currentId;
    }

    public int GetInstanceId()
    {
        return instanceId;
    }

    void OnDestroy()
    {
        if (gridManager != null && currentCell >= 0)
            gridManager.FreeCell(currentCell);
        isMoving = false;
    }
}