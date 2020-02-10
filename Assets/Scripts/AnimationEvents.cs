using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    
    public void PlayerFlip_AnimationEvent()
    {
        PlayerController.instance.FlipEnd_AnimationEvent();
    }
}
