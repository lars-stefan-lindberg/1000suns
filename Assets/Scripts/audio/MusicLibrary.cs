using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Audio/Music Library")]
public class MusicLibrary : ScriptableObject
{
    public List<MusicTrack> tracks;

    private Dictionary<string, MusicTrack> lookup;

    public void Init()
    {
        lookup = new Dictionary<string, MusicTrack>();

        foreach (var track in tracks)
        {
            lookup[track.trackId] = track;
        }
    }

    public MusicTrack GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        lookup.TryGetValue(id, out var track);
        return track;
    }
}
