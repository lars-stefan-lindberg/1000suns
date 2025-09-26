using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public string levelId;
    public int playerDeaths;
    public float timePlayed; // in seconds
    public bool hasCape;

    public List<string> playerPowers = new();
    public List<string> completedEvents = new();
    // Levels that have been completed. We store as a list since JsonUtility can't serialize Dictionary.
    public List<string> completedLevels = new();
    // Collectible IDs that have been permanently picked by the player
    public List<string> pickedCollectibles = new();
    public List<string> followingCollectibles = new();

    public string lastSaved; // store DateTime as string for JSON

    // Audio state at save time
    // If null or empty, no music/ambience was playing
    public string currentMusicId;
    public string currentAmbienceId;

    // Global lighting darkness color in HTML RGBA (e.g. "#RRGGBBAA").
    // Kept as string so SaveData has no UnityEngine dependency.
    public string darknessColorHex;
}