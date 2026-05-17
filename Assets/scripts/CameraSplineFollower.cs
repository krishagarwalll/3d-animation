using UnityEngine;

/// <summary>
/// Moves a camera along a spline defined by waypoints with smooth interpolation.
/// Attach to a camera GameObject with waypoint transforms as children or assigned in inspector.
/// </summary>
public class CameraSplineFollower : MonoBehaviour
{
    [Header("Spline Settings")]
    public Transform[] waypoints;

    [Range(0f, 1f)]
    public float speed = 0.02f;

    [Tooltip("How smoothly the camera moves along the path")]
    public float smoothness = 0.5f;

    [Header("Look At")]
    public Transform lookAtTarget;

    [Tooltip("How smoothly the camera rotates to face the target")]
    public float lookAtSmoothing = 3f;

    private float currentT = 0f;
    private bool forward = true;

    void Update()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        // Ping-pong along the path
        if (forward)
        {
            currentT += speed * Time.deltaTime;
            if (currentT >= 1f) { currentT = 1f; forward = false; }
        }
        else
        {
            currentT -= speed * Time.deltaTime;
            if (currentT <= 0f) { currentT = 0f; forward = true; }
        }

        // Catmull-Rom spline interpolation
        Vector3 targetPos = EvaluateCatmullRom(currentT);
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothness);

        // Smooth look-at
        if (lookAtTarget != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookAtTarget.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lookAtSmoothing * Time.deltaTime);
        }
    }

    Vector3 EvaluateCatmullRom(float t)
    {
        int numSections = waypoints.Length - 1;
        float sectionT = t * numSections;
        int section = Mathf.Min(Mathf.FloorToInt(sectionT), numSections - 1);
        float localT = sectionT - section;

        Vector3 p0 = waypoints[Mathf.Max(section - 1, 0)].position;
        Vector3 p1 = waypoints[section].position;
        Vector3 p2 = waypoints[Mathf.Min(section + 1, waypoints.Length - 1)].position;
        Vector3 p3 = waypoints[Mathf.Min(section + 2, waypoints.Length - 1)].position;

        return CatmullRom(p0, p1, p2, p3, localT);
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
}
