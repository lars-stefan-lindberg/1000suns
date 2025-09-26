using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager obj;
    public static int NUMBER_OF_PRISONER_COLLECTIBLES = 8;

    private List<CaveCollectibleCreature> _isFollowingPlayer = new();

    void Awake() {
        obj = this;
    }

    private HashSet<string> pickedCollectibles = new();

    public void MaybeLoadCollectible(string sceneId) {
        CaveCollectibleCreature[] collectibles = GetComponentsInChildren<CaveCollectibleCreature>(true);
        foreach(CaveCollectibleCreature collectible in collectibles) {
            if(collectible.GetId() == sceneId) {
                if(collectible.IsDespawned) {
                    collectible.gameObject.SetActive(false);
                }
                else if(collectible.IsPicked && !_isFollowingPlayer.Contains(collectible)) {
                    //Handled elsewhere
                } 
                else if(!collectible.IsPicked){
                    collectible.gameObject.SetActive(true);
                }
            }
        }
    }

    public bool SaveFollowingCollectibles() {
        CaveCollectibleCreature[] collectibles = GetComponentsInChildren<CaveCollectibleCreature>(true);
        bool updated = false;
        foreach(CaveCollectibleCreature collectible in collectibles) {
            if(collectible.gameObject.activeSelf && collectible.IsPicked && !collectible.IsPermanentlyCollected && !_isFollowingPlayer.Contains(collectible)) {
                CollectiblePickedPermanently(collectible);
                updated = true;
            }
        }
        return updated;
    }
    
    public void CollectiblePickedPermanently(CaveCollectibleCreature collectible) {
        if(!IsCollectiblePicked(collectible.GetId()))
            pickedCollectibles.Add(collectible.GetId());
        AddFollowingCaveCollectible(collectible);
    }

    public bool IsCollectiblePicked(string id) {
        return pickedCollectibles.Contains(id);
    }

    public int GetNumberOfCollectiblesPicked() {
        return pickedCollectibles.Count;
    }

    public void ResetCollectibles() {
        pickedCollectibles = new();
    }

    // Persistence helpers
    // Export current picked collectible IDs as a list so SaveData (JsonUtility) can serialize it
    public List<string> ExportPickedCollectibles()
    {
        return new List<string>(pickedCollectibles);
    }

    // Import picked collectible IDs from a list (e.g., from SaveData)
    public void ImportPickedCollectibles(List<string> picked)
    {
        pickedCollectibles.Clear();
        if (picked == null) return;
        CaveCollectibleCreature[] collectibles = GetComponentsInChildren<CaveCollectibleCreature>(true);
        foreach (var id in picked)
        {
            if (!string.IsNullOrEmpty(id))
            {
                pickedCollectibles.Add(id);
                foreach(CaveCollectibleCreature collectible in collectibles) {
                    if(collectible.GetId().Equals(id)) {
                        collectible.IsPicked = true;
                    }
                }
            }
        }
    }

    public List<string> ExportFollowingCollectibles() {
        List<string> ids = new();
        foreach(CaveCollectibleCreature collectible in _isFollowingPlayer) {
            ids.Add(collectible.GetId());
        }
        return ids;
    }

    public void ImportFollowingCollectibles(List<string> ids) {
        _isFollowingPlayer = new();
        CaveCollectibleCreature[] collectibles = GetComponentsInChildren<CaveCollectibleCreature>(true);
        foreach(string id in ids) {
            foreach(CaveCollectibleCreature collectible in collectibles) {
                if(collectible.GetId().Equals(id)) {
                    Debug.Log("Enabling collectible to follow player:   " + id);
                    collectible.gameObject.SetActive(true);
                    AddFollowingCaveCollectible(collectible);
                    collectible.RestoreFollowingState();
                }
            }
        }
    }

    public void AddFollowingCaveCollectible(CaveCollectibleCreature caveCollectible) {
        if(!_isFollowingPlayer.Contains(caveCollectible))
            _isFollowingPlayer.Add(caveCollectible);
    }

    public void RemoveFollowingCaveCollectible(CaveCollectibleCreature caveCollectible) {
        _isFollowingPlayer.Remove(caveCollectible);
    }

    public void ClearNonFollowingCollectibles() {
        CaveCollectibleCreature[] collectibles = GetComponentsInChildren<CaveCollectibleCreature>(true);
        foreach(CaveCollectibleCreature collectible in collectibles) {
            if(collectible.gameObject.activeSelf && collectible.IsPicked && !_isFollowingPlayer.Contains(collectible)) {
                collectible.IsPicked = false;
                collectible.Reset();
            }
        }
    }

    public void ClearFollowingCaveCollectibles() { 
        _isFollowingPlayer = new();
    }

    public CaveCollectibleCreature GetCaveCollectibleToFollow() {
        if(_isFollowingPlayer.Count == 0)
            return null;
        return _isFollowingPlayer[_isFollowingPlayer.Count - 1];
    }

    
    public int GetNumberOfCreaturesFollowingPlayer() {
        return _isFollowingPlayer.Count;
    }

    public List<CaveCollectibleCreature> GetFollowingCaveCollectibleCreatures() {
        return _isFollowingPlayer;
    }

    void OnDestroy() {
        obj = this;
    }
}

