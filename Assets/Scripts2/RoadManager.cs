using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [Header("Road")]
    public GameObject roadPrefab;
    public Transform player;

    [Header("Road Setting")]
    public int initialRoadCount = 5;
    public float roadLength = 1f;
    public float recycleDistance = 12f;
    public float roadSpeed = 5f;

    [Header("Runtime")]
    public List<Transform> roads = new List<Transform>();

    void Start()
    {
        SetupRoads();
    }

    void Update()
    {
        MoveRoads();
        RecycleRoads();
    }

    void SetupRoads()
    {
        roads.Clear();

        // RoadManager 아래에 이미 배치된 길 조각 중
        // 이름이 Road로 시작하는 오브젝트만 등록
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child.name.StartsWith("Road"))
                {
                    roads.Add(child);
                }
            }

            Debug.Log("[RoadManager] 기존 길 조각 사용: " + roads.Count);
            return;
        }

        // 자식 길 조각이 없고 roadPrefab이 있으면 새로 생성
        if (roadPrefab != null)
        {
            for (int i = 0; i < initialRoadCount; i++)
            {
                Vector3 pos = transform.position;
                pos.z = i * roadLength;

                GameObject newRoad = Instantiate(roadPrefab, pos, Quaternion.identity);
                newRoad.transform.SetParent(transform);

                roads.Add(newRoad.transform);
            }

            Debug.Log("[RoadManager] 프리팹으로 길 생성: " + roads.Count);
            return;
        }

        Debug.LogWarning("[RoadManager] 사용할 길 조각이 없습니다.");
    }

    void MoveRoads()
    {
        for (int i = 0; i < roads.Count; i++)
        {
            if (roads[i] == null)
                continue;

            roads[i].position += Vector3.back * roadSpeed * Time.deltaTime;
        }
    }

    void RecycleRoads()
    {
        for (int i = 0; i < roads.Count; i++)
        {
            if (roads[i] == null)
                continue;

            float playerZ = 0f;

            if (player != null)
            {
                playerZ = player.position.z;
            }

            if (roads[i].position.z < playerZ - recycleDistance)
            {
                MoveRoadToFront(roads[i]);
            }
        }
    }

    void MoveRoadToFront(Transform road)
    {
        float frontZ = GetFrontRoadZ();

        Vector3 pos = road.position;
        pos.z = frontZ + roadLength;
        road.position = pos;
    }

    float GetFrontRoadZ()
    {
        if (roads.Count == 0)
            return 0f;

        float frontZ = roads[0].position.z;

        for (int i = 1; i < roads.Count; i++)
        {
            if (roads[i] == null)
                continue;

            if (roads[i].position.z > frontZ)
            {
                frontZ = roads[i].position.z;
            }
        }

        return frontZ;
    }
}