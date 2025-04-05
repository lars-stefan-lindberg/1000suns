using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public void PlayDefaultRunStep() {
        SoundFXManager.obj.PlayStep(Player.obj.surface, gameObject.transform, 1f);
    }

    public void PlaySubtleRunStep() {
        SoundFXManager.obj.PlayStep(Player.obj.surface, gameObject.transform, 0.8f);
    }

    public void ToBlob() {
        PlayerMovement.obj.ToBlob();
    }

    public void JumpSqueeze() {
        PlayerMovement.obj.JumpSqueeze();
    }
}
