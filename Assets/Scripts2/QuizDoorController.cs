using System.Collections;
using TMPro;
using UnityEngine;

public class QuizDoorController : MonoBehaviour
{
    public enum DoorResultType
    {
        Answer,
        Wrong,
        Luck,
        Closed
    }

    [Header("Door Info")]
    public int doorIndex = 0;
    public DoorResultType doorResultType;
    public string displayText = "";
    public bool isSuccessDoor = false;
    public bool canOpen = true;

    [Header("Gate")]
    public GameObject gateControllerObject;

    [Header("Door Object")]
    public Transform doorModelRoot;
    public Transform doorRotatePivot;
    public TextMeshPro doorText;
    public BoxCollider doorHitCollider;

    [Header("Door Model Prefabs")]
    public GameObject[] openDoorPrefabs;
    public GameObject[] closedDoorPrefabs;

    [Header("Open Setting")]
    public float openAngle = 120f;
    public float openSpeed = 4f;
    public float openDirection = 1f;

    [Header("Player")]
    public string playerTag = "Player";

    private GameObject currentDoorModel;
    private Quaternion closedRotation;

    private bool isTriggered = false;
    private bool isOpened = false;

    private Coroutine openCoroutine;

    void Start()
    {
        AutoFindGateController();
        AutoFindComponents();
        SaveClosedRotation();

        // 테스트용 기본 세팅
        ApplyDoorText();
    }

    void AutoFindGateController()
    {
        if (gateControllerObject != null)
            return;

        Transform current = transform;

        while (current != null)
        {
            if (current.name.Contains("Gate"))
            {
                gateControllerObject = current.gameObject;
                break;
            }

            current = current.parent;
        }

        if (gateControllerObject == null)
        {
            Debug.LogWarning("[QuizDoorController] Gate Controller Object를 찾지 못했습니다: " + name);
        }
    }

    void AutoFindComponents()
    {
        if (doorModelRoot == null)
        {
            Transform found = transform.Find("DoorModelRoot");

            if (found != null)
            {
                doorModelRoot = found;
            }
        }

        if (doorText == null)
        {
            doorText = GetComponentInChildren<TextMeshPro>();
        }

        if (doorHitCollider == null)
        {
            Transform found = transform.Find("DoorHitCollider");

            if (found != null)
            {
                doorHitCollider = found.GetComponent<BoxCollider>();
            }
        }

        if (doorHitCollider == null)
        {
            doorHitCollider = GetComponentInChildren<BoxCollider>();
        }

        if (doorHitCollider != null)
        {
            doorHitCollider.isTrigger = true;
        }
    }

    void SaveClosedRotation()
    {
        if (doorRotatePivot != null)
        {
            closedRotation = doorRotatePivot.localRotation;
        }
    }

    public void SetupDoor(
        DoorResultType newDoorResultType,
        string newDisplayText,
        bool newIsSuccessDoor,
        bool newCanOpen
    )
    {
        doorResultType = newDoorResultType;
        displayText = newDisplayText;
        isSuccessDoor = newIsSuccessDoor;
        canOpen = newCanOpen;

        ResetDoor();
        ApplyDoorText();
        SpawnDoorModelByType();

        Debug.Log(
            "[QuizDoorController] Door 세팅: " +
            name +
            " / Type: " +
            doorResultType +
            " / Text: " +
            displayText +
            " / Success: " +
            isSuccessDoor
        );
    }

    public void SetupDoorWithPrefab(
        DoorResultType newDoorResultType,
        string newDisplayText,
        bool newIsSuccessDoor,
        bool newCanOpen,
        GameObject doorPrefab
    )
    {
        doorResultType = newDoorResultType;
        displayText = newDisplayText;
        isSuccessDoor = newIsSuccessDoor;
        canOpen = newCanOpen;

        ResetDoor();
        ApplyDoorText();
        SpawnDoorModel(doorPrefab);

        Debug.Log(
            "[QuizDoorController] Door 프리팹 세팅: " +
            name +
            " / Type: " +
            doorResultType +
            " / Text: " +
            displayText
        );
    }

    void ApplyDoorText()
    {
        if (doorText == null)
            return;

        if (doorResultType == DoorResultType.Closed)
        {
            doorText.text = "";
        }
        else
        {
            doorText.text = displayText;
        }
    }

    void SpawnDoorModelByType()
    {
        GameObject prefab = null;

        if (doorResultType == DoorResultType.Closed)
        {
            prefab = GetRandomPrefab(closedDoorPrefabs);
        }
        else
        {
            prefab = GetRandomPrefab(openDoorPrefabs);
        }

        SpawnDoorModel(prefab);
    }

    void SpawnDoorModel(GameObject prefab)
    {
        ClearCurrentDoorModel();

        if (prefab == null)
        {
            Debug.LogWarning("[QuizDoorController] 생성할 문 프리팹이 없습니다: " + name);
            return;
        }

        if (doorModelRoot == null)
        {
            Debug.LogWarning("[QuizDoorController] DoorModelRoot가 없습니다: " + name);
            return;
        }

        currentDoorModel = Instantiate(prefab, doorModelRoot);
        currentDoorModel.transform.localPosition = Vector3.zero;
        currentDoorModel.transform.localRotation = Quaternion.identity;
        currentDoorModel.transform.localScale = Vector3.one;

        FindDoorRotatePivotFromModel();

        SaveClosedRotation();
    }

    void FindDoorRotatePivotFromModel()
    {
        if (currentDoorModel == null)
            return;

        Transform foundPivot = FindChildByNameContains(currentDoorModel.transform, "rotate");

        if (foundPivot != null)
        {
            doorRotatePivot = foundPivot;
            return;
        }

        Transform foundDoor = FindChildByNameContains(currentDoorModel.transform, "door");

        if (foundDoor != null)
        {
            doorRotatePivot = foundDoor;
            return;
        }

        doorRotatePivot = currentDoorModel.transform;
    }

    Transform FindChildByNameContains(Transform parent, string keyword)
    {
        if (parent.name.ToLower().Contains(keyword.ToLower()))
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildByNameContains(parent.GetChild(i), keyword);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    GameObject GetRandomPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
            return null;

        int randomIndex = Random.Range(0, prefabs.Length);
        return prefabs[randomIndex];
    }

    void ClearCurrentDoorModel()
    {
        if (doorModelRoot == null)
            return;

        for (int i = doorModelRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(doorModelRoot.GetChild(i).gameObject);
        }

        currentDoorModel = null;
        doorRotatePivot = null;
    }

    void OnTriggerEnter(Collider other)
    {
        CheckHit(other);
    }

    public void CheckHit(Collider other)
    {
        if (isTriggered == true)
            return;

        if (other.CompareTag(playerTag) == false)
            return;

        isTriggered = true;

        Debug.Log("[QuizDoorController] 플레이어가 문 선택: " + name);

        if (gateControllerObject != null)
        {
            gateControllerObject.SendMessage(
                "CheckQuizDoorHit",
                this,
                SendMessageOptions.DontRequireReceiver
            );
        }
        else
        {
            // GateQuizController가 아직 없을 때 테스트용 임시 처리
            if (isSuccessDoor == true)
            {
                OpenDoor();
            }
            else
            {
                Time.timeScale = 0f;
            }
        }
    }

    public void OpenDoor()
    {
        if (canOpen == false)
        {
            Debug.Log("[QuizDoorController] 열 수 없는 문입니다: " + name);
            return;
        }

        if (doorRotatePivot == null)
        {
            Debug.LogWarning("[QuizDoorController] DoorRotatePivot이 없습니다: " + name);
            return;
        }

        if (isOpened == true)
            return;

        isOpened = true;

        if (openCoroutine != null)
        {
            StopCoroutine(openCoroutine);
            openCoroutine = null;
        }

        openCoroutine = StartCoroutine(OpenDoorRoutine());
    }

    IEnumerator OpenDoorRoutine()
    {
        Quaternion targetRotation = closedRotation * Quaternion.Euler(
            0f,
            openAngle * openDirection,
            0f
        );

        while (Quaternion.Angle(doorRotatePivot.localRotation, targetRotation) > 0.5f)
        {
            doorRotatePivot.localRotation = Quaternion.Slerp(
                doorRotatePivot.localRotation,
                targetRotation,
                openSpeed * Time.deltaTime
            );

            yield return null;
        }

        doorRotatePivot.localRotation = targetRotation;
    }

    public void CloseDoor()
    {
        if (openCoroutine != null)
        {
            StopCoroutine(openCoroutine);
            openCoroutine = null;
        }

        if (doorRotatePivot != null)
        {
            doorRotatePivot.localRotation = closedRotation;
        }

        isOpened = false;
    }

    public void ResetDoor()
    {
        CloseDoor();

        isTriggered = false;
        isOpened = false;
    }

    public bool IsSuccessDoor()
    {
        return isSuccessDoor;
    }

    public bool CanOpen()
    {
        return canOpen;
    }

    public DoorResultType GetDoorResultType()
    {
        return doorResultType;
    }
}