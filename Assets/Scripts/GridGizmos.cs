using UnityEngine;

[ExecuteInEditMode]
public class GridGizmos : MonoBehaviour
{
    public GridManager grid;

    void OnDrawGizmos()
    {
        if (grid == null) grid = GetComponent<GridManager>();

        Gizmos.color = Color.yellow;
        for (int r = 0; r < grid.rows; r++)
        {
            for (int c = 0; c < grid.columns; c++)
            {
                Vector2 pos = grid.origin + new Vector2(c * grid.cellSize, r * grid.cellSize);
                Gizmos.DrawWireCube(pos, Vector3.one * grid.cellSize * 0.9f);
            }
        }
    }
}