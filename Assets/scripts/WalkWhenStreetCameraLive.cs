using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Moves a set of NPCs forward (along their own facing direction) while the
/// Street Cinemachine camera is the live/active camera. As soon as the
/// CinemachineBrain blends to the assigned street camera, the NPCs start
/// walking; when another camera becomes live they stop.
///
/// The NPCs are expected to already be playing an in-place walk animation
/// (root motion off) - this script only supplies the forward translation so
/// the feet match the movement.
/// </summary>
public class WalkWhenStreetCameraLive : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("When this Cinemachine camera becomes live, the NPCs walk.")]
    public CinemachineCamera streetCamera;

    [Tooltip("CinemachineBrain that drives the main camera. Auto-found if left empty.")]
    public CinemachineBrain brain;

    [Header("Walkers")]
    [Tooltip("Root transforms to move. Each moves along its own (flattened) forward axis.")]
    public Transform[] walkers;

    [Header("Motion")]
    [Tooltip("Walk speed in metres per second (tune to match the walk cycle so feet don't slide).")]
    public float speed = 1.3f;

    [Tooltip("Keep walking once the street camera has been live, even after it cuts away.")]
    public bool keepWalkingAfterFirstActivation = false;

    [Tooltip("Reverse the travel direction if an NPC walks backwards.")]
    public bool invertDirection = false;

    bool _hasBeenLive;

    void Awake()
    {
        if (brain == null)
        {
            if (Camera.main != null)
                brain = Camera.main.GetComponent<CinemachineBrain>();
#if UNITY_2023_1_OR_NEWER
            if (brain == null)
                brain = Object.FindFirstObjectByType<CinemachineBrain>();
#else
            if (brain == null)
                brain = Object.FindObjectOfType<CinemachineBrain>();
#endif
        }
    }

    bool StreetIsLive()
    {
        if (brain == null || streetCamera == null)
            return false;
        return brain.ActiveVirtualCamera as CinemachineCamera == streetCamera;
    }

    void Update()
    {
        bool live = StreetIsLive();
        if (live)
            _hasBeenLive = true;

        bool shouldWalk = live || (keepWalkingAfterFirstActivation && _hasBeenLive);
        if (!shouldWalk || walkers == null)
            return;

        float step = speed * (invertDirection ? -1f : 1f) * Time.deltaTime;
        foreach (var w in walkers)
        {
            if (w == null)
                continue;
            Vector3 fwd = w.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 1e-6f)
                continue;
            fwd.Normalize();
            w.position += fwd * step;
        }
    }
}
