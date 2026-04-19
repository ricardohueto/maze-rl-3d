using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MazeAgent : Agent
{
    [Header("References")]
    public MazeGenerator mazeGenerator;
    public RewardSystem rewardSystem;

    [Header("Agent Settings")]
    public float moveSpeed = 5f;

    // Grid position
    private int currentRow;
    private int currentCol;
    private int exitRow;
    private int exitCol;
    private int lastRow;
    private int lastCol;

    // Visited cells
    private HashSet<(int, int)> visitedCells = new HashSet<(int, int)>();

    // Curriculum
    private int   episodeCount    = 0;
    private int   successCount    = 0;
    private int   windowSize      = 100;
    private int   currentMazeSize = 3;
    private float advanceThreshold   = 0.7f;
    private float downgradeThreshold = 0.3f;

    public void NotifySuccess()
    {
        successCount++;
    }

    public override void OnEpisodeBegin()
    {
        episodeCount++;

        // Evaluate curriculum every windowSize episodes
        if (episodeCount % windowSize == 0)
        {
            float successRate = (float)successCount / windowSize;
            successCount = 0;

            Debug.Log($"[Curriculum] Size: {currentMazeSize}x{currentMazeSize} | Success rate: {successRate:P0}");

            if (successRate >= advanceThreshold && currentMazeSize < 10)
            {
                currentMazeSize = Mathf.Min(currentMazeSize + 2, 10);
                Debug.Log($"[Curriculum] Advancing to {currentMazeSize}x{currentMazeSize}");
            }
            else if (successRate < downgradeThreshold && currentMazeSize > 3)
            {
                currentMazeSize = Mathf.Max(currentMazeSize - 2, 3);
                Debug.Log($"[Curriculum] Downgrading to {currentMazeSize}x{currentMazeSize}");
            }
        }

        // Configure maze
        mazeGenerator.width  = currentMazeSize;
        mazeGenerator.height = currentMazeSize;
        mazeGenerator.seed   = Random.Range(0, 100000);
        mazeGenerator.RegenerateMaze();

        // Reset agent
        currentRow = 0;
        currentCol = 0;
        lastRow    = 0;
        lastCol    = 0;
        visitedCells.Clear();
        visitedCells.Add((0, 0));

        exitRow = mazeGenerator.height - 1;
        exitCol = mazeGenerator.width  - 1;

        transform.position = mazeGenerator.GetCellWorldPosition(0, 0)
                             + Vector3.up * 0.5f;

        // Pass maze size to reward system
        rewardSystem.ResetStepCount(currentMazeSize);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((float)currentRow / mazeGenerator.height);
        sensor.AddObservation((float)currentCol / mazeGenerator.width);

        MazeCell cell = mazeGenerator.GetCell(currentRow, currentCol);
        sensor.AddObservation(cell.wallNorth ? 1f : 0f);
        sensor.AddObservation(cell.wallSouth ? 1f : 0f);
        sensor.AddObservation(cell.wallEast  ? 1f : 0f);
        sensor.AddObservation(cell.wallWest  ? 1f : 0f);

        float distRow = (float)(exitRow - currentRow) / mazeGenerator.height;
        float distCol = (float)(exitCol - currentCol) / mazeGenerator.width;
        sensor.AddObservation(distRow);
        sensor.AddObservation(distCol);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        lastRow = currentRow;
        lastCol = currentCol;

        int targetRow = currentRow;
        int targetCol = currentCol;

        switch (action)
        {
            case 0: targetRow++; break; // North
            case 1: targetRow--; break; // South
            case 2: targetCol++; break; // East
            case 3: targetCol--; break; // West
        }

        MazeCell currentCell = mazeGenerator.GetCell(currentRow, currentCol);
        bool wallBlocking = false;

        switch (action)
        {
            case 0: wallBlocking = currentCell.wallNorth; break;
            case 1: wallBlocking = currentCell.wallSouth; break;
            case 2: wallBlocking = currentCell.wallEast;  break;
            case 3: wallBlocking = currentCell.wallWest;  break;
        }

        bool isRevisit = false;
        if (!wallBlocking && mazeGenerator.IsInBounds(targetRow, targetCol))
        {
            currentRow = targetRow;
            currentCol = targetCol;
            transform.position = mazeGenerator.GetCellWorldPosition(currentRow, currentCol)
                                 + Vector3.up * 0.5f;

            isRevisit = visitedCells.Contains((currentRow, currentCol));
            visitedCells.Add((currentRow, currentCol));
        }

        rewardSystem.EvaluateStep(
            this, currentRow, currentCol,
            exitRow, exitCol,
            wallBlocking, isRevisit
        );
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            discreteActions[0] = 2;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            discreteActions[0] = 3;
    }
}