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
    private string temporaryPickedUpCollectibleId;

    public void CollectiblePickedTemporary(string id) {
        temporaryPickedUpCollectibleId = id;
    }
    
    public void CollectiblePickedPermanent() {
        if(temporaryPickedUpCollectibleId != null) {
            pickedColllectibles.Add(temporaryPickedUpCollectibleId);
            temporaryPickedUpCollectibleId = null;
        }
    }

    public bool IsCollectiblePicked(string id) {
        return pickedColllectibles.Contains(id);
    }
}
