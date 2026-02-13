using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Prisoner Sound Set")]
public class PrisonerSoundSet : ScriptableObject
{
    public EventReference crawl;
    public EventReference death;
    public EventReference hit;
    public EventReference slide;
    public EventReference spawn;
}
