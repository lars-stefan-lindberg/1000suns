using FMODUnity;
using UnityEngine;

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
}
