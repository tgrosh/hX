using UnityEngine;
using System.Collections;

public class AnimationHandler : MonoBehaviour {
    public Animator animator;

    public delegate void AnimationComplete(AnimationType animationType);
    public event AnimationComplete OnAnimationComplete;
    
    void AnimationEvent(AnimationType animationType)
    {
        if (OnAnimationComplete != null)
        {
            OnAnimationComplete(animationType);
        }
    }
}

public enum AnimationType
{
    FleetVesselEnter,
    FleetVesselWormholeEnter,
    FleetVesselWormholeExit
}
