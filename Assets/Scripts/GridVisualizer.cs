using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public GridManager gridManager;
    public Color gridColor = Color.white;
    public float lineWidth = 0.1f;

    private LineRenderer[] horizontalLines;
    private LineRenderer[] verticalLines;

    void Start()
    {
        if (gridManager == null) gridManager = GetComponent<GridManager>();
        CreateGridLines();
    }

    void CreateGridLines()
    {
        // Criar linhas horizontais
        horizontalLines = new LineRenderer[gridManager.rows + 1];
        for (int i = 0; i <= gridManager.rows; i++)
        {
            GameObject lineObj = new GameObject($"HorizontalLine_{i}");
            lineObj.transform.parent = transform;
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = gridColor;
            lr.endColor = gridColor;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = 2;
            
            float y = gridManager.origin.y + (i * gridManager.cellSize) - gridManager.cellSize * 0.5f;
            float startX = gridManager.origin.x - gridManager.cellSize * 0.5f;
            float endX = gridManager.origin.x + (gridManager.columns - 1) * gridManager.cellSize + gridManager.cellSize * 0.5f;
            
            lr.SetPosition(0, new Vector3(startX, y, 0));
            lr.SetPosition(1, new Vector3(endX, y, 0));
            
            horizontalLines[i] = lr;
        }

        // Criar linhas verticais
        verticalLines = new LineRenderer[gridManager.columns + 1];
        for (int i = 0; i <= gridManager.columns; i++)
        {
            GameObject lineObj = new GameObject($"VerticalLine_{i}");
            lineObj.transform.parent = transform;
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = gridColor;
            lr.endColor = gridColor;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = 2;
            
            float x = gridManager.origin.x + (i * gridManager.cellSize) - gridManager.cellSize * 0.5f;
            float startY = gridManager.origin.y - gridManager.cellSize * 0.5f;
            float endY = gridManager.origin.y + (gridManager.rows - 1) * gridManager.cellSize + gridManager.cellSize * 0.5f;
            
            lr.SetPosition(0, new Vector3(x, startY, 0));
            lr.SetPosition(1, new Vector3(x, endY, 0));
            
            verticalLines[i] = lr;
        }
    }
}