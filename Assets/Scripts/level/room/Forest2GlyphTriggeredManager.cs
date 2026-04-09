using System.Collections;
using UnityEngine;

public class Forest2GlyphTriggeredManager : MonoBehaviour
{
    [SerializeField] private GlyphStone _glyphStone;
    [SerializeField] private GameObject _rainSystems;
    [SerializeField] private ThunderLight _thunderLight;
    [SerializeField] private AmbienceTrack _rain;

    void Start() {
        PlayerEvents.OnForestGlyphTouched += Activate;
    }

    public void InitiateEliTouchAnimation() {
        PlayerMovement.obj.Freeze();
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        StartCoroutine(StartCutscene());
    }

    public void Activate() {
        _glyphStone.Activate();
        _rainSystems.SetActive(true);

        _thunderLight.Flash();
        
        AmbienceManager.obj.Play(_rain);
        //Play music
    }

    private IEnumerator StartCutscene() {
        yield return new WaitForSeconds(2f);

        PlayerMovement.obj.StartWalking();
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));

        yield return new WaitForSeconds(3f);
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        PlayerMovement.obj.StopWalking();

        Player.obj.PlayForestGlyphTouch(); //The animation contains an event trigger for PlayerEvents.OnForestGlyphTouched
    }

    void OnDestroy() {
        PlayerEvents.OnForestGlyphTouched -= Activate;
    }
}
