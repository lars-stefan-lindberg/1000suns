using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SaveManager : MonoBehaviour
{
    public static SaveManager obj;

    // Store the last loaded save and one-time flags to restore state after scene load
    public SaveData LastLoadedSaveData { get; private set; }
    public bool RestoreAudioOnNextScene { get; private set; }
    public bool RestoreBlobOnNextScene { get; private set; }

    private int _activeSaveProfile = 0;

    void Awake() {
        obj = this;
    }
    void OnDestroy() {
        obj = null;
    }

    public void SetActiveSaveProfile(int saveProfileId) {
        _activeSaveProfile = saveProfileId;
    }

    public int GetActiveSaveProfile() {
        return _activeSaveProfile;
    }

    public async void SaveGame(string levelId)
    {
        if(_activeSaveProfile == 0) {
            Debug.LogWarning("No save profile set...");
            return;
        }
        string path = GetSavePath(_activeSaveProfile);
        SaveData data = BuildSaveData(levelId);
        string json = JsonUtility.ToJson(data, true);
        
        // Write to file on background thread to avoid frame hitches
        await Task.Run(() => 
        {
            File.WriteAllText(path, json);
        });
        
        Debug.Log($"Game saved to slot {_activeSaveProfile}: {path}");
    }

    public async Task<SaveData> LoadGame(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            // Read from file on background thread to avoid frame hitches
            string json = await Task.Run(() => File.ReadAllText(path));
            var data = JsonUtility.FromJson<SaveData>(json);
            LastLoadedSaveData = data;
            RestoreAudioOnNextScene = true; // consumed by LevelManager after next scene load
            PlayerStatsManager.obj.numberOfDeaths = data.playerDeaths;
            PlayerStatsManager.obj.SetElapsedTime(data.timePlayed);
            Player.obj.SetAnimatorLayerAndHasCape(data.hasCape);
            ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(data.hasCrown);

            if(GameManager.obj != null) {
                GameManager.obj.SetCurrentSpawnPointId(data.spawnPointId);
            }

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
    public async Task<bool> HasValidSave(int slot)
    {
        try
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path))
                return false;

            // Read from file on background thread
            string json = await Task.Run(() => File.ReadAllText(path));
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

    public void DeleteSave(int slot)
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
        // Cache manager references to reduce property access overhead
        var gameMgr = GameManager.obj;
        var levelMgr = LevelManager.obj;
        var playerPowersMgr = PlayerPowersManager.obj;
        var collectibleMgr = CollectibleManager.obj;
        var musicMgr = MusicManager.obj;
        var ambienceMgr = AmbienceManager.obj;
        var playerStatsMgr = PlayerStatsManager.obj;
        var player = Player.obj;
        var shadowPlayer = ShadowTwinPlayer.obj;
        
        SaveData data = new SaveData();
        data.levelId = levelId;
        data.spawnPointId = gameMgr.GetCurrentSpawnPointId();
        data.playerDeaths = playerStatsMgr.numberOfDeaths;
        data.timePlayed = playerStatsMgr.GetElapsedTime();
        data.hasCape = player.GetHasCape();
        data.hasCrown = shadowPlayer.GetHasCrown();
        data.playerPowers = playerPowersMgr != null ? playerPowersMgr.GetUnlockedPowers() : new List<string>();
        
        // Replace LINQ ToList() with direct list access (already returns IReadOnlyList)
        if (gameMgr != null) {
            var completedEventIds = gameMgr.GetProgressForSave().GetCompletedEventIds();
            data.completedEvents = new List<string>(completedEventIds);
        } else {
            data.completedEvents = new List<string>();
        }
        
        data.completedLevels = levelMgr != null ? levelMgr.ExportCompletedLevels() : new List<string>();
        data.background = levelMgr != null ? levelMgr.GetActiveSceneInitRoomData().backgroundScene : "";
        data.surface = levelMgr != null ? levelMgr.GetActiveSceneInitRoomData().walkableSurfaceScene : "";
        data.caveTimeline = gameMgr != null ? gameMgr.GetCaveTimeline().GetCaveTimelineId() : 0;
        data.pickedCollectibles = collectibleMgr != null ? collectibleMgr.ExportPickedCollectibles() : new List<string>();
        data.lastSaved = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Capture audio state
        if (musicMgr != null)
            if(musicMgr.CurrentTrack != null)
                data.currentMusicId = musicMgr.CurrentTrack.trackId;
            else
                data.currentMusicId = "";
        
        // Replace LINQ Select().ToList() with foreach loop
        if (ambienceMgr != null)
        {
            data.currentAmbienceIds = new List<string>();
            foreach (var track in ambienceMgr.ActiveInstances.Keys)
            {
                data.currentAmbienceIds.Add(track.ambienceId);
            }
        }

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
}
