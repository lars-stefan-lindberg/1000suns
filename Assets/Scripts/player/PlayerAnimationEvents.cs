using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public void PlayRunStep() {
        AudioManager.obj.PlayStep(gameObject.transform);
    }
}
