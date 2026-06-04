using UnityEngine;

public class GameScenePlayerLoader : MonoBehaviour
{
    [Header("Player Prefabs")]
    public GameObject[] playerPrefabs;

    [Header("Spawn")]
    public Transform playerSpawnPoint;

    [Header("References")]
    public RoadManager roadManager;
    public LuckyRunGameManager gameManager;

    [Header("Fallback")]
    public int defaultCharacterIndex = 0;

    [Header("Debug")]
    public bool showDebugLog = true;

    private GameObject spawnedPlayer;

    void Awake()
    {
        SpawnSelectedPlayer();
        ConnectPlayerReferences();
    }

    void SpawnSelectedPlayer()
    {
        if (playerPrefabs == null || playerPrefabs.Length == 0)
        {
            Debug.LogWarning("[GameScenePlayerLoader] Player Prefabs가 비어 있습니다.");
            return;
        }

        int selectedIndex = PlayerSelectionData.SelectedCharacterIndex;

        if (selectedIndex < 0 || selectedIndex >= playerPrefabs.Length)
        {
            selectedIndex = defaultCharacterIndex;
        }

        GameObject selectedPrefab = playerPrefabs[selectedIndex];

        if (selectedPrefab == null)
        {
            Debug.LogWarning("[GameScenePlayerLoader] 선택된 캐릭터 프리팹이 비어 있습니다.");
            return;
        }

        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        if (playerSpawnPoint != null)
        {
            spawnPosition = playerSpawnPoint.position;
            spawnRotation = playerSpawnPoint.rotation;
        }

        spawnedPlayer = Instantiate(selectedPrefab, spawnPosition, spawnRotation);

        spawnedPlayer.name = selectedPrefab.name + "_Player";

        if (spawnedPlayer.CompareTag("Player") == false)
        {
            spawnedPlayer.tag = "Player";
        }

        if (showDebugLog == true)
        {
            Debug.Log("[GameScenePlayerLoader] 선택 캐릭터 생성 완료: " + spawnedPlayer.name);
        }
    }

    void ConnectPlayerReferences()
    {
        if (spawnedPlayer == null)
            return;

        if (roadManager == null)
        {
            roadManager = FindFirstObjectByType<RoadManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<LuckyRunGameManager>();
        }

        PlayerFailController playerFailController = spawnedPlayer.GetComponent<PlayerFailController>();

        if (playerFailController == null)
        {
            playerFailController = spawnedPlayer.GetComponentInChildren<PlayerFailController>();
        }

        if (roadManager != null)
        {
            roadManager.player = spawnedPlayer.transform;

            if (showDebugLog == true)
            {
                Debug.Log("[GameScenePlayerLoader] RoadManager.player 연결 완료: " + spawnedPlayer.name);
            }
        }
        else
        {
            Debug.LogWarning("[GameScenePlayerLoader] RoadManager를 찾지 못했습니다.");
        }

        if (gameManager != null)
        {
            gameManager.playerFailController = playerFailController;

            if (showDebugLog == true)
            {
                Debug.Log("[GameScenePlayerLoader] LuckyRunGameManager.playerFailController 연결 완료");
            }
        }
        else
        {
            Debug.LogWarning("[GameScenePlayerLoader] LuckyRunGameManager를 찾지 못했습니다.");
        }

        if (playerFailController == null)
        {
            Debug.LogWarning("[GameScenePlayerLoader] 생성된 캐릭터에서 PlayerFailController를 찾지 못했습니다.");
        }
    }
}