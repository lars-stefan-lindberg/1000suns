using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public abstract class DialogueSoundEffectPlayer : MonoBehaviour
{
    protected PARAMETER_ID emotionIndexParamId;

    protected abstract EventReference GetDialogueSound();

    protected virtual void Awake()
    {
        CacheEmotionParameter();
    }

    public virtual void Play(DialogueSoundEffect soundEffect, GameObject portraitGameObject)
    {
        if (soundEffect == DialogueSoundEffect.None) return;
        if (portraitGameObject == null) return;

        EventReference dialogueSound = GetDialogueSound();
        if (dialogueSound.IsNull) return;

        SoundFXManager.obj.PlayAtGameObject(
            dialogueSound,
            portraitGameObject,
            inst =>
            {
                inst.setParameterByID(emotionIndexParamId, (float)soundEffect);
            }
        );
    }

    void CacheEmotionParameter()
    {
        EventReference dialogueSound = GetDialogueSound();
        if (!dialogueSound.IsNull)
        {
            var inst = RuntimeManager.CreateInstance(dialogueSound);
            inst.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position)); //Required by FMOD even though we don't need it here (gives log warning if not set)
            inst.getDescription(out var desc);
            desc.getParameterDescriptionByName(
                "emotion_index",
                out var paramDesc
            );
            emotionIndexParamId = paramDesc.id;
            inst.release();
        }
    }
}
