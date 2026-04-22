using UnityEngine;
using Unity.MLAgents;

public class RewardSystem : MonoBehaviour
{
    [Header("Reward Values")]
    public float rewardGoal     =  1.0f;
    public float penaltyStep    = -0.01f;
    public float penaltyTimeout = -1.0f;

    private int stepCount = 0;
    private int maxSteps  = 200;

    public void ResetStepCount(int mazeSize)
    {
        stepCount = 0;
        maxSteps = mazeSize * mazeSize * 5;
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
            float stepsUsed = (float)stepCount / maxSteps;
            float efficiencyBonus = (1.0f - stepsUsed) * 1.0f;  // hasta +1.0 extra
            
            agent.AddReward(rewardGoal + efficiencyBonus);
            
            CurriculumManager.Instance.NotifyEpisodeEnd(true);
            agent.EndEpisode();
            return;
        }

        if (stepCount >= maxSteps)
        {
            agent.AddReward(penaltyTimeout);
            CurriculumManager.Instance.NotifyEpisodeEnd(false);
            agent.EndEpisode();
            return;
        }

        agent.AddReward(penaltyStep);
    }
}