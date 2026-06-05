using FMODUnity;
using UnityEngine;

public class DeeDialogueSoundEffectPlayer : DialogueSoundEffectPlayer
{
    [SerializeField] private EventReference soundSet;

    protected override EventReference GetDialogueSound()
    {
        return soundSet;
    }
}
