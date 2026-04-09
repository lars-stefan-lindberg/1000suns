using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class TentCutsceneManager : MonoBehaviour
{
    [SerializeField] private ForestTent _forestTent;
    [SerializeField] private float _timeUntilCloseTent = 1.5f;
    [SerializeField] private float _waitTimeUntilTentAnimation = 1f;
    [SerializeField] private Forest1CameraHandler _cameraHandler;
    [SerializeField] private GameEventId _tentCutSceneCompleted;
    [SerializeField] private EventReference _sleepingBagSfx;

    void Start() {
        PlayerEvents.OnTentExitComplete += OnEliAnimationCompleted;
    }

    public void StartTentSequence() {
        StartCoroutine(StartTentSequenceCoroutine());
    }

    private IEnumerator StartTentSequenceCoroutine() {
        SoundFXManager.obj.PlayAtPosition(_sleepingBagSfx, _forestTent.transform.position);
        yield return new WaitForSeconds(_waitTimeUntilTentAnimation);
        _forestTent.Rumble();
    }

    public void PlayEliGettingOutOfTent() {
        Player.obj.GetOutOfTent();
        StartCoroutine(DelayedCloseTent());
    }

    private IEnumerator DelayedCloseTent() {
        yield return new WaitForSeconds(_timeUntilCloseTent);
        _forestTent.Close();
        yield return new WaitForSeconds(2f);
        _forestTent.StopAnimator();
    }

    public void OnEliAnimationCompleted() {        
        GameManager.obj.RegisterEvent(_tentCutSceneCompleted);
        _cameraHandler.HandleCamera();
        StartCoroutine(DelayedEnablePlayer());
    }

    private IEnumerator DelayedEnablePlayer() {
        yield return new WaitForSeconds(2.5f);

        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        PlayerStatsManager.obj.ResumeTimer();
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        PlayerEvents.OnTentExitComplete -= OnEliAnimationCompleted;
    }

    void OnDestroy() {
        PlayerEvents.OnTentExitComplete -= OnEliAnimationCompleted;
    }
}
