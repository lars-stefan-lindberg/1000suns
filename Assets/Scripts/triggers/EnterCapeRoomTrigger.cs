using System.Collections;
using UnityEngine;

public class EnterCapeRoomTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _beamOfLight;
    [SerializeField] private GameObject _cape;
    [SerializeField] private Transform _capeMoveTarget;
    private SpriteRenderer _beamOfLightRenderer;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;
    private Color _fadeStartColor;
    private bool _isTriggered = false;

    void Awake() {
        _beamOfLightRenderer = _beamOfLight.GetComponent<SpriteRenderer>();
        _fadeStartColor = new Color(_beamOfLightRenderer.color.r, _beamOfLightRenderer.color.g, _beamOfLightRenderer.color.b, 0);
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if(collider.transform.CompareTag("Player") && !_isTriggered) {
            PlayerMovement.obj.Freeze();
            StartCoroutine(EnterCapeRoomSequence());
            _isTriggered = true;
        }
    }

    private IEnumerator EnterCapeRoomSequence() {
        //Zoom in on cape

        //Fade in beam of light
        while(_beamOfLightRenderer.color.a < 0.65f) {
            _fadeStartColor.a += Time.deltaTime * _fadeSpeed;
            _beamOfLightRenderer.color = _fadeStartColor;
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        //Lower cape
        while(_cape.transform.position != _capeMoveTarget.position) {
            _cape.transform.position = Vector2.MoveTowards(_cape.transform.position, _capeMoveTarget.position, 1.1f * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        _cape.GetComponent<Cape>().StartAnimation();

        yield return new WaitForSeconds(3);

        //Fade out beam
        while(_beamOfLightRenderer.color.a > 0f) {
            _fadeStartColor.a -= Time.deltaTime * _fadeSpeed;
            _beamOfLightRenderer.color = _fadeStartColor;
            yield return null;
        }

        yield return new WaitForSeconds(1);

        PlayerMovement.obj.UnFreeze();
    }
}
