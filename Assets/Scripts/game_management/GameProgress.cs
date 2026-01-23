using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameProgress
{   
    [SerializeField] private List<string> completedEventIds = new();

    private HashSet<string> completedEventSet;

    void EnsureSet()
    {
        if (completedEventSet == null)
            completedEventSet = new HashSet<string>(completedEventIds);
    }

    public bool HasEvent(GameEventId eventId)
    {
        EnsureSet();
        return completedEventSet.Contains(eventId.id);
    }

    public void RegisterEvent(GameEventId eventId)
    {
        EnsureSet();

        if (completedEventSet.Add(eventId.id))
            completedEventIds.Add(eventId.id);
    }

    public void Clear()
    {
        completedEventIds.Clear();
        completedEventSet = null;
    }

    public void ImportCompletedEvents(List<string> events)
    {
        completedEventIds = events;
        if(completedEventSet == null)
            EnsureSet();
        else {
            completedEventSet.Clear();
            completedEventSet.UnionWith(completedEventIds);
        }
    }

    public IReadOnlyList<string> GetCompletedEventIds()
    {
        return completedEventIds;
    }
}
