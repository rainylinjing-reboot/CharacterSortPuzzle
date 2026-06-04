using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LuckyRunGameManager : MonoBehaviour
{
    public static LuckyRunGameManager instance;

    [Header("Gate Count")]
    public int gateCount = 0;
    public int difficultyPassCount = 0;

    [Header("UI")]
    public TextMeshProUGUI gateCountText;
    public TextMeshProUGUI levelText;
    public GameObject retryButtonObject;
    public Button retryButton;

    [Header("Difficulty")]
    public LuckyRunDifficultyManager difficultyManager;

    [Header("Fail Effect")]
    public PlayerFailController playerFailController;
    public FailCameraEffect failCameraEffect;
    public float gameOverFreezeDelay = 1.2f;

    [Header("World Movement")]
    public RoadManager roadManager;
    public bool stopRoadOnGameOver = true;

    [Header("Retry")]
    public bool reloadSceneOnRetry = true;
    public float retryReloadDelay = 0.1f;

    [Header("Debug")]
    public bool showDebugLog = true;

    private bool isGameOver = false;
    private bool isRetrying = false;
    private Coroutine gameOverCoroutine;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        AutoFindReferences();
        SetupRetryButton();
        HideRetryButton();

        UpdateDifficulty();
        UpdateGateCountUI();
        UpdateLevelUI();
    }

    void AutoFindReferences()
    {
        if (difficultyManager == null)
        {
            difficultyManager = FindFirstObjectByType<LuckyRunDifficultyManager>();
        }

        if (playerFailController == null)
        {
            playerFailController = FindFirstObjectByType<PlayerFailController>();
        }

        if (failCameraEffect == null)
        {
            failCameraEffect = FindFirstObjectByType<FailCameraEffect>();
        }

        if (roadManager == null)
        {
            roadManager = FindFirstObjectByType<RoadManager>();
        }

        if (retryButtonObject == null)
        {
            GameObject foundRetry = GameObject.Find("RetryButton");

            if (foundRetry != null)
            {
                retryButtonObject = foundRetry;
            }
        }

        if (retryButton == null && retryButtonObject != null)
        {
            retryButton = retryButtonObject.GetComponent<Button>();

            if (retryButton == null)
            {
                retryButton = retryButtonObject.GetComponentInChildren<Button>();
            }
        }

        if (levelText == null)
        {
            GameObject foundLevelText = GameObject.Find("LevelText");

            if (foundLevelText != null)
            {
                levelText = foundLevelText.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    void SetupRetryButton()
    {
        if (retryButton == null)
            return;

        retryButton.onClick.RemoveListener(RetryGame);
        retryButton.onClick.AddListener(RetryGame);
    }

    public void AddGateCount()
    {
        AddGateCount(true);
    }

    public void AddGateCount(bool countForDifficulty)
    {
        if (isGameOver == true)
            return;

        gateCount++;

        if (countForDifficulty == true)
        {
            difficultyPassCount++;
            UpdateDifficulty();
        }

        if (showDebugLog == true)
        {
            Debug.Log(
                "[LuckyRunGameManager] Pass: " +
                gateCount +
                " / Difficulty Count: " +
                difficultyPassCount
            );
        }

        UpdateGateCountUI();
        UpdateLevelUI();
    }

    void UpdateDifficulty()
    {
        if (difficultyManager != null)
        {
            difficultyManager.UpdateDifficulty(difficultyPassCount);
        }
    }

    void UpdateGateCountUI()
    {
        if (gateCountText != null)
        {
            gateCountText.text = "Pass:" + gateCount;
        }
    }

    void UpdateLevelUI()
    {
        if (levelText == null)
            return;

        int level = GetCurrentLevel();

        levelText.text = "Level:" + level;
    }

    int GetCurrentLevel()
    {
        if (difficultyManager != null)
        {
            return difficultyManager.currentLevel;
        }

        if (difficultyPassCount < 5)
            return 1;

        if (difficultyPassCount < 10)
            return 2;

        if (difficultyPassCount < 15)
            return 3;

        return 4;
    }

    public void GameOver()
    {
        if (isGameOver == true)
            return;

        isGameOver = true;

        if (showDebugLog == true)
        {
            Debug.Log("[LuckyRunGameManager] Game Over 시작");
        }

        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
        }

        gameOverCoroutine = StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        Time.timeScale = 1f;

        StopWorldMovement();

        if (LuckyRunSoundManager.instance != null)
        {
            LuckyRunSoundManager.instance.StopRunSound();
            LuckyRunSoundManager.instance.PlayHitSound();
        }

        if (playerFailController != null)
        {
            playerFailController.PlayDie();
        }

        if (failCameraEffect != null)
        {
            failCameraEffect.PlayFailEffect();
        }

        yield return new WaitForSecondsRealtime(gameOverFreezeDelay);

        ShowRetryButton();

        Time.timeScale = 0f;

        if (showDebugLog == true)
        {
            Debug.Log("[LuckyRunGameManager] Game Over 정지");
        }
    }

    void StopWorldMovement()
    {
        if (stopRoadOnGameOver == false)
            return;

        if (roadManager != null)
        {
            roadManager.enabled = false;

            if (showDebugLog == true)
            {
                Debug.Log("[LuckyRunGameManager] RoadManager 정지");
            }
        }
    }

    public void ShowRetryButton()
    {
        if (retryButtonObject != null)
        {
            retryButtonObject.SetActive(true);
        }
    }

    public void HideRetryButton()
    {
        if (retryButtonObject != null)
        {
            retryButtonObject.SetActive(false);
        }
    }

    public void RetryGame()
    {
        if (isRetrying == true)
            return;

        isRetrying = true;

        if (showDebugLog == true)
        {
            Debug.Log("[LuckyRunGameManager] Retry 실행");
        }

        if (retryButton != null)
        {
            retryButton.interactable = false;
        }

        StartCoroutine(RetryRoutine());
    }

    IEnumerator RetryRoutine()
    {
        Time.timeScale = 1f;

        if (LuckyRunSoundManager.instance != null)
        {
            LuckyRunSoundManager.instance.PlayRetrySound();
        }

        yield return new WaitForSecondsRealtime(retryReloadDelay);

        if (reloadSceneOnRetry == true)
        {
            ReloadCurrentScene();
        }
        else
        {
            ResetGameWithoutSceneReload();
        }
    }

    void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    void ResetGameWithoutSceneReload()
    {
        isGameOver = false;
        isRetrying = false;
        gateCount = 0;
        difficultyPassCount = 0;

        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }

        if (playerFailController != null)
        {
            playerFailController.ResetFailState();
        }

        if (failCameraEffect != null)
        {
            failCameraEffect.ResetCamera();
        }

        if (roadManager != null)
        {
            roadManager.enabled = true;
        }

        if (LuckyRunSoundManager.instance != null)
        {
            LuckyRunSoundManager.instance.PlayRunSound();
        }

        if (retryButton != null)
        {
            retryButton.interactable = true;
        }

        HideRetryButton();

        UpdateDifficulty();
        UpdateGateCountUI();
        UpdateLevelUI();

        if (showDebugLog == true)
        {
            Debug.Log("[LuckyRunGameManager] 씬 리로드 없는 게임 리셋");
        }
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }
}