using UnityEngine;

public class ShadowTwinPlayerAnimationEvents : MonoBehaviour
{
    public void PlayDefaultRunStep() {
        SoundFXManager.obj.PlayStep(ShadowTwinPlayer.obj.surface, gameObject.transform, 1f);
    }

    public void PlaySubtleRunStep() {
        SoundFXManager.obj.PlayStep(ShadowTwinPlayer.obj.surface, gameObject.transform, 0.8f);
    }

    public void JumpSqueeze() {
        ShadowTwinMovement.obj.JumpSqueeze();
    }
}
