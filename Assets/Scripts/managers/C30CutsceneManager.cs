using System.Collections;
using UnityEngine;

public class C30CutsceneManager : MonoBehaviour
{
    [SerializeField] private Transform _caveAvatarFlyOffTarget;
    [SerializeField] private CameraManager _cameraManager;
    [SerializeField] private DialogueController _dialogueController;
    [SerializeField] private DialogueContent _dialogueContent;

    void Start() {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed += OnDialogueCompleted;
            _dialogueController.OnDialogueClosing += OnDialogueClosing;
        }
    }

    void OnDestroy()
    {
        if (_dialogueController != null)
        {
            _dialogueController.OnDialogueClosed -= OnDialogueCompleted;
            _dialogueController.OnDialogueClosing -= OnDialogueClosing;
        }
    }

    private void OnDialogueClosing() {
        SoundFXManager.obj.PlayDialogueClose();
    }

    private void OnDialogueCompleted() {
        PlayerMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;
        GameEventManager.obj.C30CutsceneCompleted = true;
        MusicManager.obj.Resume();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(GameEventManager.obj.C30CutsceneCompleted) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        MusicManager.obj.Pause();
        if(Player.obj.gameObject.activeSelf) {
            PlayerMovement.obj.Freeze();
        } else if(PlayerBlob.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze();
            PlayerBlobMovement.obj.ToHuman();
        }
        GameEventManager.obj.IsPauseAllowed = false;
        
        yield return new WaitForSeconds(1f);

        //Change to alternative camera
        _cameraManager.ActivateAlternativeCamera();

        yield return new WaitForSeconds(5f);

        CaveAvatar.obj.SetTarget(_caveAvatarFlyOffTarget.transform);

        yield return new WaitForSeconds(2f);

        _cameraManager.ActivateMainCamera();

        yield return new WaitForSeconds(3f);

        SoundFXManager.obj.PlayDialogueOpen();
        _dialogueController.ShowDialogue(_dialogueContent);

        yield return null;
    }
}
