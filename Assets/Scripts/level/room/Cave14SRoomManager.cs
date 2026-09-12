using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave14SRoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _loadLights;

    void Start()
    {
        _loadLights.SetActive(true);    
    }
}
