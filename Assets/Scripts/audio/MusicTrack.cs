using UnityEngine;
using FMODUnity;

[System.Serializable]
public class MusicParameter
{
    public string parameterName;
    public float value;
}

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
