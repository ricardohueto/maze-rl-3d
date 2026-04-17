using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class RewardSystem : MonoBehaviour
{
    [Header("Reward Values")]
    public float rewardGoal        =  1.0f;
    public float penaltyStep       = -0.01f;
    public float penaltyWall       = -0.5f;
    public float penaltyTimeout    = -1.0f;

    [Header("Settings")]
    public int maxSteps = 1000;

    private int stepCount = 0;

    public void ResetStepCount()
    {
        stepCount = 0;
    }

    public void EvaluateStep(
        MazeAgent agent,
        int currentRow, int currentCol,
        int exitRow,    int exitCol,
        bool hitWall)
    {
        stepCount++;

        // Hit a wall
        if (hitWall)
        {
            agent.AddReward(penaltyWall);
            return;
        }

        // Reached the exit
        if (currentRow == exitRow && currentCol == exitCol)
        {
            agent.AddReward(rewardGoal);
            agent.EndEpisode();
            return;
        }

        // Timeout
        if (stepCount >= maxSteps)
        {
            agent.AddReward(penaltyTimeout);
            agent.EndEpisode();
            return;
        }

        // Default step penalty
        agent.AddReward(penaltyStep);
    }
}