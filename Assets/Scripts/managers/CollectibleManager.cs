using System;
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


    public void CollectiblePickedPermanently(String id) {
        if(!IsCollectiblePicked(id))
            pickedCollectibles.Add(id);
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

    void OnDestroy() {
        obj = this;
    }
}
