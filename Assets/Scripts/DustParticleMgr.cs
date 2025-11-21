using UnityEngine;

public class DustParticleMgr : MonoBehaviour
{
    public static DustParticleMgr obj;

    public ParticleSystem dust;

    public bool Enabled { get;  set; }
    
    void Awake()
    {
        obj = this;
        Enabled = true;
    }

    // Update is called once per frame
    void OnDestroy()
    {
        obj = null;
    }

    public void CreateDust()
    {
        if(!Enabled)
            return;
        if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            dust.transform.position = PlayerBlobMovement.obj.anchor.transform.position;
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf)
            dust.transform.position = ShadowTwinMovement.obj.anchor.transform.position;
        else 
            dust.transform.position = PlayerMovement.obj.anchor.transform.position;
            
        dust.Play();
    }
}
