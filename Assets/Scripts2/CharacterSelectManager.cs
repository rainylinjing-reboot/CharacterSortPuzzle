using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "LuckyRun";

    [Header("Selection")]
    public int defaultCharacterIndex = 0;

    [Header("UI Highlight")]
    public GameObject[] selectedMarkers;

    private int selectedCharacterIndex;

    void Start()
    {
        selectedCharacterIndex = defaultCharacterIndex;
        PlayerSelectionData.SelectedCharacterIndex = selectedCharacterIndex;

        ApplySelectionVisual();
    }

    public void SelectCharacter(int index)
    {
        selectedCharacterIndex = index;
        PlayerSelectionData.SelectedCharacterIndex = selectedCharacterIndex;

        Debug.Log("[CharacterSelectManager] 선택 캐릭터: " + selectedCharacterIndex);

        ApplySelectionVisual();
    }

    public void StartGame()
    {
        PlayerSelectionData.SelectedCharacterIndex = selectedCharacterIndex;

        Debug.Log("[CharacterSelectManager] 게임 시작 / 캐릭터 인덱스: " + selectedCharacterIndex);

        SceneManager.LoadScene(gameSceneName);
    }

    void ApplySelectionVisual()
    {
        if (selectedMarkers == null || selectedMarkers.Length == 0)
            return;

        for (int i = 0; i < selectedMarkers.Length; i++)
        {
            if (selectedMarkers[i] == null)
                continue;

            selectedMarkers[i].SetActive(i == selectedCharacterIndex);
        }
    }
}