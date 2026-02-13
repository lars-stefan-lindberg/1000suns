using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Shared Character Sound Set")]
public class SharedCharacterSoundSet : ScriptableObject
{
    public EventReference jump;
    public EventReference land;
    public EventReference footstep;
    public EventReference genericDeath;
    public EventReference shadowDeath;
    public EventReference spawn;
    public EventReference shapeshift;
    public EventReference mergeSplit;
}
