using System.Collections;
using UnityEngine;

public class C31Manager : MonoBehaviour
{
    [SerializeField] private SpikesManager _spikesManager;
    [SerializeField] private GameEventId _cave52CutsceneCompleted;
    public float _attackInterval = 2f;
    private bool _startAttackSequence = false;
    private float _timer = 1.5f;

    void Start()
    {
        if(LevelManager.obj.IsLevelCompleted("Cave-52"))
            return;
        if(GameManager.obj.HasEvent(_cave52CutsceneCompleted)) {
            _startAttackSequence = true;
        }
    }

    public void Stop()
    {
        _startAttackSequence = false;
    }

    [ContextMenu("Start Attack Sequence")]
    public void StartAttackSequence() {
        _startAttackSequence = true;
    }

    void FixedUpdate()
    {
        if (_startAttackSequence) {
            _timer += Time.fixedDeltaTime;
            if (_timer > _attackInterval) {
                _timer = 0;
                StartCoroutine(FireAttack());
            }
        }
    }

    private IEnumerator FireAttack() {
        CaveAvatar.obj.Attack();
        yield return new WaitForSeconds(0.5f);
        _spikesManager.ReleaseSpikes();
    }
}
