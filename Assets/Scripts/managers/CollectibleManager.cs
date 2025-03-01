using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager obj;
    public static int NUMBER_OF_PRISONER_COLLECTIBLES = 8;

    private List<CaveCollectibleCreature> _isFollowingPlayer = new();

    void Awake() {
        obj = this;
    }

    private HashSet<string> pickedCollectibles = new();


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

    public void AddFollowingCaveCollectible(CaveCollectibleCreature caveCollectible) {
        _isFollowingPlayer.Add(caveCollectible);
    }

    public void RemoveFollowingCaveCollectible(CaveCollectibleCreature caveCollectible) {
        _isFollowingPlayer.Remove(caveCollectible);
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
