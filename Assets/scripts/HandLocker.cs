using UnityEngine;

// 1. Force this script to execute AFTER Timeline and the Animator evaluate
[DefaultExecutionOrder(1000)]
public class HandLocker : MonoBehaviour
{
    [System.Serializable]
    public struct BoneLock
    {
        public Transform bone;
        // 2. Removed [HideInInspector] so Unity can permanently save the baked pose
        public Quaternion lockedRotation; 
    }

    public BoneLock[] fingerBones;

    // 3. Right-click the script in the Inspector to save your pose permanently!
    [ContextMenu("Bake Hand Pose")]
    public void BakePose()
    {
        for (int i = 0; i < fingerBones.Length; i++)
        {
            if (fingerBones[i].bone != null)
            {
                fingerBones[i].lockedRotation = fingerBones[i].bone.localRotation;
            }
        }
        Debug.Log("Hand pose baked successfully!");
    }

    void Awake()
    {
        // Fallback: If you forget to bake, it will try to capture the pose at runtime.
        // (A completely uninitialized empty Quaternion evaluates to 0,0,0,0)
        for (int i = 0; i < fingerBones.Length; i++)
        {
            if (fingerBones[i].bone != null && fingerBones[i].lockedRotation == new Quaternion(0, 0, 0, 0))
            {
                fingerBones[i].lockedRotation = fingerBones[i].bone.localRotation;
            }
        }
    }

    void LateUpdate()
    {
        // Enforces your custom pose every frame, easily overriding the Timeline's tracks
        for (int i = 0; i < fingerBones.Length; i++)
        {
            if (fingerBones[i].bone != null)
            {
                fingerBones[i].bone.localRotation = fingerBones[i].lockedRotation;
            }
        }
    }
}