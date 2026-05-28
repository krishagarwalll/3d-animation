using UnityEngine;

public class PettingSurfaceTracker : MonoBehaviour
{
    [Header("Slide Path Setup")]
    public Transform slideStart; 
    public Transform slideEnd;   
    
    [Header("IK Targets")]
    public Transform handIkTarget; // The target your Two Bone IK uses

    [Header("Stroke Control")]
    [Tooltip("Animate this from 0 to 1 to slide the hand")]
    [Range(0f, 1f)]
    public float strokeProgress = 0f; 

    [Header("Raycast Settings")]
    public LayerMask catLayer; // Make sure your cat is on this layer!
    public float raycastDistance = 1.0f;
    
    // Optional: Offset the rotation so the palm faces the cat correctly
    public Vector3 palmRotationOffset = new Vector3(90, 0, 0);

    void LateUpdate()
    {
        if (slideStart == null || slideEnd == null || handIkTarget == null) return;

        // 1. Calculate the current position along the invisible "rail"
        Vector3 currentHoverPoint = Vector3.Lerp(slideStart.position, slideEnd.position, strokeProgress);

        // 2. Cast a ray downwards from that hover point
        // Note: You may need to change Vector3.down to -slideStart.up if the cat rolls over!
        if (Physics.Raycast(currentHoverPoint, Vector3.down, out RaycastHit hit, raycastDistance, catLayer))
        {
            // 3. Snap the IK Target precisely to the mesh surface
            handIkTarget.position = hit.point;

            // 4. (Bonus) Align the hand's rotation to the slope of the cat's back
            // This assumes your hand's "Up" vector should point away from the cat
            handIkTarget.up = hit.normal;
            handIkTarget.Rotate(palmRotationOffset); 
        }
        else
        {
            // Fallback just in case the raycast misses the cat
            handIkTarget.position = currentHoverPoint;
        }
    }
    private void OnDrawGizmos()
    {
        if (slideStart != null && slideEnd != null)
        {
            // Draw the invisible "Rail" in yellow
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(slideStart.position, slideEnd.position);

            // Draw the Raycast shooting down in red
            Vector3 hover = Vector3.Lerp(slideStart.position, slideEnd.position, strokeProgress);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(hover, Vector3.down * raycastDistance);

        }
    }
}