using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("[ 라인 컴포넌트 등록 ]")]
    public LineController[] mainLines;
    public LineController waitingLine;

    [Header("[ 캐릭터 생성 설정 ]")]
    public CharacterPrefabSet prefabSet;
    public Transform characterParent;        // Hierarchy의 CharacterGroup 오브젝트 연결 칸

    public void SetupBoard(StageData stageData)
    {
        if (stageData == null) return;

        for (int i = 0; i < mainLines.Length; i++)
        {
            if (mainLines[i] != null)
            {
                bool isLineActive = i < stageData.activeLines;
                mainLines[i].gameObject.SetActive(isLineActive);
                mainLines[i].lineIndex = i;
            }
        }

        if (waitingLine != null)
        {
            waitingLine.gameObject.SetActive(true);
            waitingLine.isWaitingLine = true;

            for (int i = 0; i < waitingLine.slots.Length; i++)
            {
                if (waitingLine.slots[i] != null)
                {
                    bool isSlotActive = i < stageData.waitingSlotCount;
                    waitingLine.slots[i].gameObject.SetActive(isSlotActive);
                }
            }
        }
    }

    public void SpawnCharacters(StageData stageData)
    {
        if (prefabSet == null)
        {
            Debug.LogError("CharacterPrefabSet이 없습니다.");
            return;
        }

        // 1. 이번 스테이지에 필요한 캐릭터 리스트 생성
        List<CharacterType> characterPool = new List<CharacterType>();
        for (int i = 0; i < stageData.activeLines; i++)
        {
            CharacterType type = (CharacterType)(i + 1);
            for (int j = 0; j < 4; j++)
            {
                characterPool.Add(type);
            }
        }

        // 2. 랜덤 셔플
        for (int i = 0; i < characterPool.Count; i++)
        {
            CharacterType temp = characterPool[i];
            int randomIndex = Random.Range(i, characterPool.Count);
            characterPool[i] = characterPool[randomIndex];
            characterPool[randomIndex] = temp;
        }

        // 3. 캐릭터 생성 및 배치 (안전성 강화)
        int poolIndex = 0;
        for (int i = 0; i < stageData.activeLines; i++)
        {
            LineController line = mainLines[i];

            // 각 라인의 Slot_0 ~ Slot_3에 배치
            for (int j = 0; j < 4; j++)
            {
                if (poolIndex >= characterPool.Count) break;

                Slot targetSlot = line.slots[j];
                if (targetSlot == null) continue;

                CharacterType currentType = characterPool[poolIndex];
                GameObject prefab = prefabSet.GetPrefab(currentType);

                if (prefab != null)
                {
                    // [변경점] 일단 부모 없이 월드 좌표 기본값(0,0,0)으로 안전하게 생성합니다.
                    GameObject characterGo = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                    characterGo.name = currentType.ToString() + $"_{i}_{j}";

                    // 생성 직후 부모를 CharacterGroup으로 강제 지정합니다.
                    if (characterParent != null)
                    {
                        characterGo.transform.SetParent(characterParent);
                    }

                    // 스크립트 체크 및 강제 추가 방어선
                    CharacterPiece piece = characterGo.GetComponent<CharacterPiece>();
                    if (piece == null)
                    {
                        piece = characterGo.AddComponent<CharacterPiece>();
                    }

                    piece.characterType = currentType;

                    // 슬롯에 데이터 등록 및 슬롯의 좌표로 물리적 이동
                    piece.InitSetup(targetSlot);
                }

                poolIndex++;
            }
        }

        Debug.Log($"[검증 완료] 총 {poolIndex}개의 캐릭터 오브젝트가 계층 구조에 배치되었습니다.");
    }
}