using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Audio/Ambience Library")]
public class AmbienceLibrary : ScriptableObject
{
    public List<AmbienceTrack> tracks;

    private Dictionary<string, AmbienceTrack> lookup;

    public void Init()
    {
        lookup = new Dictionary<string, AmbienceTrack>();

        foreach (var track in tracks)
        {
            lookup[track.ambienceId] = track;
        }
    }

    public AmbienceTrack GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        lookup.TryGetValue(id, out var track);
        return track;
    }
}
