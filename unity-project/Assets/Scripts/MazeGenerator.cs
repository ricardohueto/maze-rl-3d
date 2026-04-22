using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width    = 10;
    public int height   = 10;
    public int seed     = 42;

    [Header("Prefabs")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;

    [Header("Cell Size")]
    public float cellSize = 2f;

    private MazeCell[,] grid;
    
    [Header("Materials")]
    public Material exitMaterial;

    [Header("Parallel Training")]
    public Vector3 areaOffset = Vector3.zero;

    void Start()
    {
        GenerateMaze();
    }
    public void GenerateMaze()
    {
        grid = new MazeCell[height, width];

        for (int row = 0; row < height; row++)
            for (int col = 0; col < width; col++)
                grid[row, col] = CreateCell(row, col);

        System.Random rng = new System.Random(seed);
        int startRow = rng.Next(0, height);
        int startCol = rng.Next(0, width);
        VisitCell(startRow, startCol, rng);

        if (exitMaterial != null)
        {
            int exitRow = height - 1;
            int exitCol = width  - 1;
            GameObject floor = grid[exitRow, exitCol].floorObj;
            floor.GetComponent<Renderer>().material = exitMaterial;
        }

        UpdateWallVisibility();
        CreateBorder();
    }

    private MazeCell CreateCell(int row, int col)
    {
        float x = col * cellSize + areaOffset.x;
        float z = row * cellSize + areaOffset.z;
        Vector3 cellPosition = new Vector3(x, areaOffset.y, z);

        GameObject cellObj = new GameObject($"Cell_{row}_{col}");
        cellObj.transform.position = cellPosition;
        cellObj.transform.parent   = this.transform;

        MazeCell cell = cellObj.AddComponent<MazeCell>();

        // Floor
        cell.floorObj = Instantiate(
            floorPrefab, cellPosition,
            Quaternion.identity, cellObj.transform
        );
        cell.floorObj.name = "Floor";
        cell.floorObj.transform.localScale = new Vector3(cellSize, 0.1f, cellSize);

        // All 4 walls — algorithm will decide which to remove
        cell.wallNorthObj = CreateWall(cellObj, cellPosition, "N");
        cell.wallSouthObj = CreateWall(cellObj, cellPosition, "S");
        cell.wallEastObj  = CreateWall(cellObj, cellPosition, "E");
        cell.wallWestObj  = CreateWall(cellObj, cellPosition, "W");

        return cell;
    }

    private GameObject CreateWall(GameObject parent, Vector3 cellPos, string direction)
    {
        GameObject wall = Instantiate(
            wallPrefab, cellPos,
            Quaternion.identity, parent.transform
        );
        wall.name = $"Wall_{direction}";

        float half      = cellSize / 2f;
        float wallH     = 1f;
        float thickness = 0.1f;

        switch (direction)
        {
            case "N":
                wall.transform.localPosition = new Vector3(0, wallH / 2f, half);
                wall.transform.localScale    = new Vector3(cellSize, wallH, thickness);
                break;
            case "S":
                wall.transform.localPosition = new Vector3(0, wallH / 2f, -half);
                wall.transform.localScale    = new Vector3(cellSize, wallH, thickness);
                break;
            case "E":
                wall.transform.localPosition = new Vector3(half, wallH / 2f, 0);
                wall.transform.localScale    = new Vector3(thickness, wallH, cellSize);
                break;
            case "W":
                wall.transform.localPosition = new Vector3(-half, wallH / 2f, 0);
                wall.transform.localScale    = new Vector3(thickness, wallH, cellSize);
                break;
        }

        return wall;
    }

    private void VisitCell(int row, int col, System.Random rng)
    {
        grid[row, col].visited = true;

        List<string> directions = new List<string> { "N", "S", "E", "W" };
        Shuffle(directions, rng);

        foreach (string direction in directions)
        {
            int newRow = row, newCol = col;

            if      (direction == "N") newRow++;
            else if (direction == "S") newRow--;
            else if (direction == "E") newCol++;
            else if (direction == "W") newCol--;

            if (IsInBounds(newRow, newCol) && !grid[newRow, newCol].visited)
            {
                // Only update the data — walls removed visually in UpdateWallVisibility
                RemoveWallData(row, col, newRow, newCol, direction);
                VisitCell(newRow, newCol, rng);
            }
        }
    }

    private void RemoveWallData(int row, int col,
                                int newRow, int newCol,
                                string direction)
    {
        string opposite = "";
        switch (direction)
        {
            case "N": opposite = "S"; break;
            case "S": opposite = "N"; break;
            case "E": opposite = "W"; break;
            case "W": opposite = "E"; break;
        }

        SetWall(grid[row, col], direction, false);
        SetWall(grid[newRow, newCol], opposite, false);
    }

    private void SetWall(MazeCell cell, string direction, bool value)
    {
        switch (direction)
        {
            case "N": cell.wallNorth = value; break;
            case "S": cell.wallSouth = value; break;
            case "E": cell.wallEast  = value; break;
            case "W": cell.wallWest  = value; break;
        }
    }

    private void UpdateWallVisibility()
    {
        // After algorithm runs, destroy walls that are marked as removed
        // For shared walls, only destroy from the cell that owns it
        // to avoid destroying an already-destroyed object
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                MazeCell cell = grid[row, col];

                if (!cell.wallNorth && cell.wallNorthObj != null)
                    Destroy(cell.wallNorthObj);

                if (!cell.wallSouth && cell.wallSouthObj != null)
                    Destroy(cell.wallSouthObj);

                if (!cell.wallEast && cell.wallEastObj != null)
                    Destroy(cell.wallEastObj);

                if (!cell.wallWest && cell.wallWestObj != null)
                    Destroy(cell.wallWestObj);
            }
        }
    }

    public bool IsInBounds(int row, int col)
    {
        return row >= 0 && row < height && col >= 0 && col < width;
    }

    private void Shuffle(List<string> list, System.Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            string temp = list[i];
            list[i]     = list[j];
            list[j]     = temp;
        }
    }

    private void CreateBorder()
    {
        float wallH     = 1f;
        float thickness = 0.2f;

        float minX = -cellSize / 2f + areaOffset.x;
        float maxX = (width  - 1) * cellSize + cellSize / 2f + areaOffset.x;
        float minZ = -cellSize / 2f + areaOffset.z;
        float maxZ = (height - 1) * cellSize + cellSize / 2f + areaOffset.z;

        float centerX    = (minX + maxX) / 2f;
        float centerZ    = (minZ + maxZ) / 2f;
        float totalWidth = maxX - minX;
        float totalDepth = maxZ - minZ;

        CreateBorderWall(
            new Vector3(centerX, wallH / 2f, minZ),
            new Vector3(totalWidth + thickness, wallH, thickness)
        );
        CreateBorderWall(
            new Vector3(centerX, wallH / 2f, maxZ),
            new Vector3(totalWidth + thickness, wallH, thickness)
        );
        CreateBorderWall(
            new Vector3(minX, wallH / 2f, centerZ),
            new Vector3(thickness, wallH, totalDepth + thickness)
        );
        CreateBorderWall(
            new Vector3(maxX, wallH / 2f, centerZ),
            new Vector3(thickness, wallH, totalDepth + thickness)
        );
    }

    private void CreateBorderWall(Vector3 position, Vector3 scale)
    {
        GameObject border = Instantiate(
            wallPrefab, position,
            Quaternion.identity, this.transform
        );
        border.name = "BorderWall";
        border.transform.localScale = scale;
    }

    public void RegenerateMaze()
    {
        // Destroy all existing cell GameObjects
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // Regenerate
        GenerateMaze();
    }

    public Vector3 GetCellWorldPosition(int row, int col)
    {
        return new Vector3(
            col * cellSize + areaOffset.x,
            areaOffset.y,
            row * cellSize + areaOffset.z
        );
    }

    public MazeCell GetCell(int row, int col)
    {
        return grid[row, col];
    }
}