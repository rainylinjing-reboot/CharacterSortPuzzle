using UnityEngine;
using System.Collections.Generic;

public class CharacterPiece : MonoBehaviour
{
    [Header("[ 캐릭터 정보 ]")]
    public CharacterType characterType;
    public Slot currentSlot;

    [Header("[ 이동 및 연출 설정 ]")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 15f;
    public string runParameterName = "IsRunning";

    [Header("[ 선택 아웃라인 설정 ]")]
    [SerializeField] private uint selectedOutlineLayer = 1u << 1; // Light Layer 1

    [HideInInspector] public Animator animator;

    // 경로 이동 제어용 변수들
    private List<Vector3> movePath = new List<Vector3>(); // 밟아갈 좌표 순서 리스트
    private int currentPathIndex = 0;                     // 현재 가고 있는 주소 인덱스
    private bool isMoving = false;
    private Camera mainCamera;
    private Renderer[] cachedRenderers;
    private uint[] originalRenderingLayers;

    public bool IsMoving => isMoving;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;
        CacheRenderers();
    }

    private void Update()
    {
        if (!isMoving || movePath.Count == 0) return;

        // 1. 현재 타겟팅된 경로 점 좌표 가져오기
        Vector3 targetPoint = movePath[currentPathIndex];

        // Y축 높이 차이로 인해 땅으로 파묻히거나 붕 뜨는 현상 방지 (캐릭터 자체 Y값 유지)
        targetPoint.y = transform.position.y;

        // 2. 해당 경로 점으로 부드럽게 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

        // 3. 회전 처리 (지금 걸어가고 있는 방향 정면 바라보기)
        Vector3 moveDirection = (targetPoint - transform.position).normalized;
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. 현재 경로 점 도착 판정
        if (Vector3.Distance(transform.position, targetPoint) < 0.05f)
        {
            currentPathIndex++; // 다음 징검다리 점으로 인덱스 전환

            // 모든 경로 점을 다 밟았다면 최종 목적지 도착!
            if (currentPathIndex >= movePath.Count)
            {
                ArriveAtDestination();
            }
        }
    }

    public void InitSetup(Slot targetSlot)
    {
        if (targetSlot == null) return;

        currentSlot = targetSlot;
        targetSlot.SetCharacter(this);

        transform.position = targetSlot.transform.position;
        LookAtCameraInstantly();
    }

    // [핵심 변경] 매니저가 계산해 준 길 찾기 경로 리스트를 주입받아 출발하는 함수
    public void MoveAlongPath(List<Slot> pathSlots)
    {
        if (pathSlots == null || pathSlots.Count == 0) return;

        SetSelectedOutline(false);

        // 기존 슬롯 데이터 정리
        if (currentSlot != null) currentSlot.ClearSlot();

        // 최종 목적지 슬롯 정보 등록
        Slot finalDestination = pathSlots[pathSlots.Count - 1];
        currentSlot = finalDestination;
        finalDestination.SetCharacter(this);

        // 월드 좌표 경로 리스트 빌드
        movePath.Clear();
        foreach (Slot slot in pathSlots)
        {
            movePath.Add(slot.transform.position);
        }

        currentPathIndex = 0;
        isMoving = true;

        // RUN 애니메이션 켜기
        if (animator != null)
        {
            animator.SetBool(runParameterName, true);
        }
    }

    private void ArriveAtDestination()
    {
        isMoving = false;
        movePath.Clear();

        // IDLE 애니메이션으로 복구
        if (animator != null)
        {
            animator.SetBool(runParameterName, false);
        }

        // 카메라 정면 응시
        LookAtCameraInstantly();
    }

    public void SetSelectedOutline(bool selected)
    {
        CacheRenderers();

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] == null) continue;

            cachedRenderers[i].renderingLayerMask = selected
                ? originalRenderingLayers[i] | selectedOutlineLayer
                : originalRenderingLayers[i];
        }
    }

    private void CacheRenderers()
    {
        if (cachedRenderers != null && originalRenderingLayers != null) return;

        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        originalRenderingLayers = new uint[cachedRenderers.Length];

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            originalRenderingLayers[i] = cachedRenderers[i].renderingLayerMask;
        }
    }

    private void LookAtCameraInstantly()
    {
        if (mainCamera == null) return;

        Vector3 dirToCamera = mainCamera.transform.position - transform.position;
        dirToCamera.y = 0;

        if (dirToCamera != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dirToCamera);
        }
    }
}
