using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed in degrees per second for X, Y, and Z axes.")]
    public Vector3 rotationSpeed = new Vector3(0f, 50f, 0f);

    [Tooltip("Choose whether to rotate relative to its own local axes or the global world axes.")]
    public Space rotationSpace = Space.Self;

    void Update()
    {
        // Multiplying by Time.deltaTime ensures the rotation is perfectly smooth
        // and runs at the exact same speed regardless of the game's frame rate.
        transform.Rotate(rotationSpeed * Time.deltaTime, rotationSpace);
    }
}