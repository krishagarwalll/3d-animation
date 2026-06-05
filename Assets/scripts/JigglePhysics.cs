using UnityEngine;

public class JigglePhysics : MonoBehaviour
{
    [Header("Jiggle Settings")]
    [Tooltip("How much the hair trails behind movement.")]
    public float stiffness = 0.1f;
    [Tooltip("How heavy the hair is / how fast it settles.")]
    public float mass = 0.9f;
    [Tooltip("How much it bounces back and forth.")]
    public float damping = 0.7f;
    [Tooltip("Pulls the hair down like gravity.")]
    public float gravity = 0.75f;

    private Vector3 targetPos;
    private Vector3 dynamicPos;
    private Vector3 boneVelocity;
    
    private Vector3 localRestPosition;
    private Quaternion localRestRotation;

    void Start()
    {
        // Remember where the hair is supposed to sit naturally
        localRestPosition = transform.localPosition;
        localRestRotation = transform.localRotation;
        
        dynamicPos = transform.position;
    }

    void LateUpdate()
    {
        // 1. Where the bone WANTS to be based on the head's animation
        transform.localPosition = localRestPosition;
        targetPos = transform.position;

        // 2. Calculate the spring math
        Vector3 force = (targetPos - dynamicPos) * stiffness;
        force.y -= gravity * Time.deltaTime; // Apply a little gravity
        
        boneVelocity = (boneVelocity + force / mass) * damping;
        dynamicPos += boneVelocity;

        // 3. Point the bone toward the dragged physics point
        Vector3 direction = dynamicPos - transform.parent.position;
        transform.rotation = Quaternion.FromToRotation(transform.up, direction) * transform.rotation;
        
        // Lock the position back to the parent so it doesn't detach from the head
        transform.position = targetPos; 
    }
}