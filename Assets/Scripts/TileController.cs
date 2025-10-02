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
        if (tile == null || cam == null) return;

        // Clique esquerdo - arrastar
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            dragOffset = transform.position - mousePos;
        }
    }

    void OnMouseOver()
    {
        if (tile == null) return;

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
        if (cam == null || !isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos + dragOffset;
    }

    void OnMouseUp()
    {
        if (tile == null || cam == null) return;

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            tile.MoveTile(new Vector2(mousePos.x, mousePos.y));
            isDragging = false;
        }
    }
}