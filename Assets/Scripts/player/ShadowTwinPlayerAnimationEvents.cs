using UnityEngine;

public class ShadowTwinPlayerAnimationEvents : MonoBehaviour
{
    private SharedCharacterAudio _sharedPlayerAudio;

    void Awake()
    {
        _sharedPlayerAudio = transform.parent.gameObject.GetComponent<SharedCharacterAudio>();
    }

    public void PlayFootstep() {
        _sharedPlayerAudio.PlayFootstep(ShadowTwinPlayer.obj.surface);
    }

    public void JumpSqueeze() {
        ShadowTwinMovement.obj.JumpSqueeze();
    }

    public void ToTwin() {
        ShadowTwinMovement.obj.ToTwin();
    }

    public void ToTwinBlob() {
        ShadowTwinMovement.obj.ToBlob();
    }
}
