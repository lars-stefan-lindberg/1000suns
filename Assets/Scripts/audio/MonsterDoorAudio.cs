using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MonsterDoorAudio : MonoBehaviour
{
    [SerializeField] private MonsterDoorSoundSet sounds;

    private PARAMETER_ID soulsAbsorbed;

    void Awake()
    {
        if (sounds == null)
            Debug.LogWarning("MonsterDoorAudio has no sound set assigned");
        CacheSoulsAbsorbedParameter();
    }

    public void PlayDestroy()
    {
        SoundFXManager.obj.PlayAtPosition(sounds.destroy, transform.position);
    }

    public void PlaySoulAbsorbed(int numberOfSoulsAbsorbed)
    {
        SoundFXManager.obj.PlayAtGameObject(
            sounds.soulAbsorb,
            gameObject,
            inst =>
            {
                inst.setParameterByID(soulsAbsorbed, numberOfSoulsAbsorbed);
            }
        );
    }

    void CacheSoulsAbsorbedParameter()
    {
        if (!sounds.soulAbsorb.IsNull)
        {
            var inst = RuntimeManager.CreateInstance(sounds.soulAbsorb);
            inst.getDescription(out var desc);
            desc.getParameterDescriptionByName(
                "soulsAbsorbed",
                out var paramDesc
            );
            soulsAbsorbed = paramDesc.id;
            inst.release();
        }
    }
}
