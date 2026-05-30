using UnityEngine;

public class DoorHitTrigger : MonoBehaviour
{
    public GateController gateController;
    public GateController.DoorSide doorSide;

    void Start()
    {
        if (gateController == null)
        {
            gateController = GetComponentInParent<GateController>();
        }

        if (gateController == null)
        {
            Debug.LogWarning("[DoorHitTrigger] GateController를 찾지 못했습니다.");
            return;
        }

        AutoAssignDoorSide();
    }

    void OnTriggerEnter(Collider other)
    {
        if (gateController == null)
            return;

        gateController.CheckDoorHit(other, doorSide);
    }

    void AutoAssignDoorSide()
    {
        if (IsSameDoor(gateController.leftDoorRotate))
        {
            doorSide = GateController.DoorSide.Left;
            return;
        }

        if (IsSameDoor(gateController.rightDoorRotate))
        {
            doorSide = GateController.DoorSide.Right;
            return;
        }

        Debug.LogWarning("[DoorHitTrigger] 문 방향을 자동으로 찾지 못했습니다: " + name);
    }

    bool IsSameDoor(Transform doorRotate)
    {
        if (doorRotate == null)
            return false;

        return transform == doorRotate || transform.IsChildOf(doorRotate) || doorRotate.IsChildOf(transform);
    }
}