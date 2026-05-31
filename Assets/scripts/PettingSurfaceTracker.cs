using UnityEngine;

public class PettingSurfaceTracker : MonoBehaviour
{
    [Header("Slide Path Setup")]
    public Transform slideStart; 
    public Transform slideEnd;   
    
    [Header("IK Targets")]
    public Transform handIkTarget; // Assigned to your True_Hand_Target

    [Header("Stroke Control")]
    [Range(0f, 1f)]
    public float strokeProgress = 0f; 

    [Header("Raycast Settings")]
    public LayerMask catLayer; 
    public float raycastDistance = 2.0f;
    
    [Header("Fine-Tuning Offsets")]
    [Tooltip("Local position offset (X=Left/Right, Y=Hover Height, Z=Forward/Backward relative to hand orientation)")]
    public Vector3 handPositionOffset = Vector3.zero;

    [Tooltip("Adjust this to lay the palm perfectly flat against the fur")]
    public Vector3 palmRotationOffset = Vector3.zero;

    void LateUpdate()
    {
        if (slideStart == null || slideEnd == null || handIkTarget == null) return;

        // 1. POSITION: Calculate the base hover position along the track
        Vector3 basePosition = Vector3.Lerp(slideStart.position, slideEnd.position, strokeProgress);

        // Snap base position to the fur collider if it hits
        if (Physics.Raycast(basePosition, Vector3.down, out RaycastHit hit, raycastDistance, catLayer))
        {
            basePosition = hit.point;
        }

        // 2. ROTATION: Interpolate directly between the start and end marker rotations
        Quaternion blendedRotation = Quaternion.Slerp(slideStart.rotation, slideEnd.rotation, strokeProgress);
        Quaternion finalRotation = blendedRotation * Quaternion.Euler(palmRotationOffset);
        
        // Force the target's rotation first so we can use its coordinate space
        handIkTarget.rotation = finalRotation;

        // 3. APPLY LOCAL POSITION OFFSET
        // Multiply the rotation matrix by our local offset vector to convert it into world space directions
        Vector3 worldPositionOffset = finalRotation * handPositionOffset;
        
        // Combine the surface snapped position with our dynamic local offset
        handIkTarget.position = basePosition + worldPositionOffset;
    }

    private void OnDrawGizmos()
    {
        if (slideStart != null && slideEnd != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(slideStart.position, slideEnd.position);

            Vector3 hover = Vector3.Lerp(slideStart.position, slideEnd.position, strokeProgress);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(hover, Vector3.down * raycastDistance);
        }
    }
}