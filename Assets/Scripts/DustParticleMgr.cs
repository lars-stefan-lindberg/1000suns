using UnityEngine;

public class DustParticleMgr : MonoBehaviour
{
    public static DustParticleMgr obj;

    public ParticleSystem dust;
    
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
        if(PlayerBlobMovement.obj != null &&PlayerBlobMovement.obj.gameObject.activeSelf)
            dust.transform.position = PlayerBlobMovement.obj.anchor.transform.position;
        else
            dust.transform.position = PlayerMovement.obj.anchor.transform.position;
            
        dust.Play();
    }
}
