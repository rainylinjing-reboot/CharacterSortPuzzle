using UnityEngine;

public class PlayerFailController : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    public string runParameterName = "IsRunning";
    public string dieTriggerName = "Die";

    [Header("Reset Animation State")]
    public string resetStateName = "sprint";
    public bool playResetStateOnRetry = true;

    [Header("Control")]
    public PlayerController playerController;
    public Rigidbody playerRigidbody;

    [Header("Reset Transform")]
    public bool resetTransformOnRetry = true;

    [Header("Debug")]
    public bool showDebugLog = true;

    private bool isDead = false;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;

    void Awake()
    {
        SaveStartTransform();
        AutoFindComponents();
    }

    void SaveStartTransform()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        startScale = transform.localScale;
    }

    void AutoFindComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }
    }

    public void PlayDie()
    {
        if (isDead == true)
            return;

        isDead = true;

        if (showDebugLog == true)
        {
            Debug.Log("[PlayerFailController] Die 애니메이션 실행");
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        StopRigidbody();

        if (animator != null)
        {
            if (string.IsNullOrEmpty(runParameterName) == false)
            {
                animator.SetBool(runParameterName, false);
            }

            if (string.IsNullOrEmpty(dieTriggerName) == false)
            {
                animator.ResetTrigger(dieTriggerName);
                animator.SetTrigger(dieTriggerName);
            }
        }
    }

    public void ResetFailState()
    {
        isDead = false;

        StopRigidbody();

        if (resetTransformOnRetry == true)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
            transform.localScale = startScale;
        }

        if (animator != null)
        {
            if (string.IsNullOrEmpty(dieTriggerName) == false)
            {
                animator.ResetTrigger(dieTriggerName);
            }

            if (string.IsNullOrEmpty(runParameterName) == false)
            {
                animator.SetBool(runParameterName, true);
            }

            if (playResetStateOnRetry == true && string.IsNullOrEmpty(resetStateName) == false)
            {
                animator.Play(resetStateName, 0, 0f);
                animator.Update(0f);
            }
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (showDebugLog == true)
        {
            Debug.Log("[PlayerFailController] 실패 상태 리셋 / 다시 달리기 시작");
        }
    }

    void StopRigidbody()
    {
        if (playerRigidbody == null)
            return;

        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;
    }
}