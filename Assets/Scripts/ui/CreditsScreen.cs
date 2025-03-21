using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CreditsScreen : MonoBehaviour
{
    [SerializeField] private GameObject _collectibleCountLabel;
    [SerializeField] private GameObject _deathCountLabel;
    [SerializeField] private GameObject _timeCountLabel;
    [SerializeField] private TextMeshProUGUI _scrollKeyboardText;
    [SerializeField] private Image _scrollGamepadIcon;
    [SerializeField] private TextMeshProUGUI _scrollText;
    public InputActionReference scrollButtonActionReference;
    private Animator _animator;

    void Awake() {
        _animator = GetComponent<Animator>();

        TextMeshProUGUI collectiblesText = _collectibleCountLabel.GetComponent<TextMeshProUGUI>();
        collectiblesText.text = CollectibleManager.obj.GetNumberOfCollectiblesPicked() + " out of " + CollectibleManager.NUMBER_OF_PRISONER_COLLECTIBLES;

        TextMeshProUGUI deathCountText = _deathCountLabel.GetComponent<TextMeshProUGUI>();
        deathCountText.text = PlayerStatsManager.obj.numberOfDeaths.ToString();

        TextMeshProUGUI timeCountText = _timeCountLabel.GetComponent<TextMeshProUGUI>();
        timeCountText.text = PlayerStatsManager.obj.GetTimeDisplayString();

        InputDeviceListener.OnInputDeviceStream += HandleInputDeviceChanged;
        HandleInputDeviceChanged(InputDeviceListener.obj.GetCurrentInputDevice());

        StartCoroutine(FadeInScrollUI());
    }

    void OnDestroy() {
        InputDeviceListener.OnInputDeviceStream -= HandleInputDeviceChanged;
    }

    public void HandleScrollQuickerPerformed()
    {
        _animator.speed = 6f;
    }

    public void HandleScrollQuickerCanceled()
    {
        _animator.speed = 1f;
    }

    public void QuitToTitleScreen() {
        StartCoroutine(QuitToTitleScreenDelayed());
    }

    private IEnumerator QuitToTitleScreenDelayed() {
        yield return new WaitForSeconds(11f);
        float musicVolume = SoundMixerManager.obj.GetMusicVolume();
        StartCoroutine(SoundMixerManager.obj.StartMusicFade(3f, 0.001f));
        while(SoundMixerManager.obj.GetMusicVolume() > 0.001f) {
            yield return null;
        }
        //Give SoundMixerManager time to fully complete the fading
        yield return new WaitForSeconds(0.1f);
        MusicManager.obj.StopPlaying();
        SoundMixerManager.obj.SetMusicVolume(musicVolume);
                
        PauseMenuManager.obj.Quit();
    }

    public void HandleInputDeviceChanged(InputDeviceListener.Device device) {
        UpdateScrollUI(device);
    }

    public void UpdateScrollUI(InputDeviceListener.Device device) {
        if(device == InputDeviceListener.Device.Gamepad) {
            _scrollGamepadIcon.gameObject.SetActive(true);
            _scrollKeyboardText.gameObject.SetActive(false);
            Sprite scrollButtonSprite = GamepadIconManager.obj.GetIcon(scrollButtonActionReference.action);
            _scrollGamepadIcon.sprite = scrollButtonSprite;
        } else {
            _scrollGamepadIcon.gameObject.SetActive(false);
            _scrollKeyboardText.gameObject.SetActive(true);
            _scrollKeyboardText.text = scrollButtonActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
        }
    }

    private readonly float _fadeSpeed = 1f;
    private IEnumerator FadeInScrollUI() {
        yield return new WaitForSeconds(5);
        float alpha = 0;
        while(alpha < 1f) {
            alpha += Time.deltaTime * _fadeSpeed;
            
            _scrollGamepadIcon.color = new Color(_scrollGamepadIcon.color.r, _scrollGamepadIcon.color.g, _scrollGamepadIcon.color.b, alpha);
            _scrollKeyboardText.color = new Color(_scrollKeyboardText.color.r, _scrollKeyboardText.color.g, _scrollKeyboardText.color.b, alpha);
            _scrollText.color = new Color(_scrollText.color.r, _scrollText.color.g, _scrollText.color.b, alpha);

            yield return null;
        }
    }
}
