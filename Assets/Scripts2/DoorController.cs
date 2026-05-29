using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Rotate Object")]
    public Transform leftDoorRotate;
    public Transform rightDoorRotate;

    [Header("Open Setting")]
    public float openAngle = 120f;
    public float openSpeed = 4f;

    [Header("Open Option")]
    public bool openLeftDoor = true;
    public bool openRightDoor = true;

    [Header("Player")]
    public string playerTag = "Player";

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    private bool isOpened = false;

    void Start()
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

    void OnTriggerEnter(Collider other)
    {
        if (isOpened)
            return;

        if (!other.CompareTag(playerTag))
            return;

        OpenDoor();
    }

    void OpenDoor()
    {
        isOpened = true;

        if (openLeftDoor == true && leftDoorRotate != null)
        {
            StartCoroutine(OpenLeftDoor());
        }

        if (openRightDoor == true && rightDoorRotate != null)
        {
            StartCoroutine(OpenRightDoor());
        }
    }

    IEnumerator OpenLeftDoor()
    {
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