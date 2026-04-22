using UnityEngine;

public class CurriculumManager : MonoBehaviour
{
    public static CurriculumManager Instance { get; private set; }

    [Header("Curriculum Settings")]
    public int   startSize           = 3;
    public int   maxSize             = 10;
    public int   windowSize          = 1000;
    public float advanceThreshold    = 0.7f;
    public float downgradeThreshold  = 0.3f;

    public int CurrentMazeSize { get; private set; }

    private int episodeCount = 0;
    private int successCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentMazeSize = startSize;
    }

    public void NotifyEpisodeEnd(bool success)
    {
        episodeCount++;
        if (success) successCount++;

        if (episodeCount % windowSize == 0)
        {
            float successRate = (float)successCount / windowSize;
            successCount = 0;

            Debug.Log($"[Curriculum] Size: {CurrentMazeSize}x{CurrentMazeSize} | Success: {successRate:P0} | Episodes: {episodeCount}");

            if (successRate >= advanceThreshold && CurrentMazeSize < maxSize)
            {
                CurrentMazeSize = Mathf.Min(CurrentMazeSize + 1, maxSize);
                Debug.Log($"[Curriculum] Advancing to {CurrentMazeSize}x{CurrentMazeSize}");
            }
            else if (successRate < downgradeThreshold && CurrentMazeSize > startSize)
            {
                CurrentMazeSize = Mathf.Max(CurrentMazeSize - 1, startSize);
                Debug.Log($"[Curriculum] Downgrading to {CurrentMazeSize}x{CurrentMazeSize}");
            }
        }
    }
}