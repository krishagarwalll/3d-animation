using UnityEngine;

/// <summary>
/// Switches between two cameras at a specified time (when petting animation begins).
/// Attach to an empty GameObject in the scene.
/// </summary>
public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    public Camera protagonistCamera;
    public Camera handPettingCamera;

    [Header("Switch Timing")]
    [Tooltip("Time in seconds when the petting starts and camera switches")]
    public float switchTime = 3.0f;

    [Tooltip("Duration of the cross-fade blend between cameras")]
    public float blendDuration = 1.5f;

    [Header("State")]
    public bool hasSwitched = false;

    private float timer = 0f;
    private float blendTimer = 0f;
    private bool isBlending = false;

    void Start()
    {
        if (protagonistCamera != null)
            protagonistCamera.enabled = true;
        if (handPettingCamera != null)
            handPettingCamera.enabled = false;
        hasSwitched = false;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!hasSwitched && timer >= switchTime)
        {
            StartSwitch();
        }

        if (isBlending)
        {
            blendTimer += Time.deltaTime;
            float t = Mathf.Clamp01(blendTimer / blendDuration);

            // Use smooth step for nicer blend curve
            t = t * t * (3f - 2f * t);

            if (t >= 1f)
            {
                // Blend complete
                protagonistCamera.enabled = false;
                handPettingCamera.enabled = true;
                handPettingCamera.depth = 1;
                isBlending = false;
            }
        }
    }

    void StartSwitch()
    {
        hasSwitched = true;
        isBlending = true;
        blendTimer = 0f;

        // Enable both cameras during blend
        handPettingCamera.enabled = true;
        handPettingCamera.depth = protagonistCamera.depth + 1;
    }

    /// <summary>
    /// Call this from an Animation Event or Timeline signal to trigger the switch manually
    /// </summary>
    public void TriggerSwitch()
    {
        if (!hasSwitched)
        {
            timer = switchTime;
        }
    }
}
