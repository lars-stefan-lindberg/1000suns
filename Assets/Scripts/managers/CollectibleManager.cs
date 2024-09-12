using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager obj;

    void Awake() {
        obj = this;
    }

    private HashSet<string> pickedColllectibles = new();
    private string _temporaryPickedUpCollectibleId;

    public void ResetTemporaryPickedCollectible() {
        _temporaryPickedUpCollectibleId = null;
    }

    public void CollectiblePickedTemporarily(String id) {
        _temporaryPickedUpCollectibleId = id;
    }

    public void CollectiblePickedPermanent() {
        if(_temporaryPickedUpCollectibleId != null) {
            pickedColllectibles.Add(_temporaryPickedUpCollectibleId);
            _temporaryPickedUpCollectibleId = null;
        }
    }

    public bool IsCollectiblePicked(string id) {
        return pickedColllectibles.Contains(id);
    }

    void OnDestroy() {
        obj = this;
    }
}
