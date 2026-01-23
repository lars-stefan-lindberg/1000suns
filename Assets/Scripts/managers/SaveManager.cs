using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public static SaveManager obj;

    // Store the last loaded save and one-time flags to restore state after scene load
    public SaveData LastLoadedSaveData { get; private set; }
    public bool RestoreAudioOnNextScene { get; private set; }
    public bool RestoreBlobOnNextScene { get; private set; }
    public bool RestoreFollowingCreaturesOnNextScene { get; private set; }

    void Awake() {
        obj = this;
    }
    void OnDestroy() {
        obj = null;
    }

    public void SaveGame(string levelId, int slot = 1)
    {
        string path = GetSavePath(slot);
        SaveData data = BuildSaveData(levelId);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Game saved to slot {slot}: {path}");
    }

    public SaveData LoadGame(int slot = 1)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SaveData>(json);
            LastLoadedSaveData = data;
            RestoreAudioOnNextScene = true; // consumed by LevelManager after next scene load
            PlayerStatsManager.obj.numberOfDeaths = data.playerDeaths;
            PlayerStatsManager.obj.SetElapsedTime(data.timePlayed);
            Player.obj.SetAnimatorLayerAndHasCape(data.hasCape);
            ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(data.hasCrown);

            // Apply saved game events back into the manager
            if (GameManager.obj != null)
            {
                GameProgress gameProgress = new GameProgress();
                gameProgress.ImportCompletedEvents(data?.completedEvents);
                GameManager.obj.LoadProgress(gameProgress);
            }

            if(GameManager.obj != null) {
                GameManager.obj.SetCaveTimeline(new CaveTimeline(data.caveTimeline));
            }

            // Apply saved player powers back into the manager
            if (PlayerPowersManager.obj != null)
            {
                PlayerPowersManager.obj.ApplyUnlockedPowers(data?.playerPowers);
            }
            
            //Check if player turned to blob, but haven't acquired shapeshifting power yet
            if (PlayerPowersManager.obj.EliCanTurnFromHumanToBlob && !PlayerPowersManager.obj.EliCanTurnFromBlobToHuman)
            {
                RestoreBlobOnNextScene = true;
            }

            // Apply completed levels back into the level manager
            if (LevelManager.obj != null)
            {
                LevelManager.obj.ImportCompletedLevels(data?.completedLevels);
            }

            // Apply picked collectibles back into the collectible manager
            if (CollectibleManager.obj != null)
            {
                CollectibleManager.obj.ImportPickedCollectibles(data?.pickedCollectibles);
                if(data.followingCollectibles != null && data.followingCollectibles.Count > 0) {
                    RestoreFollowingCreaturesOnNextScene = true;
                }
            }

            return data;
        }
        else
        {
            Debug.LogWarning($"No save file found in slot {slot}.");
            return null; // return null to signal "empty slot"
        }
    }

    // Quick check to see if a save file exists and appears valid for loading
    public bool HasValidSave(int slot = 1)
    {
        try
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path))
                return false;

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) {
                DeleteSave(slot);
                return false;
            }

            // Try to parse JSON into SaveData
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) {
                DeleteSave(slot);
                return false;
            }

            // Minimal validation: a level id must be present and non-empty.
            if (string.IsNullOrEmpty(data.levelId)) {
                DeleteSave(slot);
                return false;
            }

            // Optional sanity checks
            if (data.playerDeaths < 0) {
                DeleteSave(slot);
                return false;
            }
            if (data.timePlayed < 0f) {
                DeleteSave(slot);
                return false;
            }

            // If we got here, it's likely loadable by our current loader
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Save validation failed: {ex.Message}");
            DeleteSave(slot);
            return false;
        }
    }

    public void DeleteSave(int slot = 1)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save slot {slot}");
        }
    }

    private static string GetSavePath(int slot)
    {
        return Application.persistentDataPath + $"/save_{slot}.json";
    }

    private SaveData BuildSaveData(string levelId) {
        SaveData data = new SaveData();
        data.levelId = levelId;
        data.playerDeaths = PlayerStatsManager.obj.numberOfDeaths;
        data.timePlayed = PlayerStatsManager.obj.GetElapsedTime();
        data.hasCape = Player.obj.GetHasCape();
        data.hasCrown = ShadowTwinPlayer.obj.GetHasCrown();
        data.playerPowers = PlayerPowersManager.obj != null ? PlayerPowersManager.obj.GetUnlockedPowers() : new List<string>();
        data.completedEvents = GameManager.obj != null ? GameManager.obj.GetProgressForSave().GetCompletedEventIds().ToList() : new List<string>();
        data.completedLevels = LevelManager.obj != null ? LevelManager.obj.ExportCompletedLevels() : new List<string>();
        data.background = LevelManager.obj != null ? LevelManager.obj.GetActiveSceneInitRoomData().backgroundScene : "";
        data.surface = LevelManager.obj != null ? LevelManager.obj.GetActiveSceneInitRoomData().walkableSurfaceScene : "";
        data.caveTimeline = GameManager.obj != null ? GameManager.obj.GetCaveTimeline().GetCaveTimelineId() : 0;
        data.pickedCollectibles = CollectibleManager.obj != null ? CollectibleManager.obj.ExportPickedCollectibles() : new List<string>();
        data.followingCollectibles = CollectibleManager.obj != null ? CollectibleManager.obj.ExportFollowingCollectibles() : new List<string>();
        data.lastSaved = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Capture audio state
        if (MusicManager.obj != null)
            data.currentMusicId = MusicManager.obj.GetCurrentMusicId();
        if (AmbienceManager.obj != null)
            data.currentAmbienceId = AmbienceManager.obj.GetCurrentAmbienceId();

        return data;
    }

    // Called by LevelManager after it restores audio so we don't re-apply on subsequent scene loads
    public void ConsumeRestoreAudioFlag() {
        RestoreAudioOnNextScene = false;
    }

    // Called by LevelManager after it restores blob
    public void ConsumeRestoreBlobFlag() {
        RestoreBlobOnNextScene = false;
    }

    public void ConsumeRestoreFollowingCreaturesFlag() {
        RestoreFollowingCreaturesOnNextScene = false;
    }
}