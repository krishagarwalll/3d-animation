using System.Collections;
using UnityEngine;

public class AnimationTriggerController : MonoBehaviour
{
    private Animator animator;

    [Header("Animation Settings")]
    [Tooltip("The EXACT name of the Trigger parameter you created")]
    public string triggerParameterName = "PlayHand";

    [Header("Timing Settings")]
    [Tooltip("How long to wait before triggering the animation")]
    public float startDelay = 0f; 

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("No Animator found on this GameObject!");
            return;
        }

        // --- THE FIX IS HERE ---
        // Automatically start the sequence as soon as the game begins!
        FireTrigger(triggerParameterName, startDelay);
    }

    [ContextMenu("Test Trigger (Uses Inspector Values)")]
    public void TestTriggerFromInspector()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("You must be in Play Mode to test the animation!");
            return;
        }

        FireTrigger(triggerParameterName, startDelay);
    }

    public void FireTrigger(string triggerName, float delay)
    {
        if (animator == null) return;
        
        StopAllCoroutines(); 
        StartCoroutine(TriggerRoutine(triggerName, delay));
    }

    private IEnumerator TriggerRoutine(string triggerName, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        animator.SetTrigger(triggerName);
    }
}