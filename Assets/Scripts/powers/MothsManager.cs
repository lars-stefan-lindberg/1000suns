using System.Collections.Generic;
using UnityEngine;

public class MothsManager : MonoBehaviour
{
    public static MothsManager obj;
    public GameObject mothsPrefab;

    private HashSet<GameObject> _mothsPrefabs = new();
    
    void Awake() {
        obj = this;
    }

    public void SpawnMoths() {
        GameObject moths = Instantiate(mothsPrefab, transform.position, transform.rotation);
        moths.transform.parent = transform;
        moths.GetComponent<Moths>().Activate();
        _mothsPrefabs.Add(moths);
    }

    public void DestroyMoths() {
        foreach (GameObject moths in _mothsPrefabs) {
            Destroy(moths);
        }
        _mothsPrefabs.Clear();
    }

    public void SendActiveToCrystal(Vector3 position) {
        foreach (GameObject mothsPrefab in _mothsPrefabs) {
            if(mothsPrefab == null) {
                _mothsPrefabs.Remove(mothsPrefab);
                continue;
            }
            Moths moths = mothsPrefab.GetComponent<Moths>();
            if(!moths.stopFollowingPlayer) {
                moths.SetTarget(position);
            }
        }
    }

    public void Remove(GameObject moths)
    {
        if (moths == null) {
            return;
        }
        _mothsPrefabs.Remove(moths);
        Destroy(moths);
    }

    void OnDestroy()
    {
        obj = null;
    }
}
