using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DistanceAnimationTrigger : MonoBehaviour
{
    [Header("Component References")]
    public Animator animator;
    [Tooltip("Drag the other character's Transform here.")]
    public Transform targetCharacter;

    [Header("Settings")]
    [Tooltip("How close the character needs to be to trigger the animation.")]
    public float triggerDistance = 5f;

    // To prevent firing the trigger every single frame they are near
    private bool hasTriggered = false; 

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (targetCharacter == null) return;

        // Calculate the distance between this object and the target
        float distance = Vector3.Distance(transform.position, targetCharacter.position);

        if (distance <= triggerDistance && !hasTriggered)
        {
            animator.SetTrigger("IsNear");
            hasTriggered = true; // Lock it so it only fires once
        }
        else if (distance > triggerDistance && hasTriggered)
        {
            // Reset the lock if the character walks away, so it can happen again later
            hasTriggered = false; 
        }
    }
}