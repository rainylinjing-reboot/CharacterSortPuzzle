using UnityEngine;

public class Slot : MonoBehaviour
{
    [Header("[ 슬롯 상태 ]")]
    public int slotIndex; // 0 (가까운 아래쪽) ~ 4 (먼 위쪽/출구)

    // 현재 이 슬롯에 배치된 캐릭터 피스 (비어있으면 null)
    private CharacterPiece currentPiece = null;

    public bool IsEmpty => currentPiece == null;

    public void SetCharacter(CharacterPiece piece)
    {
        currentPiece = piece;
    }

    public CharacterPiece GetCharacter()
    {
        return currentPiece;
    }

    public void ClearSlot()
    {
        currentPiece = null;
    }
}