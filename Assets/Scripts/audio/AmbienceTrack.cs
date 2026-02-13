using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Ambience Track")]
public class AmbienceTrack : ScriptableObject
{
    public string ambienceId;
    public EventReference eventRef;
}
