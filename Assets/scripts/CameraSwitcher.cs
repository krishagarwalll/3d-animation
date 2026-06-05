using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Switches between multiple cameras at specified times.
/// Attach to an empty GameObject in the scene.
/// </summary>
public class CameraSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class CameraShot
    {
        public Camera camera;
        [Tooltip("Time in seconds when the sequence switches TO this camera.")]
        public float startTime;
        [Tooltip("Duration of the cross-fade blend into this camera.")]
        public float blendDuration = 1.5f;
    }

    [Header("Camera Sequence")]
    [Tooltip("Add cameras here. The first camera in the list will start immediately at time 0, regardless of its start time.")]
    public List<CameraShot> sequence = new List<CameraShot>();

    private float timer = 0f;
    private int currentIndex = 0;

    private float blendTimer = 0f;
    private bool isBlending = false;
    
    private Camera previousCamera;
    private Camera activeCamera;

    void Start()
    {
        if (sequence == null || sequence.Count == 0)
        {
            Debug.LogWarning("CameraSwitcher: No cameras assigned to the sequence!");
            return;
        }

        // Automatically sort the list by startTime just in case they are out of order in the Inspector
        sequence = sequence.OrderBy(shot => shot.startTime).ToList();

        // Disable all cameras initially
        foreach (var shot in sequence)
        {
            if (shot.camera != null) 
                shot.camera.enabled = false;
        }

        // Enable the very first camera to kick off the scene
        activeCamera = sequence[0].camera;
        if (activeCamera != null)
        {
            activeCamera.enabled = true;
        }
    }

    void Update()
    {
        // Stop updating if we've reached the last camera
        if (sequence.Count <= 1 || currentIndex >= sequence.Count - 1) 
            return;

        timer += Time.deltaTime;
        CameraShot nextShot = sequence[currentIndex + 1];

        // Check if it's time to trigger the next camera
        if (!isBlending && timer >= nextShot.startTime)
        {
            StartSwitch(nextShot);
        }

        // Handle the blending logic
        if (isBlending)
        {
            blendTimer += Time.deltaTime;
            float t = Mathf.Clamp01(blendTimer / nextShot.blendDuration);

            // Use smooth step for a nicer blend curve
            t = t * t * (3f - 2f * t);

            if (t >= 1f)
            {
                CompleteSwitch();
            }
        }
    }

    void StartSwitch(CameraShot nextShot)
    {
        isBlending = true;
        blendTimer = 0f;

        previousCamera = sequence[currentIndex].camera;
        activeCamera = nextShot.camera;

        if (activeCamera != null)
        {
            activeCamera.enabled = true;
            
            // Draw the new camera on top of the old one during the blend
            if (previousCamera != null)
            {
                activeCamera.depth = previousCamera.depth + 1;
            }
        }
    }

    void CompleteSwitch()
    {
        if (previousCamera != null)
        {
            previousCamera.enabled = false;
        }

        isBlending = false;
        currentIndex++;
    }

    /// <summary>
    /// Force the sequence to jump to the next camera immediately.
    /// </summary>
    public void TriggerNextSwitchManually()
    {
        if (currentIndex < sequence.Count - 1)
        {
            timer = sequence[currentIndex + 1].startTime;
        }
    }
}