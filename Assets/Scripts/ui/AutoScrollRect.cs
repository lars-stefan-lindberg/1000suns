using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Add the script to your Dropdown Menu Template Object via (Your Dropdown Button > Template)

[RequireComponent(typeof(ScrollRect))]
public class AutoScrollRect : MonoBehaviour
{
    // Sets the speed to move the scrollbar
    public float scrollSpeed = 10f;

    // Set as Template Object via (Your Dropdown Button > Template)
    public ScrollRect m_templateScrollRect;

    // Set as Template Viewport Object via (Your Dropdown Button > Template > Viewport)
    public RectTransform m_templateViewportTransform;

    // Set as Template Content Object via (Your Dropdown Button > Template > Viewport > Content)
    public RectTransform m_ContentRectTransform;

    private RectTransform m_SelectedRectTransform;

    public void ResetScrollRect() {
        m_templateScrollRect.normalizedPosition = new Vector2(0, 1);
    }

    void Update()
    {
        UpdateScrollToSelected(m_templateScrollRect, m_ContentRectTransform, m_templateViewportTransform);
    }

    void UpdateScrollToSelected(ScrollRect scrollRect, RectTransform contentRectTransform, RectTransform viewportRectTransform)
    {
        // Get the current selected option from the eventsystem.
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            return;
        }

        m_SelectedRectTransform = selected.GetComponent<RectTransform>();

        //Check if the button is a rebinding component, or just a regular button
        float buttonLocalPositionY = 0;
        if(m_SelectedRectTransform.parent.GetComponent<RebindUIElement>() != null) {
            buttonLocalPositionY = contentRectTransform.rect.height - m_SelectedRectTransform.parent.GetComponent<RectTransform>().localPosition.y;
        } else {
            buttonLocalPositionY = contentRectTransform.rect.height - m_SelectedRectTransform.localPosition.y;
        }

        // Math stuff
        float selectedDifferenceY = viewportRectTransform.localPosition.y + buttonLocalPositionY; 
        float contentHeightDifference = (contentRectTransform.rect.height - viewportRectTransform.rect.height);

        float selectedPosition = (contentRectTransform.rect.height - selectedDifferenceY);
        float currentScrollRectPosition = scrollRect.normalizedPosition.y * contentHeightDifference;
        float above = currentScrollRectPosition - m_SelectedRectTransform.rect.height * 3 + viewportRectTransform.rect.height;
        float below = currentScrollRectPosition + m_SelectedRectTransform.rect.height * 3;

        // Check if selected option is out of bounds.
        if (selectedPosition > above)
        {
            float step = selectedPosition - above;
            float newY = currentScrollRectPosition + step;
            float newNormalizedY = newY / contentHeightDifference;
            scrollRect.normalizedPosition = Vector2.Lerp(scrollRect.normalizedPosition, new Vector2(0, newNormalizedY), scrollSpeed * Time.unscaledDeltaTime);
        }
        else if (selectedPosition < below)
        {
            float step = selectedPosition - below;
            float newY = currentScrollRectPosition + step;
            float newNormalizedY = newY / contentHeightDifference;
            scrollRect.normalizedPosition = Vector2.Lerp(scrollRect.normalizedPosition, new Vector2(0, newNormalizedY), scrollSpeed * Time.unscaledDeltaTime);
        }
    }
}