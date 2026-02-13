using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/Music Track")]
public class MusicTrack : ScriptableObject
{
    [Tooltip("Unique ID used for saving/loading")]
    public string trackId;

    public EventReference eventRef;

    [Header("Optional Ending")]
    public bool hasEnding;
    public string endingParameterName = "sequenceCompleted";
}
