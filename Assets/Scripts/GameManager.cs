using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("[ 매니저 참조 ]")]
    public BoardManager boardManager;
    public UIManager uiManager;

    [Header("[ 스테이지 레벨 데이터 관리 ]")]
    public StageData[] allStages;       
    private int currentStageIndex = 0;   

    [Header("[ 조작 상태 변수 ]")]
    public CharacterPiece selectedCharacter = null;

    private StageData currentStageData; 
    private float timeRemaining;
    private bool isGameActive = false;
    private bool isGiveUpConfirming = false; 
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        currentStageIndex = 0;
        LoadStage(currentStageIndex);
    }

    public void LoadStage(int index)
    {
        if (allStages == null || allStages.Length == 0) return;

        if (index < allStages.Length)
        {
            currentStageData = allStages[index];
            InitializeGame();
        }
        else
        {
            AllStageClear();
        }
    }

    private void InitializeGame()
    {
        if (currentStageData == null || boardManager == null || uiManager == null) return;

        Time.timeScale = 1f;
        isGiveUpConfirming = false;
        if (uiManager != null) uiManager.SetGiveUpPopupActive(false);

        ClearSelectedCharacter();
        ClearExistingCharacters();

        boardManager.SetupBoard(currentStageData);
        boardManager.SpawnCharacters(currentStageData);

        timeRemaining = currentStageData.timeLimit;
        uiManager.UpdateStageText(currentStageData.stageNumber);
        uiManager.UpdateTimerText(timeRemaining);
        uiManager.ShowResultText("");

        isGameActive = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.startClip);
            SoundManager.Instance.PlayBGM(); 
        }
    }

    private void Update()
    {
        if (isGiveUpConfirming)
        {
            if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
            {
                ConfirmGiveUpAndRestart();
                return;
            }
        }

        if (!isGameActive) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            uiManager.UpdateTimerText(timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            uiManager.UpdateTimerText(timeRemaining);
            GameOver();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
    }

    public void ClickGiveUpButton()
    {
        if (!isGameActive || isGiveUpConfirming) return;

        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

        isGiveUpConfirming = true;
        Time.timeScale = 0f; 
        
        if (uiManager != null) uiManager.SetGiveUpPopupActive(true); 
    }

    public void CancelGiveUp()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

        isGiveUpConfirming = false;
        Time.timeScale = 1f; 
        
        if (uiManager != null) uiManager.SetGiveUpPopupActive(false);
    }

    public void ConfirmGiveUpAndRestart()
    {
        LoadStage(currentStageIndex);
    }

    // 마우스 클릭 랭킹 확정용 추가
    public void ClickConfirmGiveUpButton()
    {
        if (!isGiveUpConfirming) return;
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
        ConfirmGiveUpAndRestart();
    }

    private void HandleMouseClick()
    {
        if (mainCamera == null || isGiveUpConfirming) return;

        Vector2 mousePosition = Pointer.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;

            CharacterPiece clickedCharacter = hitObject.GetComponentInParent<CharacterPiece>();
            if (clickedCharacter != null)
            {
                ProcessCharacterClick(clickedCharacter);
                return;
            }

            Slot clickedSlot = hitObject.GetComponentInParent<Slot>();
            if (clickedSlot != null)
            {
                ProcessSlotClick(clickedSlot);
                return;
            }
        }
    }

    private void ProcessCharacterClick(CharacterPiece clickedPiece)
    {
        if (clickedPiece.IsMoving) return;
        if (clickedPiece.currentSlot == null) return;

        LineController line = clickedPiece.currentSlot.ownerLine;
        if (line == null) return;

        CharacterPiece topPiece = line.GetTopCharacter();

        if (clickedPiece == topPiece)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
            SetSelectedCharacter(clickedPiece);
        }
    }

    private void ProcessSlotClick(Slot clickedSlot)
    {
        if (selectedCharacter == null || !clickedSlot.IsEmpty) return;

        LineController startLine = selectedCharacter.currentSlot.ownerLine;
        LineController targetLine = clickedSlot.ownerLine;

        if (startLine == targetLine) return;

        Slot realTargetSlot = targetLine.GetFirstEmptySlot();

        if (realTargetSlot != null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

            List<Slot> calculatedPath = new List<Slot>();

            int exitGateIndex = 4;
            if (startLine.slots[4] != null && !startLine.slots[4].IsEmpty) exitGateIndex = 5;

            int startIdx = selectedCharacter.currentSlot.slotIndex;
            for (int i = startIdx + 1; i <= exitGateIndex; i++)
            {
                if (startLine.slots[i] != null) calculatedPath.Add(startLine.slots[i]);
            }

            int enterGateIndex = 4;
            if (targetLine.slots[4] != null && !targetLine.slots[4].IsEmpty) enterGateIndex = 5;

            if (exitGateIndex != enterGateIndex)
            {
                if (exitGateIndex == 4 && startLine.slots[5] != null) calculatedPath.Add(startLine.slots[5]);
                if (enterGateIndex == 4 && targetLine.slots[5] != null) calculatedPath.Add(targetLine.slots[5]);
            }

            if (targetLine.slots[enterGateIndex] != null) calculatedPath.Add(targetLine.slots[enterGateIndex]);

            int targetIdx = realTargetSlot.slotIndex;
            for (int i = enterGateIndex - 1; i >= targetIdx; i--)
            {
                if (targetLine.slots[i] != null) calculatedPath.Add(targetLine.slots[i]);
            }

            selectedCharacter.MoveAlongPath(calculatedPath);
            ClearSelectedCharacter();
        }
    }

    public void CheckStageClearConditionDirect()
    {
        if (!isGameActive) return;
        if (AnyCharacterMoving()) return;

        if (boardManager.waitingLine != null)
        {
            foreach (Slot slot in boardManager.waitingLine.slots)
            {
                if (slot.gameObject.activeSelf && !slot.IsEmpty) return;
            }
        }

        for (int i = 0; i < currentStageData.activeLines; i++)
        {
            LineController line = boardManager.mainLines[i];
            if (line == null) continue;

            if (line.slots[0].IsEmpty) return;
            CharacterType targetType = line.slots[0].GetCharacter().characterType;

            for (int j = 0; j < 4; j++)
            {
                if (line.slots[j].IsEmpty || line.slots[j].GetCharacter().characterType != targetType) return;
            }
        }

        StartCoroutine(StageClearRoutine());
    }

    private IEnumerator StageClearRoutine()
    {
        isGameActive = false; 

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX(SoundManager.Instance.clearClip);
        }

        if (uiManager != null) uiManager.ShowResultText("STAGE CLEAR!");
        
        yield return new WaitForSeconds(2.5f);

        currentStageIndex++;
        LoadStage(currentStageIndex);
    }

    private void ClearExistingCharacters()
    {
        CharacterPiece[] existingPieces = FindObjectsByType<CharacterPiece>(FindObjectsSortMode.None);
        foreach (CharacterPiece piece in existingPieces)
        {
            Destroy(piece.gameObject);
        }

        if (boardManager != null)
        {
            foreach (var line in boardManager.mainLines)
            {
                if (line != null)
                {
                    foreach (var slot in line.slots) if (slot != null) slot.ClearSlot();
                }
            }
            if (boardManager.waitingLine != null)
            {
                foreach (var slot in boardManager.waitingLine.slots) if (slot != null) slot.ClearSlot();
            }
        }
    }

    private void AllStageClear()
    {
        isGameActive = false;
        ClearSelectedCharacter();
        
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clearClip);
        if (uiManager != null) uiManager.ShowResultText("ALL STAGES CLEAR!");

        // 🎯 모든 스테이지 정복 시 최종 명예의 전당 입력창 활성화!
        float totalTakenTime = currentStageData.timeLimit - timeRemaining;
        if (uiManager != null) uiManager.OpenLeaderboardInput(currentStageData.stageNumber, totalTakenTime);
    }

    private bool AnyCharacterMoving()
    {
        CharacterPiece[] allPieces = FindObjectsByType<CharacterPiece>(FindObjectsSortMode.None);
        foreach (var piece in allPieces)
        {
            if (piece.IsMoving) return true;
        }
        return false;
    }

    private void GameOver()
    {
        isGameActive = false;
        ClearSelectedCharacter();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySFX(SoundManager.Instance.failClip);
        }

        if (uiManager != null) uiManager.ShowResultText("GAME OVER");

        // 🎯 실패해서 게임오버 되었을 때도 그때까지 기록을 명예의 전당에 백업 유도!
        float takenTime = currentStageData.timeLimit - timeRemaining;
        if (uiManager != null) uiManager.OpenLeaderboardInput(currentStageIndex + 1, takenTime);
    }

    private void SetSelectedCharacter(CharacterPiece character)
    {
        if (selectedCharacter == character)
        {
            selectedCharacter.SetSelectedOutline(true);
            return;
        }
        ClearSelectedCharacter();
        selectedCharacter = character;
        if (selectedCharacter != null) selectedCharacter.SetSelectedOutline(true);
    }

    private void ClearSelectedCharacter()
    {
        if (selectedCharacter != null) selectedCharacter.SetSelectedOutline(false);
        selectedCharacter = null;
    }
}