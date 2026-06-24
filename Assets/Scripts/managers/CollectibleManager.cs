using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager obj;
    public static int NUMBER_OF_PRISONER_COLLECTIBLES = 8;

    void Awake() {
        obj = this;
    }

    private HashSet<string> pickedCollectibles = new();

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

    public void ResetCollectibles() {
        pickedCollectibles = new();
    }
    
    public void CollectiblePickedPermanently(CaveCollectibleCreature collectible) {
        if(!IsCollectiblePicked(collectible.GetId()))
            pickedCollectibles.Add(collectible.GetId());
    }

    public bool IsCollectiblePicked(string id) {
        return pickedCollectibles.Contains(id);
    }

    public int GetNumberOfCollectiblesPicked() {
        return pickedCollectibles.Count;
    }

    void OnDestroy() {
        obj = this;
    }
}

