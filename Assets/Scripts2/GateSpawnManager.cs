using UnityEngine;

public class GateSpawnManager : MonoBehaviour
{
    [Header("Gate")]
    public GameObject gatePrefab;

    [Header("Spawn")]
    public Transform gateSpawnPoint;

    private GameObject spawnedGate;

    void Start()
    {
        SpawnGate();
    }

    void SpawnGate()
    {
        if (gatePrefab == null)
        {
            Debug.LogWarning("[GateSpawnManager] Gate Prefab이 없습니다.");
            return;
        }

        if (gateSpawnPoint == null)
        {
            Debug.LogWarning("[GateSpawnManager] Gate Spawn Point가 없습니다.");
            return;
        }

        spawnedGate = Instantiate(
            gatePrefab,
            gateSpawnPoint.position,
            gateSpawnPoint.rotation,
            gateSpawnPoint
        );

        spawnedGate.transform.localPosition = Vector3.zero;
        spawnedGate.transform.localRotation = Quaternion.identity;

        GateController gateController = spawnedGate.GetComponent<GateController>();

        if (gateController != null)
        {
            gateController.ResetGate();
        }

        Debug.Log("[GateSpawnManager] Gate 생성 완료");
    }
}