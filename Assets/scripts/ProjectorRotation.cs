using UnityEngine;

public class ProjectorRotation : MonoBehaviour
{
    [Header("Rotation Limits (Min / Max)")]
    [Tooltip("X is the negative limit, Y is the positive limit. E.g., -15 to 15")]
    [SerializeField] private Vector2 xLimits = new Vector2(-20f, 20f);
    [SerializeField] private Vector2 yLimits = new Vector2(-45f, 45f);
    [SerializeField] private Vector2 zLimits = new Vector2(0f, 0f);

    [Header("Movement Settings")]
    [Tooltip("How fast the light wanders. (Usually a low number like 0.2 to 2)")]
    [SerializeField] private float rotationSpeed = 0.5f;

    private Quaternion startRotation;

    // Random offsets so the X, Y, and Z axes don't all move in the exact same pattern
    private float noiseOffsetX;
    private float noiseOffsetY;
    private float noiseOffsetZ;

    void Start()
    {
        // Remember the starting rotation
        startRotation = transform.localRotation;

        // Pick a random starting point in the noise wave for each axis
        noiseOffsetX = Random.Range(0f, 1000f);
        noiseOffsetY = Random.Range(0f, 1000f);
        noiseOffsetZ = Random.Range(0f, 1000f);
    }

    void Update()
    {
        // Generate a smooth, flowing value between 0.0 and 1.0 based on time
        float noiseX = Mathf.PerlinNoise(Time.time * rotationSpeed + noiseOffsetX, 0f);
        float noiseY = Mathf.PerlinNoise(Time.time * rotationSpeed + noiseOffsetY, 0f);
        float noiseZ = Mathf.PerlinNoise(Time.time * rotationSpeed + noiseOffsetZ, 0f);

        // Map that 0 to 1 value smoothly between your minimum and maximum Inspector limits
        float angleX = Mathf.Lerp(xLimits.x, xLimits.y, noiseX);
        float angleY = Mathf.Lerp(yLimits.x, yLimits.y, noiseY);
        float angleZ = Mathf.Lerp(zLimits.x, zLimits.y, noiseZ);

        // Apply the smooth rotation
        Quaternion smoothOffset = Quaternion.Euler(angleX, angleY, angleZ);
        transform.localRotation = startRotation * smoothOffset;
    }
}