using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlobAnimationEvents : MonoBehaviour
{
    private BlobAudio _blobAudio;
    
    void Awake()
    {
        _blobAudio = transform.parent.gameObject.GetComponent<BlobAudio>();
    }

    public void PlayFootstep() {
        if(PlayerBlobMovement.obj.isGrounded)
            _blobAudio.PlayFootstep();
    }
}
