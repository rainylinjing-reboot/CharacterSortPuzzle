using UnityEngine;

public class LuckyRunDifficultyManager : MonoBehaviour
{
    public static LuckyRunDifficultyManager instance;

    [Header("References")]
    public RoadManager roadManager;

    [Header("Current Difficulty")]
    public int difficultyPassCount = 0;
    public int currentLevel = 1;

    [Header("Level 1 / Difficulty Count 0~4")]
    public float level1RoadSpeed = 5f;
    public int level1OneDigitChancePercent = 100;
    public int level1LuckSuccessPercent = 90;
    public int level1MaxConsecutiveLuckQuiz = 1;

    [Header("Level 2 / Difficulty Count 5~9")]
    public float level2RoadSpeed = 6f;
    public int level2OneDigitChancePercent = 60;
    public int level2LuckSuccessPercent = 75;
    public int level2MaxConsecutiveLuckQuiz = 2;

    [Header("Level 3 / Difficulty Count 10~14")]
    public float level3RoadSpeed = 7f;
    public int level3OneDigitChancePercent = 35;
    public int level3LuckSuccessPercent = 50;
    public int level3MaxConsecutiveLuckQuiz = 3;

    [Header("Level 4 / Difficulty Count 15+")]
    public float level4RoadSpeed = 8f;
    public int level4OneDigitChancePercent = 15;
    public int level4LuckSuccessPercent = 25;
    public int level4MaxConsecutiveLuckQuiz = 3;

    [Header("Number Range")]
    public int oneDigitMin = 1;
    public int oneDigitMax = 9;
    public int twoDigitMin = 10;
    public int twoDigitMax = 19;

    [Header("Debug")]
    public bool showDebugLog = true;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        AutoFindReferences();
        UpdateDifficulty(0);
    }

    void AutoFindReferences()
    {
        if (roadManager == null)
        {
            roadManager = FindFirstObjectByType<RoadManager>();
        }
    }

    public void UpdateDifficulty(int newDifficultyPassCount)
    {
        difficultyPassCount = newDifficultyPassCount;

        int previousLevel = currentLevel;
        currentLevel = CalculateLevel(difficultyPassCount);

        ApplyRoadSpeed();

        if (showDebugLog == true && previousLevel != currentLevel)
        {
            Debug.Log("[LuckyRunDifficultyManager] 난이도 레벨 변경: " + previousLevel + " → " + currentLevel);
        }

        if (showDebugLog == true)
        {
            Debug.Log(
                "[LuckyRunDifficultyManager] Difficulty Count: " +
                difficultyPassCount +
                " / Level: " +
                currentLevel +
                " / RoadSpeed: " +
                GetCurrentRoadSpeed()
            );
        }
    }

    int CalculateLevel(int count)
    {
        if (count < 5)
            return 1;

        if (count < 10)
            return 2;

        if (count < 15)
            return 3;

        return 4;
    }

    void ApplyRoadSpeed()
    {
        if (roadManager == null)
            return;

        roadManager.roadSpeed = GetCurrentRoadSpeed();
    }

    public float GetCurrentRoadSpeed()
    {
        if (currentLevel == 1)
            return level1RoadSpeed;

        if (currentLevel == 2)
            return level2RoadSpeed;

        if (currentLevel == 3)
            return level3RoadSpeed;

        return level4RoadSpeed;
    }

    public int GetCurrentOneDigitChancePercent()
    {
        if (currentLevel == 1)
            return level1OneDigitChancePercent;

        if (currentLevel == 2)
            return level2OneDigitChancePercent;

        if (currentLevel == 3)
            return level3OneDigitChancePercent;

        return level4OneDigitChancePercent;
    }

    public int GetCurrentTwoDigitChancePercent()
    {
        return 100 - GetCurrentOneDigitChancePercent();
    }

    public int GetCurrentLuckSuccessPercent()
    {
        if (currentLevel == 1)
            return level1LuckSuccessPercent;

        if (currentLevel == 2)
            return level2LuckSuccessPercent;

        if (currentLevel == 3)
            return level3LuckSuccessPercent;

        return level4LuckSuccessPercent;
    }

    public int GetCurrentMaxConsecutiveLuckQuiz()
    {
        if (currentLevel == 1)
            return level1MaxConsecutiveLuckQuiz;

        if (currentLevel == 2)
            return level2MaxConsecutiveLuckQuiz;

        if (currentLevel == 3)
            return level3MaxConsecutiveLuckQuiz;

        return level4MaxConsecutiveLuckQuiz;
    }

    public int GetOneDigitNumber()
    {
        return Random.Range(oneDigitMin, oneDigitMax + 1);
    }

    public int GetTwoDigitNumber()
    {
        return Random.Range(twoDigitMin, twoDigitMax + 1);
    }

    public bool ShouldUseOneDigitQuestion()
    {
        int chance = GetCurrentOneDigitChancePercent();
        return Random.Range(0, 100) < chance;
    }

    public bool IsLuckSuccess()
    {
        int chance = GetCurrentLuckSuccessPercent();
        return Random.Range(0, 100) < chance;
    }
}