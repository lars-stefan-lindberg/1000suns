using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Monster Door Sound Set")]
public class MonsterDoorSoundSet : ScriptableObject
{
    public EventReference destroy;
    public EventReference soulAbsorb;
}
