using UnityEngine;

[DisallowMultipleComponent]
public class SplinePathFollower : MonoBehaviour
{
    private enum LocalForwardAxis
    {
        PositiveZ,
        NegativeZ,
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY
    }

    [Header("Path")]
    [SerializeField] private Transform pathRoot;
    [SerializeField] private Transform movingTarget;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool startAtFirstPoint = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private bool faceMoveDirection = true;
    [SerializeField] private float turnSpeed = 6f;
    [SerializeField] private float lookAheadDistance = 0.75f;

    [Header("Bow Direction")]
    [SerializeField] private Transform bowReference;
    [SerializeField] private LocalForwardAxis bowForwardAxis = LocalForwardAxis.PositiveZ;
    [SerializeField] private Vector3 rotationOffset;

    [Header("Spline")]
    [SerializeField, Range(4, 32)] private int samplesPerSegment = 12;

    [Header("Runtime Status")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private int pathPointCount;
    [SerializeField] private float pathLength;
    [SerializeField] private float traveledDistance;
    [SerializeField] private bool playing;

    private Transform[] pathPoints = new Transform[0];
    private Vector3[] sampledPositions = new Vector3[0];
    private float[] sampledDistances = new float[0];
    private float totalDistance;
    private float currentDistance;
    private bool isPlaying;
    private Vector3 localBowForward = Vector3.forward;

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        turnSpeed = Mathf.Max(0f, turnSpeed);
        lookAheadDistance = Mathf.Max(0.01f, lookAheadDistance);
        samplesPerSegment = Mathf.Max(4, samplesPerSegment);

        CaptureLocalBowForward();
        UpdateRuntimeStatus();
    }

    private void Awake()
    {
        if (movingTarget == null)
        {
            movingTarget = transform;
        }

        if (pathRoot == null)
        {
            GameObject foundPathRoot = GameObject.Find("Path_Root");
            if (foundPathRoot != null)
            {
                pathRoot = foundPathRoot.transform;
            }
        }

        RebuildPath();
        CaptureLocalBowForward();

        if (startAtFirstPoint && sampledPositions.Length > 0)
        {
            currentDistance = 0f;
            ApplyPositionAndRotation(EvaluatePosition(currentDistance), EvaluatePosition(0.1f));
        }

        UpdateRuntimeStatus();
    }

    private void OnEnable()
    {
        isPlaying = playOnAwake;
        UpdateRuntimeStatus();
    }

    private void LateUpdate()
    {
        if (totalDistance <= 0f && pathRoot != null)
        {
            RebuildPath();
        }

        if (!isPlaying || movingTarget == null || totalDistance <= 0f)
        {
            UpdateRuntimeStatus();
            return;
        }

        currentDistance += moveSpeed * Time.deltaTime;

        if (loop)
        {
            currentDistance %= totalDistance;
        }
        else if (currentDistance >= totalDistance)
        {
            currentDistance = totalDistance;
            isPlaying = false;
        }

        Vector3 position = EvaluatePosition(currentDistance);
        Vector3 lookAhead = EvaluatePosition(currentDistance + lookAheadDistance);
        ApplyPositionAndRotation(position, lookAhead);
        UpdateRuntimeStatus();
    }

    public void Play()
    {
        isPlaying = true;
        UpdateRuntimeStatus();
    }

    public void Pause()
    {
        isPlaying = false;
        UpdateRuntimeStatus();
    }

    public void Restart()
    {
        currentDistance = 0f;
        isPlaying = true;
        UpdateRuntimeStatus();
    }

    public void RebuildPath()
    {
        CachePathPoints();
        BuildSampledPath();
        CaptureLocalBowForward();
        UpdateRuntimeStatus();

        if (showDebugLogs)
        {
            Debug.Log($"[SplinePathFollower] Path rebuilt. Points: {pathPointCount}, Length: {pathLength:F2}", this);
        }
    }

    private void CachePathPoints()
    {
        if (pathRoot == null || pathRoot.childCount == 0)
        {
            pathPoints = new Transform[0];
            return;
        }

        pathPoints = new Transform[pathRoot.childCount];
        for (int i = 0; i < pathRoot.childCount; i++)
        {
            pathPoints[i] = pathRoot.GetChild(i);
        }
    }

    private void BuildSampledPath()
    {
        totalDistance = 0f;

        if (pathPoints.Length < 2)
        {
            sampledPositions = new Vector3[0];
            sampledDistances = new float[0];
            return;
        }

        int segmentCount = loop ? pathPoints.Length : pathPoints.Length - 1;
        int sampleCount = segmentCount * samplesPerSegment + 1;
        sampledPositions = new Vector3[sampleCount];
        sampledDistances = new float[sampleCount];

        Vector3 previous = GetSplinePosition(0, 0f);
        sampledPositions[0] = previous;
        sampledDistances[0] = 0f;

        int sampleIndex = 1;
        for (int segment = 0; segment < segmentCount; segment++)
        {
            for (int sample = 1; sample <= samplesPerSegment; sample++)
            {
                float t = sample / (float)samplesPerSegment;
                Vector3 current = GetSplinePosition(segment, t);

                totalDistance += Vector3.Distance(previous, current);
                sampledPositions[sampleIndex] = current;
                sampledDistances[sampleIndex] = totalDistance;

                previous = current;
                sampleIndex++;
            }
        }
    }

    private Vector3 EvaluatePosition(float distance)
    {
        if (sampledPositions.Length == 0)
        {
            return movingTarget != null ? movingTarget.position : transform.position;
        }

        if (loop && totalDistance > 0f)
        {
            distance %= totalDistance;
            if (distance < 0f)
            {
                distance += totalDistance;
            }
        }
        else
        {
            distance = Mathf.Clamp(distance, 0f, totalDistance);
        }

        for (int i = 1; i < sampledDistances.Length; i++)
        {
            if (distance <= sampledDistances[i])
            {
                float previousDistance = sampledDistances[i - 1];
                float nextDistance = sampledDistances[i];
                float t = Mathf.InverseLerp(previousDistance, nextDistance, distance);
                return Vector3.Lerp(sampledPositions[i - 1], sampledPositions[i], t);
            }
        }

        return sampledPositions[sampledPositions.Length - 1];
    }

    private Vector3 GetSplinePosition(int segmentIndex, float t)
    {
        Vector3 p0 = GetPointPosition(segmentIndex - 1);
        Vector3 p1 = GetPointPosition(segmentIndex);
        Vector3 p2 = GetPointPosition(segmentIndex + 1);
        Vector3 p3 = GetPointPosition(segmentIndex + 2);

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    private Vector3 GetPointPosition(int index)
    {
        if (loop)
        {
            int wrappedIndex = (index % pathPoints.Length + pathPoints.Length) % pathPoints.Length;
            return pathPoints[wrappedIndex].position + Vector3.up * heightOffset;
        }

        int clampedIndex = Mathf.Clamp(index, 0, pathPoints.Length - 1);
        return pathPoints[clampedIndex].position + Vector3.up * heightOffset;
    }

    private void ApplyPositionAndRotation(Vector3 position, Vector3 lookAhead)
    {
        movingTarget.position = position;

        if (!faceMoveDirection)
        {
            return;
        }

        Vector3 direction = lookAhead - position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion axisCorrection = Quaternion.FromToRotation(localBowForward.normalized, Vector3.forward);
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up) * Quaternion.Euler(rotationOffset) * axisCorrection;
        movingTarget.rotation = Quaternion.Slerp(movingTarget.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void CaptureLocalBowForward()
    {
        if (movingTarget == null)
        {
            localBowForward = Vector3.forward;
            return;
        }

        if (bowReference == null && movingTarget.childCount == 1)
        {
            bowReference = movingTarget.GetChild(0);
        }

        Transform reference = bowReference != null ? bowReference : movingTarget;
        Vector3 bowWorldForward = reference.TransformDirection(GetLocalAxis(bowForwardAxis));
        localBowForward = movingTarget.InverseTransformDirection(bowWorldForward);

        if (localBowForward.sqrMagnitude < 0.0001f)
        {
            localBowForward = Vector3.forward;
        }
    }

    private void UpdateRuntimeStatus()
    {
        pathPointCount = pathPoints != null ? pathPoints.Length : 0;
        pathLength = totalDistance;
        traveledDistance = currentDistance;
        playing = isPlaying;
    }

    private void OnDrawGizmosSelected()
    {
        if (pathRoot == null || pathRoot.childCount < 2)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        Vector3 previous = pathRoot.GetChild(0).position + Vector3.up * heightOffset;
        int segmentCount = loop ? pathRoot.childCount : pathRoot.childCount - 1;
        int gizmoSamplesPerSegment = Mathf.Max(4, samplesPerSegment);

        for (int segment = 0; segment < segmentCount; segment++)
        {
            for (int sample = 1; sample <= gizmoSamplesPerSegment; sample++)
            {
                float t = sample / (float)gizmoSamplesPerSegment;
                Vector3 current = GetGizmoSplinePosition(segment, t);
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }
    }

    private Vector3 GetGizmoSplinePosition(int segmentIndex, float t)
    {
        Vector3 p0 = GetGizmoPointPosition(segmentIndex - 1);
        Vector3 p1 = GetGizmoPointPosition(segmentIndex);
        Vector3 p2 = GetGizmoPointPosition(segmentIndex + 1);
        Vector3 p3 = GetGizmoPointPosition(segmentIndex + 2);

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    private Vector3 GetGizmoPointPosition(int index)
    {
        if (loop)
        {
            int wrappedIndex = (index % pathRoot.childCount + pathRoot.childCount) % pathRoot.childCount;
            return pathRoot.GetChild(wrappedIndex).position + Vector3.up * heightOffset;
        }

        int clampedIndex = Mathf.Clamp(index, 0, pathRoot.childCount - 1);
        return pathRoot.GetChild(clampedIndex).position + Vector3.up * heightOffset;
    }

    private Vector3 GetLocalAxis(LocalForwardAxis axis)
    {
        switch (axis)
        {
            case LocalForwardAxis.NegativeZ:
                return Vector3.back;
            case LocalForwardAxis.PositiveX:
                return Vector3.right;
            case LocalForwardAxis.NegativeX:
                return Vector3.left;
            case LocalForwardAxis.PositiveY:
                return Vector3.up;
            case LocalForwardAxis.NegativeY:
                return Vector3.down;
            default:
                return Vector3.forward;
        }
    }
}
