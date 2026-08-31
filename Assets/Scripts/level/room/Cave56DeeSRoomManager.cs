using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave56DeeSRoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _loadLightBeam;

    void Start()
    {
        _loadLightBeam.SetActive(true);    
    }
}
