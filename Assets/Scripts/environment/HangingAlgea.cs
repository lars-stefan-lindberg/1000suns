using UnityEngine;

public class HangingAlgea : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private float _minInitialStartDelay = 0f;
    [SerializeField] private float _maxInitialStartDelay = 1.5f;
    private float _initialDelay;

    [SerializeField] private float _minDelayBetweenPlays = 0.25f;
    [SerializeField] private float _maxDelayBetweenPlays = 1.25f;

    [SerializeField] private float _minPlaySpeed = 0.9f;
    [SerializeField] private float _maxPlaySpeed = 1.3f;

    private string _stateName = "light_pulse"; // Optional: if empty, uses current default state

    private int _stateHash;
    private bool _isPlaying;
    private bool _waitingForStateToFinish;
    private bool _nextPlayScheduled;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        float min = Mathf.Min(_minInitialStartDelay, _maxInitialStartDelay);
        float max = Mathf.Max(_minInitialStartDelay, _maxInitialStartDelay);
        _initialDelay = Random.Range(min, max);
        if (_animator != null && _initialDelay > 0f)
            _animator.enabled = false;
    }

    void Start()
    {
        if (_animator == null) return;
        if (_initialDelay > 0f)
            StartCoroutine(EnableAfterDelay(_initialDelay));
        else
            BeginPlayCycle();
    }

    System.Collections.IEnumerator EnableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_animator != null)
        {
            _animator.enabled = true;
            BeginPlayCycle();
        }
    }

    void Update()
    {
        if (_animator == null || !_isPlaying) return;
        var info = _animator.GetCurrentAnimatorStateInfo(0);

        // We only care about the target state we play explicitly
        bool isInTargetState = info.shortNameHash == _stateHash;
        bool finishedByTime = isInTargetState && info.normalizedTime >= 1f;
        bool leftTargetState = _waitingForStateToFinish && !isInTargetState && !_animator.IsInTransition(0);

        if ((finishedByTime || leftTargetState) && !_nextPlayScheduled)
        {
            _isPlaying = false;
            _waitingForStateToFinish = false;
            _nextPlayScheduled = true;
            float delayMin = Mathf.Min(_minDelayBetweenPlays, _maxDelayBetweenPlays);
            float delayMax = Mathf.Max(_minDelayBetweenPlays, _maxDelayBetweenPlays);
            float nextDelay = Random.Range(delayMin, delayMax);
            StartCoroutine(PlayAfterDelay(nextDelay));
        }
    }

    void BeginPlayCycle()
    {
        // Determine state hash once. If _stateName is set, use it; else infer from current state.
        if (_stateHash == 0)
        {
            if (!string.IsNullOrEmpty(_stateName))
            {
                _stateHash = Animator.StringToHash(_stateName);
            }
            else
            {
                var info = _animator.GetCurrentAnimatorStateInfo(0);
                _stateHash = info.shortNameHash;
            }
        }

        // Start the first play immediately with random speed
        float sMin = Mathf.Min(_minPlaySpeed, _maxPlaySpeed);
        float sMax = Mathf.Max(_minPlaySpeed, _maxPlaySpeed);
        _animator.speed = Random.Range(sMin, sMax);
        _animator.Play(_stateHash, 0, 0f);
        _isPlaying = true;
        _waitingForStateToFinish = true;
        _nextPlayScheduled = false;
    }

    System.Collections.IEnumerator PlayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_animator == null) yield break;
        float sMin = Mathf.Min(_minPlaySpeed, _maxPlaySpeed);
        float sMax = Mathf.Max(_minPlaySpeed, _maxPlaySpeed);
        _animator.speed = Random.Range(sMin, sMax);
        _animator.Play(_stateHash, 0, 0f);
        _isPlaying = true;
        _waitingForStateToFinish = true;
        _nextPlayScheduled = false;
    }
}
