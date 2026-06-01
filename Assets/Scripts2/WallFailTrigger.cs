using UnityEngine;

public class WallFailTrigger : MonoBehaviour
{
    [Header("Gate")]
    public GameObject gateControllerObject;

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Debug")]
    public bool stopGameIfNoGateController = true;

    private bool isTriggered = false;

    void Start()
    {
        AutoFindGateController();
    }

    void AutoFindGateController()
    {
        if (gateControllerObject != null)
            return;

        Transform current = transform;

        while (current != null)
        {
            GateQuizController gateQuizController = current.GetComponent<GateQuizController>();

            if (gateQuizController != null)
            {
                gateControllerObject = current.gameObject;
                return;
            }

            current = current.parent;
        }

        Debug.LogWarning("[WallFailTrigger] GateQuizController를 찾지 못했습니다: " + name);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered == true)
            return;

        if (other.CompareTag(playerTag) == false)
            return;

        isTriggered = true;

        Debug.Log("[WallFailTrigger] 벽 충돌 실패: " + name);

        if (gateControllerObject != null)
        {
            gateControllerObject.SendMessage(
                "FailGateByWall",
                SendMessageOptions.DontRequireReceiver
            );
        }
        else
        {
            if (stopGameIfNoGateController == true)
            {
                Time.timeScale = 0f;
            }
        }
    }

    public void ResetWallFail()
    {
        isTriggered = false;
    }
}