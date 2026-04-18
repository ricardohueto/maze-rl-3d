using System.Collections;
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

    // Current position in grid coordinates
    private int currentRow;
    private int currentCol;

    // Exit position
    private int exitRow;
    private int exitCol;

    // To detect wall collisions
    private int lastRow;
    private int lastCol;
    // Track visited cells to penalize revisiting
    private HashSet<(int, int)> visitedCells = new HashSet<(int, int)>();

    public override void OnEpisodeBegin()
    {
        rewardSystem.ResetStepCount();

        visitedCells.Clear();
        visitedCells.Add((0, 0));
        
        // Pick a random seed from the pool
        int[] seedPool = { 42, 99, 123, 256, 512, 1024, 2048, 4096 };
        int randomSeed = seedPool[Random.Range(0, seedPool.Length)];
        mazeGenerator.seed = randomSeed;

        // Regenerate the maze
        mazeGenerator.RegenerateMaze();

        // Place agent at top-left corner
        currentRow = 0;
        currentCol = 0;
        lastRow    = 0;
        lastCol    = 0;

        // Exit is always at bottom-right corner
        exitRow = mazeGenerator.height - 1;
        exitCol = mazeGenerator.width  - 1;

        // Move agent GameObject to starting position
        transform.position = mazeGenerator.GetCellWorldPosition(0, 0)
                             + Vector3.up * 0.5f;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalized position in the grid
        sensor.AddObservation((float)currentRow / mazeGenerator.height);
        sensor.AddObservation((float)currentCol / mazeGenerator.width);

        // Walls around current cell
        MazeCell cell = mazeGenerator.GetCell(currentRow, currentCol);
        sensor.AddObservation(cell.wallNorth ? 1f : 0f);
        sensor.AddObservation(cell.wallSouth ? 1f : 0f);
        sensor.AddObservation(cell.wallEast  ? 1f : 0f);
        sensor.AddObservation(cell.wallWest  ? 1f : 0f);

        // Normalized distance to exit
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

