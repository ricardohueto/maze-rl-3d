using UnityEngine;
using Unity.MLAgents;

public class RewardSystem : MonoBehaviour
{
    [Header("Reward Values")]
    public float rewardGoal     =  1.0f;
    public float penaltyStep    = -0.005f;
    public float penaltyTimeout = -1.0f;

    private int stepCount = 0;
    private int maxSteps  = 200;

    public void ResetStepCount(int mazeSize)
    {
        stepCount = 0;
        // Scale max steps with maze size
        // 3x3 → 50 steps, 5x5 → 100 steps, 10x10 → 300 steps
        maxSteps = mazeSize * mazeSize * 3;
    }

    public void EvaluateStep(
        MazeAgent agent,
        int currentRow, int currentCol,
        int exitRow,    int exitCol,
        bool hitWall,   bool isRevisit)
    {
        stepCount++;

        if (currentRow == exitRow && currentCol == exitCol)
        {
            agent.AddReward(rewardGoal);
            agent.NotifySuccess();
            agent.EndEpisode();
            return;
        }

        if (stepCount >= maxSteps)
        {
            agent.AddReward(penaltyTimeout);
            agent.EndEpisode();
            return;
        }

        agent.AddReward(penaltyStep);
    }
}