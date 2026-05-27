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
    
    [Header("Animation Settings")]
    [Tooltip("The name of the Trigger parameter in your Animator.")]
    public string pettingTriggerName = "StartPetting";
    
    [Tooltip("How long (in seconds) it takes for her to fully twist and reach out.")]
    public float twistDuration = 1.0f;

    [Header("Auto Trigger Settings")]
    [Tooltip("If true, the sequence will start automatically after the delay.")]
    public bool autoTrigger = false;
    
    [Tooltip("How long (in seconds) to wait before automatically starting the petting sequence.")]
    public float delayBeforePetting = 3.0f;

    private void Start()
    {
        // 1. Ensure both the torso AND the hand are completely turned off while she walks/sits
        if (torsoAimConstraint != null) torsoAimConstraint.weight = 0f;
        if (handIKConstraint != null) handIKConstraint.weight = 0f;

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
        if (characterAnimator == null || torsoAimConstraint == null || handIKConstraint == null)
        {
            Debug.LogError("PettingController: Missing Animator or Constraint references!");
            return;
        }

        StartCoroutine(TwistAndPetRoutine());
    }

    private IEnumerator TwistAndPetRoutine()
    {
        float elapsedTime = 0f;
        
        float startTorsoWeight = torsoAimConstraint.weight;
        float startHandWeight = handIKConstraint.weight;

        // Tell the Animator to start playing the arm movement
        characterAnimator.SetTrigger(pettingTriggerName);

        // Smoothly increase BOTH IK weights over time
        while (elapsedTime < twistDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Mathf.SmoothStep gives us a nice ease-in, ease-out curve
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / twistDuration);
            
            torsoAimConstraint.weight = Mathf.Lerp(startTorsoWeight, 1f, t);
            handIKConstraint.weight = Mathf.Lerp(startHandWeight, 1f, t);
            
            yield return null; 
        }

        // Ensure both weights are locked to exactly 1 when finished
        torsoAimConstraint.weight = 1f;
        handIKConstraint.weight = 1f;
    }
}