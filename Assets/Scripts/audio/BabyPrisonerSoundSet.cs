using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Baby Prisoner Sound Set")]
public class BabyPrisonerSoundSet : ScriptableObject
{
    public EventReference crawl;
    public EventReference alert;
    public EventReference despawn;
    public EventReference idle;
    public EventReference escapeLoop;
    public EventReference jumpedOn;
    public EventReference angryAfterJumpedOn;
    public EventReference cuteGreeting;
}
