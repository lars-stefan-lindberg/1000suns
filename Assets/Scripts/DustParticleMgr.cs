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

    public void CreateDust(PlayerManager.PlayerType playerType)
    {
        if(!Enabled)
            return;
        if(playerType == PlayerManager.PlayerType.BLOB)
            dust.transform.position = PlayerBlobMovement.obj.anchor.transform.position;
        else if(playerType == PlayerManager.PlayerType.SHADOW_TWIN)
            dust.transform.position = ShadowTwinMovement.obj.anchor.transform.position;
        else 
            dust.transform.position = PlayerMovement.obj.anchor.transform.position;
            
        dust.Play();
    }
}
