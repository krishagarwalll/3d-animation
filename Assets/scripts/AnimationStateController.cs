using System.Collections;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;

    [Header("Animation Settings")]
    [Tooltip("The EXACT name of the state block in the Animator (Case-sensitive!)")]
    public string targetStateName = "LookingHand";
    
    [Tooltip("The index of the layer where this state lives")]
    public int layerIndex = 2; 

    [Header("Timing Settings")]
    [Tooltip("How long the smooth blend into the new animation takes (in seconds)")]
    public float transitionDuration = 0.5f;
    
    [Tooltip("How long to wait before starting the transition")]
    public float startDelay = 0f; 

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("No Animator found on this GameObject!");
        }
    }

    // Right-click this component in the Inspector to test it
    [ContextMenu("Test Play Animation (Uses Inspector Values)")]
    public void TestCrossFadeFromInspector()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("You must be in Play Mode to test the animation!");
            return;
        }

        PlayAnimation(targetStateName, transitionDuration, startDelay);
    }

    // Call this from other scripts to play any animation with a delay
    public void PlayAnimation(string stateName, float duration, float delay)
    {
        if (animator == null) return;
        
        StopAllCoroutines(); 
        StartCoroutine(CrossFadeRoutine(stateName, duration, delay));
    }

    private IEnumerator CrossFadeRoutine(string stateName, float duration, float delay)
    {
        // 1. Wait for the delay
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        // 2. Smoothly blend into the target animation state
        animator.CrossFade(stateName, duration, layerIndex);
        
        Debug.Log($"CrossFading to state: '{stateName}' on layer: {layerIndex}");
    }
}