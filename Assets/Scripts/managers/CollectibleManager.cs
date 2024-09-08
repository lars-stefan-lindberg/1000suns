using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    private static readonly Lazy<CollectibleManager> lazyInstance =
        new Lazy<CollectibleManager>(() => new CollectibleManager());

    private CollectibleManager() {}

    public static CollectibleManager Instance
    {
        get
        {
            return lazyInstance.Value;
        }
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
}
