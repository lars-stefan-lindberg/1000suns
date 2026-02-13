using UnityEngine;

public class DialogueAudio : MonoBehaviour
{
    [SerializeField] private DialogueSoundSet sounds;

    public void PlayOpen() {
        SoundFXManager.obj.Play2D(sounds.open);
    }

    public void PlayClose() {
        SoundFXManager.obj.Play2D(sounds.close);
    }

    public void PlayConfirm() {
        SoundFXManager.obj.Play2D(sounds.confirm);
    }
}
