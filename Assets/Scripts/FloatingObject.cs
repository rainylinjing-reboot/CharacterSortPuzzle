using UnityEngine;

[DisallowMultipleComponent]
public class FloatingObject : MonoBehaviour
{
    [Header("[ Floating Motion ]")]
    [SerializeField] private float bobAmplitude = 0.25f;
    [SerializeField] private float bobSpeed = 1.2f;
    [SerializeField] private Vector3 driftAmplitude = new Vector3(0.08f, 0f, 0.08f);

    [Header("[ Rocking Rotation ]")]
    [SerializeField] private Vector3 rotationAmplitude = new Vector3(4f, 0f, 4f);
    [SerializeField] private float rotationSpeed = 0.8f;

    [Header("[ Variation ]")]
    [SerializeField] private bool randomizePhase = true;
    [SerializeField] private float phaseOffset = 0f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        if (randomizePhase)
        {
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    private void Update()
    {
        float bobTime = Time.time * bobSpeed + phaseOffset;
        float rotationTime = Time.time * rotationSpeed + phaseOffset;

        Vector3 offset = new Vector3(
            Mathf.Sin(bobTime * 0.7f) * driftAmplitude.x,
            Mathf.Sin(bobTime) * bobAmplitude,
            Mathf.Cos(bobTime * 0.6f) * driftAmplitude.z
        );

        Vector3 rotationOffset = new Vector3(
            Mathf.Sin(rotationTime) * rotationAmplitude.x,
            Mathf.Sin(rotationTime * 0.5f) * rotationAmplitude.y,
            Mathf.Cos(rotationTime) * rotationAmplitude.z
        );

        transform.localPosition = startPosition + offset;
        transform.localRotation = startRotation * Quaternion.Euler(rotationOffset);
    }

    public void ResetFloatingOrigin()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }
}
