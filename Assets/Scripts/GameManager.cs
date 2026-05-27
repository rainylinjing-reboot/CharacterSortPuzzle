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
    private Coroutine stageLoadCoroutine;

    private void Start()
    {
        mainCamera = Camera.main;
        currentStageIndex = 0;
    }

    public void LoadStage(int index)
    {
        index = Mathf.Max(0, index);

        if (stageLoadCoroutine != null)
        {
            StopCoroutine(stageLoadCoroutine);
        }

        if (uiManager != null)
        {
            uiManager.ClearCountdownText();
        }

        stageLoadCoroutine = StartCoroutine(LoadStageRoutine(index));
    }

    public void LoadStageDirectFromIntro(int index)
    {
        index = Mathf.Max(0, index);

        if (stageLoadCoroutine != null)
        {
            StopCoroutine(stageLoadCoroutine);
            stageLoadCoroutine = null;
        }

        if (allStages == null || allStages.Length == 0) return;

        if (index < allStages.Length)
        {
            currentStageIndex = index;
            currentStageData = allStages[index];

            if (InitializeGame())
            {
                StartCurrentStage();
            }
        }
        else
        {
            AllStageClear();
        }
    }

    private IEnumerator LoadStageRoutine(int index)
    {
        if (allStages == null || allStages.Length == 0)
        {
            stageLoadCoroutine = null;
            yield break;
        }

        if (index < allStages.Length)
        {
            currentStageIndex = index;
            currentStageData = allStages[index];

            if (!InitializeGame())
            {
                stageLoadCoroutine = null;
                yield break;
            }

            if (uiManager != null)
            {
                yield return uiManager.PlayCountdownRoutine();
            }

            StartCurrentStage();
        }
        else
        {
            AllStageClear();
        }

        stageLoadCoroutine = null;
    }

    private bool InitializeGame()
    {
        if (currentStageData == null || boardManager == null || uiManager == null) return false;

        Time.timeScale = 1f;
        isGameActive = false;
        isGiveUpConfirming = false;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
        }
        
        if (uiManager != null) 
        {
            uiManager.SetGiveUpPopupActive(false);
        }

        ClearSelectedCharacter();
        ClearExistingCharacters();

        boardManager.SetupBoard(currentStageData);
        boardManager.SpawnCharacters(currentStageData);

        timeRemaining = currentStageData.timeLimit;
        uiManager.UpdateStageText(currentStageData.stageNumber);
        uiManager.UpdateTimerText(timeRemaining);
        uiManager.ShowResultText("");

        return true;
    }

    private void StartCurrentStage()
    {
        if (currentStageData == null) return;

        isGameActive = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.startClip);
            SoundManager.Instance.PlayStageBGM(currentStageData.stageNumber);
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
            if (uiManager != null) uiManager.UpdateTimerText(timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            if (uiManager != null) uiManager.UpdateTimerText(timeRemaining);
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
        Time.timeScale = 1f; 
        isGiveUpConfirming = false;
        isGameActive = false;

        LoadStage(currentStageIndex);
    }

    public void ClickConfirmGiveUpButton()
    {
        if (!isGiveUpConfirming) return;
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);
        ConfirmGiveUpAndRestart();
    }

    private void HandleMouseClick()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || isGiveUpConfirming) return;

        // 2026 하이엔드 최신 InputSystem 구문 반영 오차 보정 보완
        Vector2 mousePosition = Mouse.current.position.ReadValue();
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
        if (clickedSlot == null || selectedCharacter == null || !clickedSlot.IsEmpty) return;
        if (selectedCharacter.currentSlot == null) return;

        LineController startLine = selectedCharacter.currentSlot.ownerLine;
        LineController targetLine = clickedSlot.ownerLine;

        if (startLine == null || targetLine == null || startLine == targetLine) return;

        Slot realTargetSlot = targetLine.GetFirstEmptySlot();

        if (realTargetSlot != null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.clickClip);

            List<Slot> calculatedPath = new List<Slot>();

            int exitGateIndex = ResolveGateIndex(startLine);
            int enterGateIndex = ResolveGateIndex(targetLine);
            if (exitGateIndex < 0 || enterGateIndex < 0) return;

            int startIdx = selectedCharacter.currentSlot.slotIndex;
            for (int i = startIdx + 1; i <= exitGateIndex; i++)
            {
                AddSlotIfPresent(calculatedPath, startLine, i);
            }

            if (exitGateIndex != enterGateIndex)
            {
                if (exitGateIndex == 4) AddSlotIfPresent(calculatedPath, startLine, 5);
                if (enterGateIndex == 4) AddSlotIfPresent(calculatedPath, targetLine, 5);
            }

            AddSlotIfPresent(calculatedPath, targetLine, enterGateIndex);

            int targetIdx = realTargetSlot.slotIndex;
            for (int i = enterGateIndex - 1; i >= targetIdx; i--)
            {
                AddSlotIfPresent(calculatedPath, targetLine, i);
            }

            if (calculatedPath.Count == 0) return;

            selectedCharacter.MoveAlongPath(calculatedPath);
            ClearSelectedCharacter();
        }
    }

    public void CheckStageClearConditionDirect()
    {
        if (!isGameActive) return;
        if (currentStageData == null || boardManager == null || boardManager.mainLines == null) return;
        if (AnyCharacterMoving()) return;

        if (boardManager.waitingLine != null && boardManager.waitingLine.slots != null)
        {
            foreach (Slot slot in boardManager.waitingLine.slots)
            {
                if (slot != null && slot.gameObject.activeSelf && !slot.IsEmpty) return;
            }
        }

        int activeLineCount = Mathf.Min(currentStageData.activeLines, boardManager.mainLines.Length);
        for (int i = 0; i < activeLineCount; i++)
        {
            LineController line = boardManager.mainLines[i];
            if (line == null) continue;
            if (line.slots == null || line.slots.Length < 4) return;
            if (line.slots[0] == null) return;

            if (line.slots[0].IsEmpty) return;
            CharacterType targetType = line.slots[0].GetCharacter().characterType;

            for (int j = 0; j < 4; j++)
            {
                if (line.slots[j] == null) return;
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
            if (piece != null && piece.gameObject != null) Destroy(piece.gameObject);
        }

        if (boardManager != null)
        {
            foreach (var line in boardManager.mainLines)
            {
                if (line != null)
                {
                    if (line.slots != null)
                    {
                        foreach (var slot in line.slots) if (slot != null) slot.ClearSlot();
                    }
                }
            }
            if (boardManager.waitingLine != null && boardManager.waitingLine.slots != null)
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

        int finalStageNumber = currentStageData != null ? currentStageData.stageNumber : currentStageIndex;
        float totalTakenTime = currentStageData != null ? currentStageData.timeLimit - timeRemaining : 0f;
        if (uiManager != null) uiManager.OpenLeaderboardInput(finalStageNumber, totalTakenTime);
    }

    private bool AnyCharacterMoving()
    {
        CharacterPiece[] allPieces = FindObjectsByType<CharacterPiece>(FindObjectsSortMode.None);
        foreach (var piece in allPieces)
        {
            if (piece != null && piece.IsMoving) return true;
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

        if (uiManager != null)
        {
            uiManager.ShowResultText("FAIL!");
            uiManager.OpenFailureResult(currentStageIndex);
        }
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

    private int ResolveGateIndex(LineController line)
    {
        if (!TryGetSlot(line, 4, out Slot frontGate)) return -1;
        if (frontGate.IsEmpty) return 4;

        return TryGetSlot(line, 5, out _) ? 5 : -1;
    }

    private static bool TryGetSlot(LineController line, int index, out Slot slot)
    {
        slot = null;

        if (line == null || line.slots == null) return false;
        if (index < 0 || index >= line.slots.Length) return false;

        slot = line.slots[index];
        return slot != null;
    }

    private static void AddSlotIfPresent(List<Slot> path, LineController line, int index)
    {
        if (TryGetSlot(line, index, out Slot slot))
        {
            path.Add(slot);
        }
    }
}
