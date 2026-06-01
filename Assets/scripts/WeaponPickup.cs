using UnityEngine;

public class WeaponEquipper : MonoBehaviour
{
    [Header("References")]
    public Transform weaponSocket; // Drag your left hand socket here
    public GameObject knifeObject; // Drag the knife from the scene here

    void Start()
    {
        // This runs automatically exactly once when you press Play
        if (knifeObject != null && weaponSocket != null)
        {
            EquipWeapon(knifeObject);
        }
        else
        {
            Debug.LogWarning("Wait! You forgot to drag the Socket or the Knife into the Inspector slots.");
        }
    }

    private void EquipWeapon(GameObject weapon)
    {
        // 1. Turn off physics so it doesn't fall to the floor
        Rigidbody rb = weapon.GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; 
        }

        // 2. Parent the weapon to the hand socket
        weapon.transform.SetParent(weaponSocket);

        // 3. Snap the weapon to the socket's exact position and rotation
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }
}