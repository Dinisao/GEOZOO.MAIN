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

        // Clique esquerdo - arrastar
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            dragOffset = transform.position - mousePos;
            Debug.Log("🖱️ Iniciando drag da Tile " + (tile != null ? tile.tileId : "desconhecida"));
        }
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

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // Chama MoveTile com verificações de ocupação
            bool success = tile.MoveTile(new Vector2(mousePos.x, mousePos.y));

            if (!success)
            {
                Debug.Log("❌ Drag finalizado sem sucesso para Tile " + tile.tileId);
            }
            else
            {
                Debug.Log("✅ Drag finalizado com sucesso para Tile " + tile.tileId);
            }

            isDragging = false;
        }
    }
}