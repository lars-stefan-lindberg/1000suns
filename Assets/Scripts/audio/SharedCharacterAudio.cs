using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SharedCharacterAudio : MonoBehaviour
{
    [SerializeField] private SharedCharacterSoundSet sounds;

    private PARAMETER_ID surfaceParamId;

    void Awake()
    {
        if (sounds == null)
            Debug.LogWarning("SharedPlayerAudio has no sound set assigned");
        CacheSurfaceParameter();
    }

    public void PlayJump()
    {
        SoundFXManager.obj.PlayAtPosition(sounds.jump, transform.position);
    }

    public void PlayLand(SurfaceType surface)
    {
        SoundFXManager.obj.PlayAtGameObject(
            sounds.land,
            gameObject,
            inst =>
            {
                inst.setParameterByID(surfaceParamId, (float)surface);
            }
        );
    }

    public void PlayFootstep(SurfaceType surface)
    {
        SoundFXManager.obj.PlayAtGameObject(
            sounds.footstep,
            gameObject,
            inst =>
            {
                inst.setParameterByID(surfaceParamId, (float)surface);
            }
        );
    }

    public void PlayGenericDeath(Transform transform) {
        SoundFXManager.obj.PlayAtPosition(sounds.genericDeath, transform.position);
    }

    public void PlayShadowDeath(Transform transform) {
        SoundFXManager.obj.PlayAtPosition(sounds.shadowDeath, transform.position);
    }

    public void PlaySpawn(Transform transform) {
        SoundFXManager.obj.PlayAtPosition(sounds.spawn, transform.position);
    }

    public void PlayShapeshift() {
        SoundFXManager.obj.PlayAtPosition(sounds.shapeshift, transform.position);
    }

    public void PlayMergeSplit(ref EventInstance mergeSplitInstance) {
        mergeSplitInstance = SoundFXManager.obj.CreateAttachedInstance(sounds.mergeSplit, gameObject);
        mergeSplitInstance.start();
        mergeSplitInstance.release();
    }

    void CacheSurfaceParameter()
    {
        if (!sounds.footstep.IsNull)
        {
            var inst = RuntimeManager.CreateInstance(sounds.footstep);
            inst.getDescription(out var desc);
            desc.getParameterDescriptionByName(
                "surface_index",
                out var paramDesc
            );
            surfaceParamId = paramDesc.id;
            inst.release();
        }
    }
}
