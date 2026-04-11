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
    private EliAudio _eliAudio;

    void Awake()
    {
        _sharedPlayerAudio = transform.parent.gameObject.GetComponent<SharedCharacterAudio>();
        _eliAudio = transform.parent.gameObject.GetComponent<EliAudio>();
    }

    public void PlayFootstep() {
        _sharedPlayerAudio.PlayFootstep(PlayerMovement.obj.surface);
    }

    public void PlayHeadOutOfTent() {
        _eliAudio.PlayHeadMovingOutOfTent();
    }

    public void PlayGetOutOfTent() {
        _eliAudio.PlayGetOutOfTent();
    }

    public void PlayYawn() {
        _eliAudio.PlayYawn();
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
        PlayHeadOutOfTent();
    }

    public void OnGetOutOfTentFinished() {
        PlayerEvents.TriggerTentExitComplete();
    }

    public void ForestTouchGlyph() {
        PlayerEvents.TriggerForestGlyphTouched();
    }
}
