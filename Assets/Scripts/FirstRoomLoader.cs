using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRoomLoader : MonoBehaviour
{
    void Start()
    {
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(1f);
        SceneFadeManager.obj.StartFadeIn();
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            MusicManager.obj.PlayCaveSong();
            SceneFadeManager.obj.SetFadeInSpeed(5f);
            Destroy(this);
        }
    }
}
