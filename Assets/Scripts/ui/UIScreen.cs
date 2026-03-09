using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIScreen : MonoBehaviour
{
    public static float FADE_DURATION = 0.3f;
    [SerializeField] protected GameObject defaultSelected;

    protected CanvasGroup canvasGroup;
    private GameObject _backSelectable;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetBackSelectable(GameObject selectable)
    {
        _backSelectable = selectable;
    }

    protected virtual void OnBeforeShow() { }
    protected virtual void SelectDefaultOrBack() {
        GameObject toSelect = _backSelectable != null 
            ? _backSelectable 
            : defaultSelected;

        EventSystem.current.SetSelectedGameObject(toSelect);
        _backSelectable = null;
    }

    public virtual Tween Show()
    {
        gameObject.SetActive(true);
        SelectDefaultOrBack();

        OnBeforeShow();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        return canvasGroup
            .DOFade(1f, FADE_DURATION)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
    }

    public virtual Tween Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        return canvasGroup
            .DOFade(0f, FADE_DURATION)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}
