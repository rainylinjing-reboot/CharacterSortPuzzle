using System.Collections;
using UnityEngine;

public class FailCameraEffect : MonoBehaviour
{
    [Header("Push Back")]
    public Vector3 pushDirection = new Vector3(0f, 0f, -1f);
    public float pushBackDistance = 0.45f;
    public float pushUpDistance = 0.08f;
    public float pushDuration = 0.18f;

    [Header("Shake")]
    public float shakeDuration = 0.25f;
    public float shakePower = 0.035f;

    [Header("Return")]
    public bool returnToOriginalPosition = false;
    public float returnDuration = 0.25f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Coroutine effectCoroutine;

    void Awake()
    {
        SaveOriginalTransform();
    }

    void SaveOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void PlayFailEffect()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }

        effectCoroutine = StartCoroutine(FailEffectRoutine());
    }

    IEnumerator FailEffectRoutine()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 normalizedDirection = pushDirection.normalized;

        Vector3 targetPosition =
            startPosition
            + normalizedDirection * pushBackDistance
            + Vector3.up * pushUpDistance;

        float timer = 0f;

        while (timer < pushDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / pushDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;

        timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.unscaledDeltaTime;

            Vector3 randomOffset = Random.insideUnitSphere * shakePower;
            randomOffset.z = 0f;

            transform.position = targetPosition + randomOffset;

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = startRotation;

        if (returnToOriginalPosition == true)
        {
            timer = 0f;
            Vector3 returnStartPosition = transform.position;

            while (timer < returnDuration)
            {
                timer += Time.unscaledDeltaTime;

                float t = timer / returnDuration;
                t = Mathf.SmoothStep(0f, 1f, t);

                transform.position = Vector3.Lerp(returnStartPosition, originalPosition, t);
                transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, t);

                yield return null;
            }

            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
    }

    public void ResetCamera()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}