using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelTracker : MonoBehaviour
{
    // Dictionary to store level IDs and their corresponding start times
    private Dictionary<string, DateTime> levelStartTimes = new Dictionary<string, DateTime>();
    
    // Singleton instance
    public static LevelTracker obj;
    
    private void Awake()
    {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }

    // Start tracking time for a specific level
    public void StartTimeTracking(string levelId)
    {
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning("Cannot start time tracking: Level ID is null or empty");
            return;
        }

        // Start tracking time for this level
        levelStartTimes[levelId] = DateTime.Now;
        Debug.Log($"Started time tracking for level: {levelId}");
    }
    
    // Stop tracking time for a specific level
    public void StopTimeTracking(string levelId)
    {
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning("Cannot stop time tracking: Level ID is null or empty");
            return;
        }
        
        if (!levelStartTimes.ContainsKey(levelId))
        {
            Debug.LogWarning($"Cannot stop time tracking: Level {levelId} is not being tracked");
            return;
        }
        // If player is running back and forth between levels, start new tracking
        if(GetLevelElapsedTime(levelId) < 5f)
        {
            Debug.Log($"Won't send tracking. Too short session. Level: {levelId}");
            return;
        }
        
        // Calculate elapsed time
        TimeSpan elapsed = DateTime.Now - levelStartTimes[levelId];
        float elapsedSeconds = (float)elapsed.TotalSeconds;

        Tracker.obj.TrackEvent("level_duration", new Dictionary<string, object>{
            {"level", levelId},
            {"duration", elapsedSeconds}
        });
        
        // Remove from active tracking
        levelStartTimes.Remove(levelId);
        
        Debug.Log($"Stopped time tracking for level: {levelId}. Session time: {elapsedSeconds:F2} seconds");
    }
    
    // Get the total elapsed time for a level (in seconds)
    public float GetLevelElapsedTime(string levelId)
    {
        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning("Cannot get elapsed time: Level ID is null or empty");
            return 0f;
        }
        
        // If the level is currently being tracked, calculate current elapsed time
        if (levelStartTimes.ContainsKey(levelId))
        {
            TimeSpan currentSession = DateTime.Now - levelStartTimes[levelId];
            float currentSessionSeconds = (float)currentSession.TotalSeconds;
            
            return currentSessionSeconds;
        }
        
        return 0f;
    }
}
