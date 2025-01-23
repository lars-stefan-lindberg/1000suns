using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebindUIElement : MonoBehaviour
{
    public void OnRebindCancel() {
        SoundFXManager.obj.PlayUIBack();
    }

    public void OnRebindConfirm() {
        SoundFXManager.obj.PlayUIConfirm();
    }
}
