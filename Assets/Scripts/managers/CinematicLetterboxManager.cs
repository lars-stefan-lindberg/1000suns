using UnityEngine;
using UnityEngine.UI;

public class CinematicLetterboxManager : MonoBehaviour
{
    public static CinematicLetterboxManager obj;

    [SerializeField] private RectTransform _topBar;
    [SerializeField] private RectTransform _bottomBar;
    [SerializeField] private float _barHeight = 100f;
    [SerializeField] private float _animationSpeed = 300f;

    private float _targetTopY;
    private float _targetBottomY;
    private bool _isAnimating;
    private bool _isShowing;

    public bool IsAnimating => _isAnimating;
    public bool IsShowing => _isShowing;

    void Awake()
    {
        obj = this;

        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "UI";
    }

    void Start()
    {
        if (_topBar != null)
        {
            _topBar.anchoredPosition = new Vector2(0, _barHeight);
        }
        if (_bottomBar != null)
        {
            _bottomBar.anchoredPosition = new Vector2(0, -_barHeight);
        }
    }

    void Update()
    {
        if (!_isAnimating) return;

        bool topComplete = false;
        bool bottomComplete = false;

        if (_topBar != null)
        {
            Vector2 currentPos = _topBar.anchoredPosition;
            float newY = Mathf.MoveTowards(currentPos.y, _targetTopY, _animationSpeed * Time.deltaTime);
            _topBar.anchoredPosition = new Vector2(0, newY);
            topComplete = Mathf.Approximately(newY, _targetTopY);
        }
        else
        {
            topComplete = true;
        }

        if (_bottomBar != null)
        {
            Vector2 currentPos = _bottomBar.anchoredPosition;
            float newY = Mathf.MoveTowards(currentPos.y, _targetBottomY, _animationSpeed * Time.deltaTime);
            _bottomBar.anchoredPosition = new Vector2(0, newY);
            bottomComplete = Mathf.Approximately(newY, _targetBottomY);
        }
        else
        {
            bottomComplete = true;
        }

        if (topComplete && bottomComplete)
        {
            _isAnimating = false;
        }
    }

    public void ShowLetterbox()
    {
        _targetTopY = 0;
        _targetBottomY = 0;
        _isAnimating = true;
        _isShowing = true;
    }

    public void ShowLetterbox(float customSpeed)
    {
        float previousSpeed = _animationSpeed;
        _animationSpeed = customSpeed;
        ShowLetterbox();
        _animationSpeed = previousSpeed;
    }

    public void HideLetterbox()
    {
        _targetTopY = _barHeight;
        _targetBottomY = -_barHeight;
        _isAnimating = true;
        _isShowing = false;
    }

    public void HideLetterbox(float customSpeed)
    {
        float previousSpeed = _animationSpeed;
        _animationSpeed = customSpeed;
        HideLetterbox();
        _animationSpeed = previousSpeed;
    }

    public void SetLetterboxImmediate(bool show)
    {
        _isAnimating = false;
        _isShowing = show;

        if (show)
        {
            if (_topBar != null) _topBar.anchoredPosition = new Vector2(0, 0);
            if (_bottomBar != null) _bottomBar.anchoredPosition = new Vector2(0, 0);
        }
        else
        {
            if (_topBar != null) _topBar.anchoredPosition = new Vector2(0, _barHeight);
            if (_bottomBar != null) _bottomBar.anchoredPosition = new Vector2(0, -_barHeight);
        }
    }
}
