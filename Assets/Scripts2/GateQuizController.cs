using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateQuizController : MonoBehaviour
{
    [Header("Quiz")]
    public QuizManager quizManager;

    [Header("Doors")]
    public QuizDoorController[] doors = new QuizDoorController[3];

    [Header("Wall Fail")]
    public WallFailTrigger[] wallFails;

    [Header("Mission Setting")]
    public bool setupOnStart = true;

    [Tooltip("정답 통과 후 시간으로 문을 리셋할지 여부. RoadManager 재사용 리셋을 쓰면 false 추천.")]
    public bool resetAfterSuccessDelay = false;

    public float resetDelay = 1.5f;

    [Header("Text Setting")]
    public string luckText = "LUCK";
    public string leftArrowText = "←";
    public string rightArrowText = "→";

    [Header("Luck Quiz Fixed Layout")]
    public bool useFixedLuckQuizLayout = true;

    [Header("Fail Impact")]
    public bool useFailImpactPush = true;
    public Vector3 failImpactDirectionLocal = new Vector3(0f, 0f, 1f);
    public float failImpactDistance = 0.35f;
    public float failImpactDuration = 0.12f;

    [Header("Debug")]
    public bool showDebugLog = true;

    private bool isResolved = false;
    private Coroutine resetCoroutine;
    private Coroutine impactCoroutine;

    private Vector3 originalLocalPosition;

    void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    void Start()
    {
        AutoFindReferences();

        if (setupOnStart == true)
        {
            SetupNewGateMission();
        }
    }

    void AutoFindReferences()
    {
        if (quizManager == null)
        {
            quizManager = FindFirstObjectByType<QuizManager>();
        }

        AutoFindDoors();
        AutoFindWallFails();
    }

    void AutoFindDoors()
    {
        bool needAutoFind = false;

        if (doors == null || doors.Length != 3)
        {
            needAutoFind = true;
        }
        else
        {
            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] == null)
                {
                    needAutoFind = true;
                    break;
                }
            }
        }

        if (needAutoFind == false)
            return;

        QuizDoorController[] foundDoors = GetComponentsInChildren<QuizDoorController>(true);

        List<QuizDoorController> doorList = new List<QuizDoorController>(foundDoors);

        doorList.Sort((a, b) => a.doorIndex.CompareTo(b.doorIndex));

        doors = new QuizDoorController[3];

        for (int i = 0; i < doors.Length; i++)
        {
            if (i < doorList.Count)
            {
                doors[i] = doorList[i];
            }
        }
    }

    void AutoFindWallFails()
    {
        if (wallFails != null && wallFails.Length > 0)
            return;

        wallFails = GetComponentsInChildren<WallFailTrigger>(true);
    }

    public void SetupNewGateMission()
    {
        isResolved = false;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        if (impactCoroutine != null)
        {
            StopCoroutine(impactCoroutine);
            impactCoroutine = null;
        }

        transform.localPosition = originalLocalPosition;

        ResetWallFails();

        if (quizManager == null)
        {
            Debug.LogWarning("[GateQuizController] QuizManager가 없습니다.");
            return;
        }

        QuizData quizData = quizManager.CreateNewQuiz();

        if (quizData == null)
        {
            Debug.LogWarning("[GateQuizController] QuizData 생성 실패");
            return;
        }

        if (quizData.quizType == QuizType.Luck && useFixedLuckQuizLayout == true)
        {
            ApplyFixedLuckQuizDoors();

            if (showDebugLog == true)
            {
                Debug.Log("[GateQuizController] 운 테스트 고정 배치 세팅 완료: " + quizData.questionText);
            }

            return;
        }

        List<QuizDoorSetupData> doorSetupList = CreateDoorSetupList(quizData);
        ShuffleDoorSetupList(doorSetupList);
        ApplyDoorSetupList(doorSetupList);

        if (showDebugLog == true)
        {
            Debug.Log("[GateQuizController] 새 미션 세팅 완료: " + quizData.questionText);
        }
    }

    List<QuizDoorSetupData> CreateDoorSetupList(QuizData quizData)
    {
        List<QuizDoorSetupData> result = new List<QuizDoorSetupData>();

        if (quizData.quizType == QuizType.Luck)
        {
            CreateLuckQuizDoors(result);
        }
        else
        {
            CreateNumberQuizDoors(result, quizData);
        }

        return result;
    }

    void CreateNumberQuizDoors(List<QuizDoorSetupData> result, QuizData quizData)
    {
        QuizDoorSetupData answerDoor = new QuizDoorSetupData(
            QuizDoorController.DoorResultType.Answer,
            quizData.answer.ToString(),
            true,
            true
        );

        QuizDoorSetupData wrongDoor = new QuizDoorSetupData(
            QuizDoorController.DoorResultType.Wrong,
            quizData.wrongAnswer.ToString(),
            false,
            true
        );

        QuizDoorSetupData specialDoor = CreateSpecialDoor();

        result.Add(answerDoor);
        result.Add(wrongDoor);
        result.Add(specialDoor);
    }

    void CreateLuckQuizDoors(List<QuizDoorSetupData> result)
    {
        bool passIsLeftArrow = Random.Range(0, 2) == 0;

        string passText = passIsLeftArrow ? leftArrowText : rightArrowText;
        string failText = passIsLeftArrow ? rightArrowText : leftArrowText;

        QuizDoorSetupData luckPassDoor = new QuizDoorSetupData(
            QuizDoorController.DoorResultType.Luck,
            passText,
            true,
            true
        );

        QuizDoorSetupData luckFailDoor = new QuizDoorSetupData(
            QuizDoorController.DoorResultType.Luck,
            failText,
            false,
            true
        );

        QuizDoorSetupData closedDoor = new QuizDoorSetupData(
            QuizDoorController.DoorResultType.Closed,
            "",
            false,
            false
        );

        result.Add(luckPassDoor);
        result.Add(luckFailDoor);
        result.Add(closedDoor);
    }

    void ApplyFixedLuckQuizDoors()
    {
        if (doors == null || doors.Length < 3)
        {
            Debug.LogWarning("[GateQuizController] 운 테스트 고정 배치 실패: Door 배열이 부족합니다.");
            return;
        }

        bool leftIsSuccess = Random.Range(0, 2) == 0;

        QuizDoorSetupData leftDoor = new QuizDoorSetupData(
            QuizDoorController.DoorResultType.Luck,
            leftArrowText,
            leftIsSuccess,
            true
        );

        QuizDoorSetupData centerDoor = new QuizDoorSetupData(
            QuizDoorController.DoorResultType.Closed,
            "",
            false,
            false
        );

        QuizDoorSetupData rightDoor = new QuizDoorSetupData(
            QuizDoorController.DoorResultType.Luck,
            rightArrowText,
            !leftIsSuccess,
            true
        );

        ApplyDoorSetupToDoor(0, leftDoor);
        ApplyDoorSetupToDoor(1, centerDoor);
        ApplyDoorSetupToDoor(2, rightDoor);

        if (showDebugLog == true)
        {
            Debug.Log(
                "[GateQuizController] 운 테스트 고정 배치 / Left Success: " +
                leftIsSuccess +
                " / Right Success: " +
                (!leftIsSuccess)
            );
        }
    }

    QuizDoorSetupData CreateSpecialDoor()
    {
        int randomValue = Random.Range(0, 2);

        if (randomValue == 0)
        {
            bool luckSuccess = Random.Range(0, 2) == 0;

            return new QuizDoorSetupData(
                QuizDoorController.DoorResultType.Luck,
                luckText,
                luckSuccess,
                true
            );
        }
        else
        {
            return new QuizDoorSetupData(
                QuizDoorController.DoorResultType.Closed,
                "",
                false,
                false
            );
        }
    }

    void ShuffleDoorSetupList(List<QuizDoorSetupData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            QuizDoorSetupData temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void ApplyDoorSetupList(List<QuizDoorSetupData> doorSetupList)
    {
        if (doors == null || doors.Length == 0)
        {
            Debug.LogWarning("[GateQuizController] Door 배열이 비어 있습니다.");
            return;
        }

        for (int i = 0; i < doors.Length; i++)
        {
            if (i >= doorSetupList.Count)
                continue;

            ApplyDoorSetupToDoor(i, doorSetupList[i]);
        }
    }

    void ApplyDoorSetupToDoor(int doorIndex, QuizDoorSetupData setupData)
    {
        if (doors == null)
            return;

        if (doorIndex < 0 || doorIndex >= doors.Length)
            return;

        if (doors[doorIndex] == null)
            return;

        doors[doorIndex].doorIndex = doorIndex;

        doors[doorIndex].SetupDoor(
            setupData.doorResultType,
            setupData.displayText,
            setupData.isSuccessDoor,
            setupData.canOpen
        );

        if (showDebugLog == true)
        {
            Debug.Log(
                "[GateQuizController] Door " + doorIndex +
                " / Type: " + setupData.doorResultType +
                " / Text: " + setupData.displayText +
                " / Success: " + setupData.isSuccessDoor
            );
        }
    }

    public void CheckQuizDoorHit(QuizDoorController selectedDoor)
    {
        if (isResolved == true)
            return;

        if (selectedDoor == null)
            return;

        isResolved = true;

        if (showDebugLog == true)
        {
            Debug.Log(
                "[GateQuizController] 선택한 문: " +
                selectedDoor.name +
                " / Type: " +
                selectedDoor.GetDoorResultType() +
                " / Success: " +
                selectedDoor.IsSuccessDoor()
            );
        }

        if (selectedDoor.IsSuccessDoor() == true)
        {
            HandleSuccessDoor(selectedDoor);
        }
        else
        {
            HandleFailDoor(selectedDoor);
        }
    }

    void HandleSuccessDoor(QuizDoorController selectedDoor)
    {
        if (selectedDoor.CanOpen() == true)
        {
            selectedDoor.OpenDoor();
        }

        if (LuckyRunGameManager.instance != null)
        {
            LuckyRunGameManager.instance.AddGateCount();
        }

        if (resetAfterSuccessDelay == true)
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
            }

            resetCoroutine = StartCoroutine(ResetAfterDelay());
        }
    }

    void HandleFailDoor(QuizDoorController selectedDoor)
    {
        Debug.Log("[GateQuizController] 실패: 잘못된 문 선택 / " + selectedDoor.name);

        PlayFailImpact();
        CallGameOver();
    }

    public void FailGateByWall()
    {
        if (isResolved == true)
            return;

        isResolved = true;

        Debug.Log("[GateQuizController] 실패: 문 사이 벽 충돌");

        PlayFailImpact();
        CallGameOver();
    }

    void PlayFailImpact()
    {
        if (useFailImpactPush == false)
            return;

        if (impactCoroutine != null)
        {
            StopCoroutine(impactCoroutine);
        }

        impactCoroutine = StartCoroutine(FailImpactRoutine());
    }

    IEnumerator FailImpactRoutine()
    {
        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition =
            startPosition + failImpactDirectionLocal.normalized * failImpactDistance;

        float timer = 0f;

        while (timer < failImpactDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / failImpactDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.localPosition = targetPosition;
    }

    void CallGameOver()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        if (LuckyRunGameManager.instance != null)
        {
            LuckyRunGameManager.instance.GameOver();
        }
        else
        {
            Time.timeScale = 0f;
        }
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        SetupNewGateMission();
    }

    void ResetWallFails()
    {
        if (wallFails == null)
            return;

        for (int i = 0; i < wallFails.Length; i++)
        {
            if (wallFails[i] != null)
            {
                wallFails[i].ResetWallFail();
            }
        }
    }
}

[System.Serializable]
public class QuizDoorSetupData
{
    public QuizDoorController.DoorResultType doorResultType;
    public string displayText;
    public bool isSuccessDoor;
    public bool canOpen;

    public QuizDoorSetupData(
        QuizDoorController.DoorResultType newDoorResultType,
        string newDisplayText,
        bool newIsSuccessDoor,
        bool newCanOpen
    )
    {
        doorResultType = newDoorResultType;
        displayText = newDisplayText;
        isSuccessDoor = newIsSuccessDoor;
        canOpen = newCanOpen;
    }
}