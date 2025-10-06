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

        // Se mesma célula, só atualiza posição (sem checks)
        if (newCell == currentCell)
        {
            transform.position = snapPos;
            previousPosition = snapPos;
            isMoving = false;
            return true;
        }

        // Verifica se nova célula está ocupada (reforçado: não permite mesmo se for a mesma tile)
        if (gridManager.IsCellOccupied(newCell))
        {
            // Rollback imediato: volta para posição anterior (sem mudança de cor)
            transform.position = previousPosition;

            Debug.Log("❌ Movimento rejeitado: Célula " + newCell + " ocupada. Tile " + tileId +
                      " (Instância #" + instanceId + ", ID: " + currentId + ") voltou para célula " + currentCell);
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

            // Verifica solução (se existir) - agora usa IsCorrect com suporte a duplicatas
            AnimalSolution solution = AnimalSolution.Instance;
            if (solution != null && solution.IsCorrectPlacement(this))
            {
                GameManager.Instance.AddScore(10); // Usa singleton para otimizar
                Debug.Log("✅ Colocação correta! Pontos adicionados para Tile " + tileId +
                          " (Instância #" + instanceId + ", ID: " + currentId + ")");
            }

            Debug.Log("✅ Tile " + tileId + " (Instância #" + instanceId + ", ID: " + currentId + ") movida para célula " + newCell);
            return true;
        }
        else
        {
            // Rollback se falhou na ocupação (raro, mas possível)
            if (currentCell >= 0)
                gridManager.OccupyCell(currentCell, this); // Re-ocupa anterior
            transform.position = previousPosition;

            Debug.Log("❌ Falha na ocupação da célula " + newCell + ". Tile " + tileId +
                      " (Instância #" + instanceId + ", ID: " + currentId + ") voltou para anterior.");
            isMoving = false;
            return false;
        }
    }

    // Atualizada: Suporte a duplicatas - aceita se ID bater, mas checa se permite duplicatas ou se já usado
    public bool IsCorrect(TileData expected)
    {
        if (expected == null) return false;

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
            // Chama método global para checar se ID já foi usado
            duplicateCheck = !AnimalSolution.Instance.IsIdUsed(currentId);
            if (!duplicateCheck)
            {
                Debug.Log("❌ ID " + currentId + " já usado em outra tile. Duplicata rejeitada para " +
                          tileId + " (Instância #" + instanceId + ")");
            }
        }

        bool isCorrect = cellMatch && rotationMatch && flipMatch && idMatch && duplicateCheck;

        Debug.Log("🔍 Verificando Tile " + tileId + " (Instância #" + instanceId + ", ID atual: " + currentId + "): " +
                  "Célula: " + (cellMatch ? "OK" : "ERR") +
                  ", Rotação: " + (rotationMatch ? "OK" : "ERR") +
                  ", Flip: " + (flipMatch ? "OK" : (ignoreFlip ? "IGNORADO" : "ERR")) +
                  ", ID Face: " + (idMatch ? "OK" : "ERR") +
                  ", Duplicata: " + (duplicateCheck ? "OK" : "USADO") +
                  " → " + (isCorrect ? "Correta!" : "Incorreta."));

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