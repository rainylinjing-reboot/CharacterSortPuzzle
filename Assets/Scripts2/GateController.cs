using System.Collections;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public enum DoorSide
    {
        Left,
        Right
    }

    [Header("Door Rotate Object")]
    public Transform leftDoorRotate;
    public Transform rightDoorRotate;

    [Header("Open Setting")]
    public float openAngle = 120f;
    public float openSpeed = 4f;

    [Header("Closed Rotation")]
    public Vector3 leftClosedEuler = new Vector3(0f, 90f, 0f);
    public Vector3 rightClosedEuler = new Vector3(0f, 270f, 0f);

    [Header("Reset Setting")]
    public float resetDelay = 2.5f;

    [Header("Answer Setting")]
    public DoorSide correctDoor;
    public bool randomCorrectDoor = true;

    [Header("Player")]
    public string playerTag = "Player";

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    private bool isTriggered = false;
    private bool isOpening = false;
    private bool isInitialized = false;

    private Coroutine openCoroutine;
    private Coroutine resetCoroutine;

    void Awake()
    {
        InitializeClosedRotation();
    }

    void Start()
    {
        Time.timeScale = 1f;

        ResetGate();

        Debug.Log("[GateController] Left Door Rotate = " + GetObjectName(leftDoorRotate));
        Debug.Log("[GateController] Right Door Rotate = " + GetObjectName(rightDoorRotate));
    }

    string GetObjectName(Transform target)
    {
        if (target == null)
            return "None";

        return target.name;
    }

    void InitializeClosedRotation()
    {
        leftClosedRotation = Quaternion.Euler(leftClosedEuler);
        rightClosedRotation = Quaternion.Euler(rightClosedEuler);
        isInitialized = true;
    }

    void SetCorrectDoor()
    {
        if (randomCorrectDoor == false)
        {
            Debug.Log("[GateController] 정답 문 고정: " + correctDoor);
            return;
        }

        int randomValue = Random.Range(0, 2);

        if (randomValue == 0)
        {
            correctDoor = DoorSide.Left;
        }
        else
        {
            correctDoor = DoorSide.Right;
        }

        Debug.Log("[GateController] 랜덤 정답 문: " + correctDoor);
    }

    // DoorHitTrigger가 호출하는 함수
    public void CheckDoorHit(Collider other, DoorSide selectedDoor)
    {
        if (isTriggered == true)
            return;

        if (other.CompareTag(playerTag) == false)
            return;

        isTriggered = true;

        Debug.Log("[GateController] 플레이어가 닿은 문: " + selectedDoor);
        Debug.Log("[GateController] 실제 정답 문: " + correctDoor);

        if (selectedDoor == correctDoor)
        {
            OpenSelectedDoor(selectedDoor);
        }
        else
        {
            FailGate(selectedDoor);
        }
    }

    void OpenSelectedDoor(DoorSide selectedDoor)
    {
        if (isOpening == true)
            return;

        isOpening = true;

        // 문 통과 카운트 증가
        if (LuckyRunGameManager.instance != null)
        {
            LuckyRunGameManager.instance.AddGateCount();
        }

        if (openCoroutine != null)
        {
            StopCoroutine(openCoroutine);
            openCoroutine = null;
        }

        if (selectedDoor == DoorSide.Left)
        {
            Debug.Log("[GateController] 왼쪽 문 열기 실행: " + GetObjectName(leftDoorRotate));
            openCoroutine = StartCoroutine(OpenDoor(leftDoorRotate, leftClosedRotation, -openAngle));
        }
        else
        {
            Debug.Log("[GateController] 오른쪽 문 열기 실행: " + GetObjectName(rightDoorRotate));
            openCoroutine = StartCoroutine(OpenDoor(rightDoorRotate, rightClosedRotation, openAngle));
        }

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        resetCoroutine = StartCoroutine(ResetGateAfterDelay());
    }

    IEnumerator OpenDoor(Transform doorRotate, Quaternion closedRotation, float angle)
    {
        if (doorRotate == null)
        {
            Debug.LogWarning("[GateController] 열 문이 비어 있습니다.");
            yield break;
        }

        Quaternion targetRotation = closedRotation * Quaternion.Euler(0f, angle, 0f);

        while (Quaternion.Angle(doorRotate.localRotation, targetRotation) > 0.5f)
        {
            doorRotate.localRotation = Quaternion.Slerp(
                doorRotate.localRotation,
                targetRotation,
                openSpeed * Time.deltaTime
            );

            yield return null;
        }

        doorRotate.localRotation = targetRotation;
    }

    IEnumerator ResetGateAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        ResetGate();
    }

    public void ResetGate()
    {
        if (isInitialized == false)
        {
            InitializeClosedRotation();
        }

        if (openCoroutine != null)
        {
            StopCoroutine(openCoroutine);
            openCoroutine = null;
        }

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        if (leftDoorRotate != null)
        {
            leftDoorRotate.localRotation = leftClosedRotation;
        }

        if (rightDoorRotate != null)
        {
            rightDoorRotate.localRotation = rightClosedRotation;
        }

        isTriggered = false;
        isOpening = false;

        SetCorrectDoor();

        Debug.Log("[GateController] Gate 리셋 완료 / 새 정답 문: " + correctDoor);
    }

    void FailGate(DoorSide selectedDoor)
    {
        Debug.Log("[GateController] 실패: 닫힌 문 선택");
        Debug.Log("[GateController] 선택한 문: " + selectedDoor + " / 정답 문: " + correctDoor);

        if (LuckyRunGameManager.instance != null)
        {
            LuckyRunGameManager.instance.ShowRetryButton();
        }

        Time.timeScale = 0f;
    }
}
