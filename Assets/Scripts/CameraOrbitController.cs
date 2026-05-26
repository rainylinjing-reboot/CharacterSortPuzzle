using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitController : MonoBehaviour
{
    [Header("[ 회전 속도 및 민감도 ]")]
    public float rotateSpeedX = 20f;    
    public float rotateSpeedY = 15f;    
    public float smoothTime = 7f;       

    [Header("[ 상하 회전 각도 제한 ]")]
    public float minVerticalAngle = 10f;  
    public float maxVerticalAngle = 80f;  

    [Header("[ 줌(Zoom) 기능 설정 ]")]
    public float zoomSpeed = 15f;       
    public float minDistance = 5f;      
    public float maxDistance = 40f;     

    private Vector3 calculatedCenter;   
    private float currentX = 0f;        
    private float targetX = 0f;         
    private float currentY = 0f;        
    private float targetY = 0f;         
    private float currentDistance = 15f;
    private float targetDistance = 15f; 

    private bool isOrbiting = false;
    private bool hasDrivenOnce = false; 

    private void Start()
    {
        // 에디터 구도를 바탕으로 역산해내는 가상 중심점 방식
        currentX = transform.eulerAngles.y;
        targetX = currentX;
        
        currentY = transform.eulerAngles.x; 
        targetY = currentY;

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
        calculatedCenter = transform.position + transform.forward * currentDistance;

        hasDrivenOnce = false; // 마우스 우클릭 조작 전까지 개입 철저히 방지
    }

    private void LateUpdate()
    {
        if (Mouse.current == null) return;

        bool rightInFrame = Mouse.current.rightButton.isPressed;
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        bool isZooming = Mathf.Abs(scrollValue) > 0.01f;

        if (rightInFrame)
        {
            if (!isOrbiting)
            {
                isOrbiting = true;
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

        // 🛡️ [절대 방어선] 마우스 조작을 직접 하기 전까지는 에디터에 수동으로 맞춰놓으신 구도를 100% 강제 박제합니다.
        if (!hasDrivenOnce)
        {
            return;
        }

        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * smoothTime);
        currentY = Mathf.Lerp(currentY, targetY, Time.deltaTime * smoothTime);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothTime);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        Vector3 position = rotation * negDistance + calculatedCenter;

        transform.rotation = rotation;
        transform.position = position;
    }
}