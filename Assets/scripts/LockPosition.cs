using UnityEngine;

// This line forces Unity to only allow this script on an object with an Animator
[RequireComponent(typeof(Animator))]
public class LockPosition : MonoBehaviour
{
    [Header("Lock Settings")]
    public bool lockPosition = true;
    public bool lockRotation = false;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        // Record the exact starting spot
        startPos = transform.position;
        startRot = transform.rotation;
    }

    // OnAnimatorMove completely intercepts the Mixamo Root Motion engine
    void OnAnimatorMove()
    {
        if (lockPosition)
        {
            // Force the position back to start
            transform.position = startPos;
        }

        if (lockRotation)
        {
            // Force the rotation back to start
            transform.rotation = startRot;
        }
    }
}