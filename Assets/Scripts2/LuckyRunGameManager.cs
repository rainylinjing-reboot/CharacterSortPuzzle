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

    [Header("UI")]
    public TextMeshProUGUI gateCountText;
    public GameObject retryButtonObject;
    public Button retryButton;

    [Header("Fail Effect")]
    public PlayerFailController playerFailController;
    public FailCameraEffect failCameraEffect;
    public float gameOverFreezeDelay = 1.2f;

    [Header("World Movement")]
    public RoadManager roadManager;
    public bool stopRoadOnGameOver = true;

    [Header("Retry")]
    public bool reloadSceneOnRetry = true;

    [Header("Debug")]
    public bool showDebugLog = true;

    private bool isGameOver = false;
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
        UpdateGateCountUI();
    }

    void AutoFindReferences()
    {
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
        if (isGameOver == true)
            return;

        gateCount++;

        if (showDebugLog == true)
        {
            Debug.Log("[LuckyRunGameManager] 통과한 문 개수: " + gateCount);
        }

        UpdateGateCountUI();
    }

    void UpdateGateCountUI()
    {
        if (gateCountText != null)
        {
            gateCountText.text = "Pass:" + gateCount;
        }
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
        if (showDebugLog == true)
        {
            Debug.Log("[LuckyRunGameManager] Retry 실행");
        }

        Time.timeScale = 1f;

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
        gateCount = 0;

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

        HideRetryButton();
        UpdateGateCountUI();

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