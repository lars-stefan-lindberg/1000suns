using FMODUnity;
using UnityEngine;

public class EliDialogueSoundEffectPlayer : DialogueSoundEffectPlayer
{
    [SerializeField] private EventReference soundSet;

    protected override EventReference GetDialogueSound()
    {
        return soundSet;
    }
}
