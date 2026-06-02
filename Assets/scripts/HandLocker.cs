using UnityEngine;

public class HandLocker : MonoBehaviour
{
    [System.Serializable]
    public struct BoneLock
    {
        public Transform bone;
        [HideInInspector] public Quaternion lockedRotation;
    }

    public BoneLock[] fingerBones;

    void Awake()
    {
        // Automatically records the exact pose you made in the editor
        for (int i = 0; i < fingerBones.Length; i++)
        {
            if (fingerBones[i].bone != null)
            {
                fingerBones[i].lockedRotation = fingerBones[i].bone.localRotation;
            }
        }
    }

    void LateUpdate()
    {
        // Enforces your custom pose every frame, overriding the Animator
        for (int i = 0; i < fingerBones.Length; i++)
        {
            if (fingerBones[i].bone != null)
            {
                fingerBones[i].bone.localRotation = fingerBones[i].lockedRotation;
            }
        }
    }
}