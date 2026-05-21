using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitController : MonoBehaviour
{
    [Header("[ 회전 속도 및 민감도 ]")]
    public float rotateSpeedX = 20f;    // 마우스 좌우 회전 속도
    public float rotateSpeedY = 15f;    // 마우스 위아래 회전 속도
    public float smoothTime = 7f;       // 회전/줌 감속도 (쫀득한 손맛)

    [Header("[ 상하 회전 각도 제한 ]")]
    public float minVerticalAngle = 10f;  // 너무 바닥으로 내려가지 않게 제한
    public float maxVerticalAngle = 80f;  // 하늘 위에서 수직으로 내려다보는 최고 각도 제한

    [Header("[ 줌(Zoom) 기능 설정 ]")]
    public float zoomSpeed = 15f;       // 시원시원한 고속 휠 줌 속도
    public float minDistance = 5f;      
    public float maxDistance = 40f;     

    // 내부 연산 및 부드러운 보간용 변수
    private Vector3 calculatedCenter;   // 💡 에디터 구도를 바탕으로 역산해낸 진짜 가상 중심점
    private float currentX = 0f;        
    private float targetX = 0f;         
    private float currentY = 0f;        
    private float targetY = 0f;         
    private float currentDistance = 15f;
    private float targetDistance = 15f; 

    private bool isOrbiting = false;
    private bool hasDrivenOnce = false; // 마우스 조작 전까지 개입 방지선

    private void Start()
    {
        // ⭐ [튐 현상 근본적 해결의 핵심]
        // 에디터 구도 상에서 카메라가 현재 정확히 바라보고 있는 3D 공간상의 '가상 중심점'을 수학적으로 역산합니다.
        // 이 계산을 통해 카메라의 초기 Position, Rotation, Z축 꼬임(-16.48) 데이터가 그대로 중심 축에 녹아듭니다.
        
        currentX = transform.eulerAngles.y;
        targetX = currentX;
        
        currentY = transform.eulerAngles.x; 
        targetY = currentY;

        // 하이어라키에 GameBoard가 있다면 그 위치를 기준으로 거리를 잡고, 없으면 기본 15 유닛 앞으로 가상 중심 지정
        GameObject gameBoard = GameObject.Find("GameBoard");
        if (gameBoard != null)
        {
            currentDistance = Vector3.Distance(transform.position, gameBoard.transform.position);
        }
        else
        {
            currentDistance = 15f;
        }
        targetDistance = currentDistance;

        // 카메라 정면 방향 벡터에 거리를 곱해 '진짜 쳐다보는 과녁점'을 구합니다.
        calculatedCenter = transform.position + transform.forward * currentDistance;

        hasDrivenOnce = false;
    }

    private void LateUpdate()
    {
        if (Mouse.current == null) return;

        bool rightInFrame = Mouse.current.rightButton.isPressed;
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        bool isZooming = Mathf.Abs(scrollValue) > 0.01f;

        // 🎯 A. 마우스 우클릭 드래그 조작 세션 감지
        if (rightInFrame)
        {
            if (!isOrbiting)
            {
                isOrbiting = true;
                
                // 💡 우클릭을 딱 누르는 순간, 미세한 오차를 한 번 더 잡아주기 위해 실시간 동기화
                currentX = transform.eulerAngles.y;
                targetX = currentX;
                currentY = transform.eulerAngles.x;
                targetY = currentY;
                
                hasDrivenOnce = true; 
            }

            float mouseDeltaX = Mouse.current.delta.x.ReadValue();
            float mouseDeltaY = Mouse.current.delta.y.ReadValue();

            targetX += mouseDeltaX * rotateSpeedX * 0.02f;
            targetY -= mouseDeltaY * rotateSpeedY * 0.02f;
            targetY = Mathf.Clamp(targetY, minVerticalAngle, maxVerticalAngle);
        }
        else
        {
            isOrbiting = false;
        }

        // 🎯 B. 마우스 휠 스크롤 감지 및 고속 줌 연산
        if (isZooming)
        {
            if (!hasDrivenOnce)
            {
                currentX = transform.eulerAngles.y;
                targetX = currentX;
                currentY = transform.eulerAngles.x;
                targetY = currentY;
                hasDrivenOnce = true;
            }

            targetDistance -= (scrollValue * 0.01f) * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // 🛡️ [마우스 조작 전 절대 보존선]
        // 우클릭이나 휠을 만지기 전까지는 카메라 트랜스폼을 절대 건드리지 않고 에디터 세팅을 100% 박제합니다.
        if (!hasDrivenOnce)
        {
            return;
        }

        // 🎯 C. 조작이 시작되면 부드럽고 매끄럽게 보간 이동
        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * smoothTime);
        currentY = Mathf.Lerp(currentY, targetY, Time.deltaTime * smoothTime);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothTime);

        // 🎯 D. 계산된 가상 중심점(calculatedCenter) 기준으로 완벽한 공전 좌표 주입
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        Vector3 position = rotation * negDistance + calculatedCenter;

        transform.rotation = rotation;
        transform.position = position;
    }
}