using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager obj;
    public static int NUMBER_OF_COLLECTIBLES = 8;

    void Awake() {
        obj = this;
    }

    private HashSet<string> pickedCollectibles = new();
    private string _temporaryPickedUpCollectibleId;

    public void ResetTemporaryPickedCollectible() {
        _temporaryPickedUpCollectibleId = null;
    }

    public void CollectiblePickedTemporarily(String id) {
        _temporaryPickedUpCollectibleId = id;
    }

    public void CollectiblePickedPermanent() {
        if(_temporaryPickedUpCollectibleId != null) {
            pickedCollectibles.Add(_temporaryPickedUpCollectibleId);
            _temporaryPickedUpCollectibleId = null;
        }
    }

    public bool IsCollectiblePicked(string id) {
        return pickedCollectibles.Contains(id);
    }

    public int GetNumberOfCollectiblesPicked() {
        return pickedCollectibles.Count;
    }

    public void ResetCollectibles() {
        ResetTemporaryPickedCollectible();
        pickedCollectibles = new();
    }

    void OnDestroy() {
        obj = this;
    }
}
