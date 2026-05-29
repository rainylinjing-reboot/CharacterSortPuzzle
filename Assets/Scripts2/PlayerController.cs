using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float horizontalSpeed = 5f;
    public float horizontalLimit = 3f;

    [Header("Smooth Movement")]
    public float moveSmooth = 10f;

    [Header("Turn")]
    public float turnAngle = 40f;
    public float turnSmooth = 8f;

    [Header("Animation")]
    public Animator animator;
    public string runParameterName = "IsRunning";

    private float targetX;
    private Quaternion startRotation;

    void Start()
    {
        targetX = transform.position.x;
        startRotation = transform.rotation;

        SetAnimator();
        StartRunning();
    }

    void Update()
    {
        MovePlayer();
        TurnPlayer();
    }

    void SetAnimator()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning("[PlayerController] Animator를 찾지 못했습니다.");
        }
        else
        {
            Debug.Log("[PlayerController] Animator 연결 완료: " + animator.name);
        }
    }

    void StartRunning()
    {
        if (animator == null)
            return;

        animator.SetBool(runParameterName, true);

        Debug.Log("[PlayerController] 달리기 시작: " + runParameterName + " = true");
    }

    void MovePlayer()
    {
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            horizontal = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontal = 1f;
        }

        targetX += horizontal * horizontalSpeed * Time.deltaTime;
        targetX = Mathf.Clamp(targetX, -horizontalLimit, horizontalLimit);

        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, moveSmooth * Time.deltaTime);
        transform.position = pos;
    }

    void TurnPlayer()
    {
        float direction = 0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            direction = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            direction = 1f;
        }

        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, direction * turnAngle, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSmooth * Time.deltaTime
        );
    }
}