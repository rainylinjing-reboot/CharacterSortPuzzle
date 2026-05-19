using UnityEngine;

public class CharacterPiece : MonoBehaviour
{
    [Header("[ 캐릭터 정보 ]")]
    public CharacterType characterType;
    public Slot currentSlot;

    // 이후 단계(4차) 애니메이션 연동을 위한 컴포넌트 선언부 미리 확보
    [HideInInspector] public Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // 해당 슬롯으로 정보 등록 및 강제 텔레포트 (1차 단계 확인용)
    public void InitSetup(Slot targetSlot)
    {
        if (targetSlot == null) return;

        currentSlot = targetSlot;
        targetSlot.SetCharacter(this);

        // 1차 단계에서는 애니메이션 없이 즉시 위치 이동
        transform.position = targetSlot.transform.position;
    }
}