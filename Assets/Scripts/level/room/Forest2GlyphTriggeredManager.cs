using System.Collections;
using UnityEngine;

public class Forest2GlyphTriggeredManager : MonoBehaviour
{
    [SerializeField] private GlyphStone _glyphStone;
    [SerializeField] private GameObject _rainSystems;
    [SerializeField] private ThunderLight _thunderLight;

    void Start() {
        PlayerEvents.OnForestGlyphTouched += Activate;
    }

    public void InitiateEliTouchAnimation() {
        PlayerMovement.obj.Freeze();
        StartCoroutine(StartEliTouchAnimation());
    }

    public void Activate() {
        _glyphStone.Activate();
        _rainSystems.SetActive(true);

        _thunderLight.Flash();
        
        //Play rain ambience
        //Play music
    }

    private IEnumerator StartEliTouchAnimation() {
        yield return new WaitForSeconds(1f);

        Player.obj.PlayForestGlyphTouch(); //The animation contains an event trigger for PlayerEvents.OnForestGlyphTouched
    }

    void OnDestroy() {
        PlayerEvents.OnForestGlyphTouched -= Activate;
    }
}
