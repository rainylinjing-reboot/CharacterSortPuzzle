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

    [Header("Answer Setting")]
    public DoorSide correctDoor;
    public bool randomCorrectDoor = true;

    [Header("Player")]
    public string playerTag = "Player";

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    private bool isTriggered = false;

    void Start()
    {
        SaveClosedRotation();
        SetCorrectDoor();
    }

    void SaveClosedRotation()
    {
        if (leftDoorRotate != null)
        {
            leftClosedRotation = leftDoorRotate.localRotation;
        }

        if (rightDoorRotate != null)
        {
            rightClosedRotation = rightDoorRotate.localRotation;
        }
    }

    void SetCorrectDoor()
    {
        if (randomCorrectDoor == false)
            return;

        int randomValue = Random.Range(0, 2);

        if (randomValue == 0)
        {
            correctDoor = DoorSide.Left;
        }
        else
        {
            correctDoor = DoorSide.Right;
        }

        Debug.Log("[GateController] 정답 문: " + correctDoor);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered == true)
            return;

        if (other.CompareTag(playerTag) == false)
            return;

        isTriggered = true;

        DoorSide playerSide = CheckPlayerSide(other.transform);

        if (playerSide == correctDoor)
        {
            OpenCorrectDoor();
        }
        else
        {
            FailGate();
        }
    }

    DoorSide CheckPlayerSide(Transform player)
    {
        // Gate 기준으로 플레이어가 왼쪽에 있으면 Left, 오른쪽에 있으면 Right
        if (player.position.x < transform.position.x)
        {
            return DoorSide.Left;
        }
        else
        {
            return DoorSide.Right;
        }
    }

    void OpenCorrectDoor()
    {
        Debug.Log("[GateController] 통과 성공: " + correctDoor);

        if (correctDoor == DoorSide.Left)
        {
            StartCoroutine(OpenLeftDoor());
        }
        else
        {
            StartCoroutine(OpenRightDoor());
        }
    }

    void FailGate()
    {
        Debug.Log("[GateController] 실패: 닫힌 문 선택");

        // 일단 게임 전체 정지
        Time.timeScale = 0f;
    }

    IEnumerator OpenLeftDoor()
    {
        if (leftDoorRotate == null)
            yield break;

        Quaternion targetRotation = leftClosedRotation * Quaternion.Euler(0f, -openAngle, 0f);

        while (Quaternion.Angle(leftDoorRotate.localRotation, targetRotation) > 0.5f)
        {
            leftDoorRotate.localRotation = Quaternion.Slerp(
                leftDoorRotate.localRotation,
                targetRotation,
                openSpeed * Time.deltaTime
            );

            yield return null;
        }

        leftDoorRotate.localRotation = targetRotation;
    }

    IEnumerator OpenRightDoor()
    {
        if (rightDoorRotate == null)
            yield break;

        Quaternion targetRotation = rightClosedRotation * Quaternion.Euler(0f, openAngle, 0f);

        while (Quaternion.Angle(rightDoorRotate.localRotation, targetRotation) > 0.5f)
        {
            rightDoorRotate.localRotation = Quaternion.Slerp(
                rightDoorRotate.localRotation,
                targetRotation,
                openSpeed * Time.deltaTime
            );

            yield return null;
        }

        rightDoorRotate.localRotation = targetRotation;
    }
}