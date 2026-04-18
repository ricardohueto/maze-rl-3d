using UnityEngine;
using Unity.MLAgents;

public class RewardSystem : MonoBehaviour
{
    [Header("Reward Values")]
    public float rewardGoal        =  1.0f;
    public float penaltyStep       = -0.001f;
    public float penaltyWall       = -0.06f;
    public float penaltyTimeout    = -0.5f;
    public float penaltyRevisit    = -0.06f;
    public float rewardProgress    =  0.05f;

    [Header("Settings")]
    public int maxSteps = 500;

    private int   stepCount        = 0;
    private float previousDistance = -1f;

    public void ResetStepCount()
    {
        stepCount        = 0;
        previousDistance = -1f;
    }

    public void EvaluateStep(
        MazeAgent agent,
        int currentRow, int currentCol,
        int exitRow,    int exitCol,
        bool hitWall,   bool isRevisit)
    {
        stepCount++;

        if (hitWall)
        {
            agent.AddReward(penaltyWall);
            return;
        }

        if (currentRow == exitRow && currentCol == exitCol)
        {
            agent.AddReward(rewardGoal);
            agent.EndEpisode();
            return;
        }

        if (stepCount >= maxSteps)
        {
            agent.AddReward(penaltyTimeout);
            agent.EndEpisode();
            return;
        }

        // Progress signal
        float currentDistance = Mathf.Abs(currentRow - exitRow)
                              + Mathf.Abs(currentCol - exitCol);

        if (previousDistance >= 0f && currentDistance < previousDistance)
            agent.AddReward(rewardProgress);

        previousDistance = currentDistance;

        // Revisit penalty
        if (isRevisit)
            agent.AddReward(penaltyRevisit);

        agent.AddReward(penaltyStep);
    }
}