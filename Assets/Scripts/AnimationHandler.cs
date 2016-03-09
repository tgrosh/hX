using UnityEngine;
using System.Collections;

public class AnimationHandler : MonoBehaviour {
    public Animator animator;
    public delegate void AnimationComplete(AnimationType animationType);
    public static event AnimationComplete OnAnimationComplete;
    
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
    FleetVesselEnter
}
