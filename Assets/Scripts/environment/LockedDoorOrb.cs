using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LockedDoorOrb : MonoBehaviour
{
    public GameObject lockedDoor;
    public int numberOfSoulsBeforeUnlock = 0;
    private Animator _animator;
    private int _numberOfSoulsCount = 0;
    private SpriteRenderer _spriteRenderer;
    private Color _fadeOutStartColor;
    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 5f;

    void Awake() {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _fadeOutStartColor = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("PrisonerSoul")) {
            _numberOfSoulsCount += 1;
            _animator.SetTrigger("absorb");
            Destroy(other.gameObject);
            if(_numberOfSoulsCount == numberOfSoulsBeforeUnlock) {
                lockedDoor.GetComponent<LockedDoor>().PlayDeathAnimation();
                SoundFXManager.obj.PlayMonsterDoorDestroy(transform);
                StartCoroutine(FadeOut());
            } else {
                switch(_numberOfSoulsCount) {
                    case 1:
                        SoundFXManager.obj.PlayPrisonerSoulAbsorb1(transform);
                        break;
                    case 2:
                        SoundFXManager.obj.PlayPrisonerSoulAbsorb2(transform);
                        break;
                    default:
                        SoundFXManager.obj.PlayPrisonerSoulAbsorb3(transform);
                        break;
                }
            }
        }
    }

    private IEnumerator FadeOut() {
        while(_spriteRenderer.color.a > 0f) {
            _fadeOutStartColor.a -= Time.deltaTime * _fadeOutSpeed;
            _spriteRenderer.color = _fadeOutStartColor;
            yield return null;
        }
    }
}
