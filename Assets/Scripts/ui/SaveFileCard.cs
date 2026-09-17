using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SaveFileCard : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _location;
    [SerializeField] private TMP_Text _numberOfDeaths;
    [SerializeField] private TMP_Text _numberOfCaveCreatures;
    [SerializeField] private TMP_Text _playTime;
    [SerializeField] private GameObject _newGameContainer;
    [SerializeField] private GameObject _portraitContainer;
    [SerializeField] private GameObject _detailsContainer;
    [SerializeField] private GameObject _deleteSaveFileContainer;
    [SerializeField] private GameObject _deleteSaveFileContainerCancelButton;
    [SerializeField] private GameObject _eliPortrait;
    [SerializeField] private GameObject _deePortrait;
    [SerializeField] private GameObject _bothPortrait;
    
    [Header("Blinking Settings")]
    [SerializeField] private float _blinkMinDelay = 2f;
    [SerializeField] private float _blinkMaxDelay = 6f;
    [SerializeField] [Range(0f, 1f)] private float _doubleBlinkChance = 0.15f;

    public UnityEvent OnDeleteSaveFileConfirm;
    public UnityEvent OnDeleteSaveFileCancel;

    private GameObject _currentPortrait;
    private ISaveFileCardPortrait _portraitInterface;
    private Coroutine _blinkCoroutine;
    private Coroutine _startAnimationsCoroutine;

    public void CreateFromSave(SaveData saveData) {
        _newGameContainer.SetActive(false);
        _portraitContainer.SetActive(true);
        _detailsContainer.SetActive(true);

        if(saveData.caveTimeline == CaveTimelineId.Id.Both) {
            //TODO set portrait
            _name.text = "Eli & Dee";
        } else if(saveData.caveTimeline == CaveTimelineId.Id.Eli) {
            if(_currentPortrait == null) {
                _currentPortrait = Instantiate(_eliPortrait);
                _currentPortrait.transform.SetParent(_portraitContainer.transform);
                RectTransform rectTransform = _currentPortrait.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = Vector2.zero;
                _currentPortrait.transform.localScale = new Vector3(0.67f, 0.67f, 0.67f);
                _currentPortrait.transform.localRotation = _portraitContainer.transform.localRotation;
                _portraitInterface = _currentPortrait.GetComponent<EliSaveFileCardPortrait>();

                if (_portraitInterface is EliSaveFileCardPortrait eliPortrait)
                {
                    if(saveData.hasCape)
                        eliPortrait.SetCape(true);
                    else
                        eliPortrait.SetCape(false);
                }
            }
            _name.text = "Eli";
        } else if(saveData.caveTimeline == CaveTimelineId.Id.Dee) {
            if(_currentPortrait == null) {
                _currentPortrait = Instantiate(_deePortrait);
                _currentPortrait.transform.SetParent(_portraitContainer.transform);
                RectTransform rectTransform = _currentPortrait.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, -6.8f);
                _currentPortrait.transform.localScale = new Vector3(0.67f, 0.67f, 0.67f);
                _currentPortrait.transform.localRotation = _portraitContainer.transform.localRotation;
                _portraitInterface = _currentPortrait.GetComponent<DeeSaveFileCardPortrait>();

                if (_portraitInterface is DeeSaveFileCardPortrait deePortrait)
                {
                    if(saveData.hasCrown)
                        deePortrait.SetCrown(true);
                    else
                        deePortrait.SetCrown(false);
                }
            }
            _name.text = "Dee";
        }

        // Activate the portrait
        _currentPortrait.SetActive(true);
        
        // Get localized location string from the location key
        if (GameLocationManager.obj != null && !string.IsNullOrEmpty(saveData.locationKey))
        {
            _location.text = GameLocationManager.obj.GetLocalizedLocation(saveData.locationKey);
        }
        
        _numberOfDeaths.text = "x " + saveData.playerDeaths;
        _numberOfCaveCreatures.text = "x " + saveData.pickedCollectibles.Count;
        _playTime.text = GetTimeDisplayString(saveData.timePlayed);
    }

    public string GetTimeDisplayString(float elapsedTime)
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600);
        int minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        string displayString = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        return displayString;
    }

    public void ShowDeleteSaveFileContainer() {
        _portraitContainer.SetActive(false);
        _detailsContainer.SetActive(false);
        _deleteSaveFileContainer.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_deleteSaveFileContainerCancelButton);
    }

    public void HideDeleteSaveFileContainer() {
        _deleteSaveFileContainer.SetActive(false);
        _portraitContainer.SetActive(true);
        _detailsContainer.SetActive(true);
    }
    
    public void OnDeleteSaveFileConfirmButtonClicked() {
        Destroy(_currentPortrait);
        _currentPortrait = null;
        _portraitInterface = null;
        OnDeleteSaveFileConfirm?.Invoke();
    }

    public void OnDeleteSaveFileCancelButtonClicked() {
        OnDeleteSaveFileCancel?.Invoke();
    }

    public void ShowNewGameContainer() {
        _newGameContainer.SetActive(true);
        _portraitContainer.SetActive(false);
        _detailsContainer.SetActive(false);
        _deleteSaveFileContainer.SetActive(false);
    }

    public void StartAnimations() {
        if(_portraitInterface == null) {
            return;
        }
        
        // Stop any existing delayed start
        if(_startAnimationsCoroutine != null) {
            StopCoroutine(_startAnimationsCoroutine);
        }
        _startAnimationsCoroutine = StartCoroutine(StartAnimationsDelayed());
    }
    
    private IEnumerator StartAnimationsDelayed() {
        // Wait a couple frames to ensure the portrait GameObject is fully initialized
        yield return null;
        yield return null;
        
        if(_portraitInterface != null) {
            _portraitInterface.StartAnimation();
            StartBlinking();
        }
        _startAnimationsCoroutine = null;
    }

    public void StopAnimations() {
        // Cancel any pending delayed start
        if(_startAnimationsCoroutine != null) {
            StopCoroutine(_startAnimationsCoroutine);
            _startAnimationsCoroutine = null;
        }
        
        if(_portraitInterface == null) {
            return;
        }
        _portraitInterface.StopAnimation();
        StopBlinking();
    }
    
    private IEnumerator BlinkRoutine() {
        while(true) {
            float delay = UnityEngine.Random.Range(_blinkMinDelay, _blinkMaxDelay);
            yield return new WaitForSeconds(delay);
            
            if(_portraitInterface != null) {
                float roll = UnityEngine.Random.value;
                if(roll < _doubleBlinkChance) {
                    _portraitInterface.DoubleBlink();
                } else {
                    _portraitInterface.Blink();
                }
            }
        }
    }
    
    private void StartBlinking() {
        StopBlinking();
        if(_portraitInterface != null) {
            _blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }
    
    private void StopBlinking() {
        if(_blinkCoroutine != null) {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
    }
}
