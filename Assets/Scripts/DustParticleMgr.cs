using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustParticleMgr : MonoBehaviour
{
    public static DustParticleMgr obj;

    public ParticleSystem dust;
    public GameObject playerAnchor;

    
    void Awake()
    {
        obj = this;
    }

    // Update is called once per frame
    void OnDestroy()
    {
        obj = null;
    }

    public void CreateDust()
    {
        dust.transform.position = playerAnchor.transform.position;
        dust.Play();
    }
}
