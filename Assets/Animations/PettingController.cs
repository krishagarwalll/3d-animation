using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PettingController : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("The main Animator on your character.")]
    public Animator characterAnimator;
    
    [Tooltip("The Multi-Aim Constraint controlling the torso twist.")]
    public MultiAimConstraint torsoAimConstraint;

    [Tooltip("The IK Constraint controlling the hand (Usually a TwoBoneIKConstraint).")]
    public TwoBoneIKConstraint handIKConstraint;

    [Tooltip("The script that handles snapping the hand to the cat's surface.")]
    public PettingSurfaceTracker tracker; 

    [Header("Animation Settings")]
    [Tooltip("The name of the Trigger parameter in your Animator.")]
    public string pettingTriggerName = "StartPetting";
    
    [Tooltip("How long (in seconds) it takes for her to fully twist and reach out.")]
    public float twistDuration = 1.0f;

    [Header("Procedural Stroke Settings")]
    [Tooltip("How fast the hand slides along the cat.")]
    public float strokeSpeed = 1.5f;          
    [Tooltip("How many times to pet before pulling the hand back.")]
    public int numberOfStrokes = 3;           

    [Header("Auto Trigger Settings")]
    [Tooltip("If true, the sequence will start automatically after the delay.")]
    public bool autoTrigger = false;
    
    [Tooltip("How long (in seconds) to wait before automatically starting the petting sequence.")]
    public float delayBeforePetting = 3.0f;

    private bool isPetting = false;

    private void Start()
    {
        // 1. Ensure both the torso AND the hand are completely turned off while she walks/sits
        if (torsoAimConstraint != null) torsoAimConstraint.weight = 0f;
        if (handIKConstraint != null) handIKConstraint.weight = 0f;
        if (tracker != null) tracker.strokeProgress = 0f;

        // If auto-trigger is enabled, start the countdown immediately
        if (autoTrigger)
        {
            StartCoroutine(AutoTriggerRoutine());
        }
    }

    private IEnumerator AutoTriggerRoutine()
    {
        yield return new WaitForSeconds(delayBeforePetting);
        TriggerPettingSequence();
    }

    public void TriggerPettingSequence()
    {
        // Prevent triggering multiple times concurrently
        if (isPetting) return;

        if (characterAnimator == null || torsoAimConstraint == null || handIKConstraint == null || tracker == null)
        {
            Debug.LogError("PettingController: Missing Component references!");
            return;
        }

        StartCoroutine(TwistAndPetRoutine());
    }

    private IEnumerator TwistAndPetRoutine()
    {
        isPetting = true;
        float elapsedTime = 0f;
        
        float startTorsoWeight = torsoAimConstraint.weight;
        float startHandWeight = handIKConstraint.weight;

        // --- PHASE 1: REACH AND TWIST ---
        // Tell the Animator to start playing any base body adjustments
        if (!string.IsNullOrEmpty(pettingTriggerName))
        {
            characterAnimator.SetTrigger(pettingTriggerName);
        }

        // Smoothly increase BOTH IK weights over time
        while (elapsedTime < twistDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / twistDuration);
            
            torsoAimConstraint.weight = Mathf.Lerp(startTorsoWeight, 1f, t);
            handIKConstraint.weight = Mathf.Lerp(startHandWeight, 1f, t);
            
            yield return null; 
        }

        // --- PHASE 2: PROCEDURAL STROKING ---
        for (int i = 0; i < numberOfStrokes; i++)
        {
            float progress = 0f;
            
            // Stroke Down the back
            while (progress < 1f)
            {
                progress += Time.deltaTime * strokeSpeed;
                tracker.strokeProgress = Mathf.SmoothStep(0f, 1f, progress);
                yield return null;
            }

            // Return hand to the top of the head
            progress = 0f;
            while (progress < 1f)
            {
                // Returning slightly faster than stroking for a natural cadence
                progress += Time.deltaTime * (strokeSpeed * 1.5f); 
                tracker.strokeProgress = Mathf.SmoothStep(1f, 0f, progress);
                yield return null;
            }
        }

        // --- PHASE 3: PULL BACK AND UNTWIST ---
        elapsedTime = 0f;
        while (elapsedTime < twistDuration)
        {
            elapsedTime += Time.deltaTime;
            // Easing from 1 back down to 0
            float t = Mathf.SmoothStep(1f, 0f, elapsedTime / twistDuration);
            
            torsoAimConstraint.weight = t;
            handIKConstraint.weight = t;
            
            yield return null; 
        }

        // Ensure weights are cleanly zeroed out and flag is reset
        torsoAimConstraint.weight = 0f;
        handIKConstraint.weight = 0f;
        isPetting = false;
    }
}