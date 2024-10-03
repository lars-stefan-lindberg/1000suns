using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public void PlayRunStep() {
        SoundFXManager.obj.PlayStep(Player.obj.surface, gameObject.transform);
    }
}
