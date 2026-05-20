using UnityEngine;

public class LineController : MonoBehaviour
{
    [Header("[ 라인 설정 ]")]
    public int lineIndex;
    public bool isWaitingLine = false;
    public Slot[] slots = new Slot[5];

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].slotIndex = i;
                slots[i].ownerLine = this; // [3차 추가 파트 자동 연동] 슬롯에게 주인 라인 주입
            }
        }
    }

    public CharacterPiece GetTopCharacter()
    {
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i] != null && !slots[i].IsEmpty)
            {
                return slots[i].GetCharacter();
            }
        }
        return null;
    }

    public Slot GetFirstEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].IsEmpty)
            {
                return slots[i];
            }
        }
        return null;
    }
}