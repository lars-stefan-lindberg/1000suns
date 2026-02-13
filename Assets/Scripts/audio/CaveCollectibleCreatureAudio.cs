using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class CaveCollectibleCreatureAudio : MonoBehaviour
{
    [SerializeField] private EventReference _creatureSound;
    private PARAMETER_ID creatureIndexParam;

    void Awake()
    {
        if (_creatureSound.IsNull)
            Debug.LogWarning("CaveCollectibleCreatureAudio has no sound set assigned");
        CacheCreatureIndexParameter();
    }

    public void PlayCreatureSound(int creatureIndex) {
        SoundFXManager.obj.PlayAtGameObject(
            _creatureSound,
            gameObject,
            inst =>
            {
                inst.setParameterByID(creatureIndexParam, creatureIndex);
            }
        );
    }

    void CacheCreatureIndexParameter()
    {
        if (!_creatureSound.IsNull)
        {
            var inst = RuntimeManager.CreateInstance(_creatureSound);
            inst.getDescription(out var desc);
            desc.getParameterDescriptionByName(
                "creature_index",
                out var paramDesc
            );
            creatureIndexParam = paramDesc.id;
            inst.release();
        }
    }
}
