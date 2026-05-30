using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LuckyRunGameManager : MonoBehaviour
{
    public static LuckyRunGameManager instance;

    [Header("Gate Count")]
    public int gateCount = 0;

    [Header("UI")]
    public TextMeshProUGUI gateCountText;
    public Button retryButton;
    public TextMeshProUGUI retryButtonText;

    [Header("Retry Button")]
    public string retryButtonLabel = "Retry";
    public Vector2 retryButtonSize = new Vector2(260f, 90f);
    public Vector2 retryButtonPosition = new Vector2(0f, -120f);

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetupRetryButton();
        HideRetryButton();
        UpdateGateCountUI();
    }

    public void AddGateCount()
    {
        gateCount++;

        Debug.Log("[LuckyRunGameManager] 통과한 문 개수: " + gateCount);

        UpdateGateCountUI();
    }

    void UpdateGateCountUI()
    {
        if (gateCountText != null)
        {
            gateCountText.text = "Pass:" + gateCount;
        }
    }

    public void ShowRetryButton()
    {
        SetupRetryButton();

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
        }
    }

    public void HideRetryButton()
    {
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
        }
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SetupRetryButton()
    {
        if (retryButton == null)
        {
            CreateRetryButton();
        }

        if (retryButton == null)
            return;

        retryButton.onClick.RemoveListener(Retry);
        retryButton.onClick.AddListener(Retry);

        if (retryButtonText != null)
        {
            retryButtonText.text = retryButtonLabel;
        }
    }

    void CreateRetryButton()
    {
        Canvas canvas = null;

        if (gateCountText != null)
        {
            canvas = gateCountText.GetComponentInParent<Canvas>();
        }

        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            Debug.LogWarning("[LuckyRunGameManager] Retry 버튼을 만들 Canvas를 찾지 못했습니다.");
            return;
        }

        GameObject buttonObject = new GameObject(
            "RetryButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        buttonObject.layer = canvas.gameObject.layer;
        buttonObject.transform.SetParent(canvas.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = retryButtonSize;
        buttonRect.anchoredPosition = retryButtonPosition;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.12f, 0.12f, 0.12f, 0.92f);

        retryButton = buttonObject.GetComponent<Button>();

        GameObject textObject = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        textObject.layer = buttonObject.layer;
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        retryButtonText = textObject.GetComponent<TextMeshProUGUI>();
        retryButtonText.text = retryButtonLabel;
        retryButtonText.fontSize = 42f;
        retryButtonText.color = Color.white;
        retryButtonText.alignment = TextAlignmentOptions.Center;
        retryButtonText.raycastTarget = false;
    }
}
