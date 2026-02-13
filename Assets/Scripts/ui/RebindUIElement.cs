using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebindUIElement : MonoBehaviour
{
    public void OnRebindCancel() {
        UISoundPlayer.obj.PlayBack();
    }

    public void OnRebindConfirm() {
        UISoundPlayer.obj.PlaySelect();
    }
}
