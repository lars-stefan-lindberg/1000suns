using UnityEngine;

public class BlobAudio : MonoBehaviour
{
    [SerializeField] private BlobSoundSet _sounds;

    public void PlayLand() {
        SoundFXManager.obj.PlayAtPosition(_sounds.land, transform.position);
    }

    public void PlayJump() {
        SoundFXManager.obj.PlayAtPosition(_sounds.jump, transform.position);
    }

    public void PlayFootstep() {
        SoundFXManager.obj.PlayAtPosition(_sounds.footstep, transform.position);
    }

    public void PlayShapeshift() {
        SoundFXManager.obj.PlayAtPosition(_sounds.shapeshift, transform.position);
    }
}
