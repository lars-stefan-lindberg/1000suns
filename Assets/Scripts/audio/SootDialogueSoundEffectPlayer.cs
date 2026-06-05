using FMODUnity;
using UnityEngine;

public class SootDialogueSoundEffectPlayer : DialogueSoundEffectPlayer
{
    [SerializeField] private EventReference soundSet;

    protected override EventReference GetDialogueSound()
    {
        return soundSet;
    }
}
