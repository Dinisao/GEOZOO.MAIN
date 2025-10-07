using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TileController : MonoBehaviour
{
    private Camera cam;
    private bool isDragging;
    private Tile tile;
    private Vector3 dragOffset;

    void Awake()
    {
        // Procura a câmera principal
        cam = Camera.main;

        // Se não encontrar pela tag, procura qualquer câmera
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
            Debug.LogWarning("⚠️ Camera.main não encontrada! Usando câmera alternativa.");
        }

        if (cam == null)
        {
            Debug.LogError("❌ Nenhuma câmera encontrada na cena!");
        }

        tile = GetComponent<Tile>();

        if (tile == null)
        {
            Debug.LogError("❌ Tile.cs não encontrado em " + gameObject.name);
        }
    }

    void OnMouseDown()
    {
        if (tile == null || cam == null || tile.isMoving) return; // Não inicia drag se em movimento

        // Inicia drag (OnMouseDown já é para clique esquerdo - sem check extra)
        isDragging = true;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        dragOffset = transform.position - mousePos;
        Debug.Log("🖱️ Iniciando drag da Tile " + (tile != null ? tile.tileId : "desconhecida") + " (ID atual: " + (tile != null ? tile.GetCurrentId() : "N/A") + ")");
    }

    void OnMouseOver()
    {
        if (tile == null || tile.isMoving) return; // Ignora interações durante movimento

        // Clique direito - rotacionar (tem que estar com o mouse em cima)
        if (Input.GetMouseButtonDown(1))
        {
            tile.RotateTile();
        }

        // Clique do meio (scroll) - flip
        if (Input.GetMouseButtonDown(2))
        {
            tile.FlipTile();
        }
    }

    void OnMouseDrag()
    {
        if (cam == null || !isDragging || tile == null) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // Opcional: Snap durante drag para evitar sobreposições visuais (descomente para ativar)
        // Vector2 snappedPos = tile.gridManager.GetNearestCell(new Vector2(mousePos.x + dragOffset.x, mousePos.y + dragOffset.y));
        // transform.position = new Vector3(snappedPos.x, snappedPos.y, 0);

        // Movimento livre (padrão)
        transform.position = mousePos + dragOffset;
    }

    void OnMouseUp()
    {
        if (tile == null || cam == null || !isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // **DEBUG SCORE: Log posição final antes de MoveTile**
        Debug.Log("🔍 [DEBUG TILECONTROLLER] Finalizando drag: Posição mouse=" + mousePos + ", Tile ID=" + tile.tileId + ", CurrentCell anterior=" + tile.currentCell);

        // Chama MoveTile com verificações de ocupação
        bool success = tile.MoveTile(new Vector2(mousePos.x, mousePos.y));

        // **DEBUG SCORE: Log resultado e cell final**
        int finalCell = tile.currentCell; // Pega cell após MoveTile
        Debug.Log("🔍 [DEBUG TILECONTROLLER] MoveTile retornou: " + success + ". Cell final=" + finalCell +
                  (finalCell == 0 ? " (TESTE CELL 0! Verifique validação abaixo)" : "") +
                  ". Se success=true e cell=0, score deve +1 se ID/flip bater.");

        if (!success)
        {
            Debug.Log("❌ Drag finalizado sem sucesso para Tile " + tile.tileId + " (possível célula ocupada ou inválida).");
        }
        else
        {
            Debug.Log("✅ Drag finalizado com sucesso para Tile " + tile.tileId + " em cell " + finalCell +
                      ". Aguardando logs de validação/score...");
        }

        isDragging = false;
    }
}