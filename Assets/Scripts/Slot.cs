using UnityEngine;

public class Slot : MonoBehaviour
{
    [Header("[ 슬롯 상태 ]")]
    public int slotIndex;
    public LineController ownerLine;

    private CharacterPiece currentPiece = null;

    public bool IsEmpty => currentPiece == null;

    // 기존 OnMouseDown()은 깔끔하게 삭제되었습니다.
    // 이제 마우스 감지는 GameManager가 총괄합니다.

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