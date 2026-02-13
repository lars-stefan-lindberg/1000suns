using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Eli Sound Set")]
public class EliSoundSet : ScriptableObject
{
    public EventReference forcePushStart;
    public EventReference forcePushRelease;
    public EventReference heavyLand;
    public EventReference longFall;
    public EventReference shapeshiftToBlob;
    public EventReference shapeshiftToHuman;
    public EventReference forcePushJump;
    public EventReference forcePushLand;
}
