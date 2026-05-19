using UnityEngine;

public class LineController : MonoBehaviour
{
    [Header("[ 라인 설정 ]")]
    public int lineIndex; // 라인의 고유 번호
    public bool isWaitingLine = false; // 대기열 라인 여부

    // 하위에 배치된 슬롯들 (Slot_0 ~ Slot_4 순서대로 Inspector에서 등록 필수)
    public Slot[] slots = new Slot[5];

    private void Awake()
    {
        // 인덱스 방어 코드 및 자동 세팅
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].slotIndex = i;
            }
        }
    }

    // 이 라인에서 가장 위쪽(높은 index)에 있으면서 선택 가능한 캐릭터를 찾음
    public CharacterPiece GetTopCharacter()
    {
        // Slot_4부터 Slot_0까지 역순으로 검사
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i] != null && !slots[i].IsEmpty)
            {
                return slots[i].GetCharacter();
            }
        }
        return null; // 캐릭터가 하나도 없는 빈 줄인 경우
    }

    // 캐릭터가 이동해 들어올 수 있는 가장 아래쪽(낮은 index) 빈 슬롯을 반환
    public Slot GetFirstEmptySlot()
    {
        // Slot_0부터 Slot_4까지 순서대로 검사
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].IsEmpty)
            {
                return slots[i];
            }
        }
        return null; // 줄이 가득 찬 경우
    }
}