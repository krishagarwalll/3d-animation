using System.Collections;
using UnityEngine;

public class LayerWeightController : MonoBehaviour
{
    private Animator animator;

    [Header("Layer Settings")]
    [Tooltip("The index of your animation layer (Base Layer is 0, next is 1, etc.)")]
    public int layerIndex = 1; 

    [Header("Weight Settings (Inspector Control)")]
    [Range(0f, 1f)]
    public float startWeight = 0f;
    
    [Range(0f, 1f)]
    public float endWeight = 1f;

    [Header("Timing Settings")]
    [Tooltip("How long the transition should take in seconds")]
    public float transitionDuration = 1.0f;
    
    [Tooltip("How long to wait before the transition starts")]
    public float startDelay = 0f; 

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("No Animator found on this GameObject!");
            return;
        }
    }

    // --- THE FIX ---
    // This allows you to test the script by right-clicking the component in the Inspector!
    [ContextMenu("Test Fade (Uses Inspector Values)")]
    public void TestFadeFromInspector()
    {
        // Don't run if we aren't in Play mode
        if (!Application.isPlaying)
        {
            Debug.LogWarning("You must be in Play Mode to test the animation!");
            return;
        }

        FadeLayer(startWeight, endWeight, startDelay);
    }

    public void FadeLayer(float start, float end, float delay)
    {
        if (animator == null || layerIndex >= animator.layerCount) return;

        StopAllCoroutines(); 
        StartCoroutine(SmoothTransition(start, end, transitionDuration, delay));
    }

    private IEnumerator SmoothTransition(float start, float end, float duration, float delay)
    {
        // Snap to start weight
        animator.SetLayerWeight(layerIndex, start);

        // Wait for delay
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        // Smooth transition
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            float currentWeight = Mathf.Lerp(start, end, t);
            animator.SetLayerWeight(layerIndex, currentWeight);
            
            timeElapsed += Time.deltaTime;
            yield return null; 
        }

        // Snap to end weight
        animator.SetLayerWeight(layerIndex, end);
    }
}