using System.Collections;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private EventReference _rootsPull;
    [SerializeField] private EventReference _breakableWallCracklingSfx;
    private SharedCharacterAudio _sharedPlayerAudio;

    void Awake()
    {
        _sharedPlayerAudio = transform.parent.gameObject.GetComponent<SharedCharacterAudio>();
    }

    public void PlayFootstep() {
        _sharedPlayerAudio.PlayFootstep(Player.obj.surface);
    }

    public void ToBlob() {
        PlayerMovement.obj.ToBlob();
    }

    public void ToShadowTwin() {
        PlayerMovement.obj.ToTwin();
    }

    public void JumpSqueeze() {
        PlayerMovement.obj.JumpSqueeze();
    }

    public void BreakCaveRoots() {
        SoundFXManager.obj.PlayAtPosition(_breakableWallCracklingSfx, Camera.main.transform.position);
        PlayerMovement.obj.NudgePlayer();
        CaveRootsTrap.obj.StartBreaking();
    }

    public void PullRoots() {
        SoundFXManager.obj.PlayAtPosition(_rootsPull, Camera.main.transform.position);
    }

    public void StartGettingOutOfTent() {
        PlayerMovement.obj.spriteRenderer.enabled = true;
    }

    public void OnGetOutOfTentFinished() {
        PlayerEvents.TriggerTentExitComplete();
    }

    public void ForestTouchGlyph() {
        PlayerEvents.TriggerForestGlyphTouched();
    }

    public void UnFreeze() {
        StartCoroutine(DelayedUnFreeze());
    }

    private IEnumerator DelayedUnFreeze() {
        yield return new WaitForSeconds(1f);

        PlayerMovement.obj.UnFreeze();
    }
}
