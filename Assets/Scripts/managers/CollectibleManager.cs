using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager obj;

    private HashSet<string> pickedColllectibles;
    private string temporaryPickedUpCollectibleId;

    void Awake() {
        obj = this;
        pickedColllectibles = new HashSet<string>();
    }

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

    void OnDestroy() {
        obj = null;
    }
}
