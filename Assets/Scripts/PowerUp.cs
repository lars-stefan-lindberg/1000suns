using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private float _recoveryTime = 10;
    private float _recoveryTimer = 0;

    private bool _isPicked = false;
    private bool _playerEntered = false;
    private bool _isSpawned = false;

    void Awake() {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _playerEntered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _playerEntered = false;
        }
    }

    void FixedUpdate() {
        if(_isSpawned && _playerEntered && !_isPicked && !Player.obj.hasPowerUp) {
            Player.obj.SetHasPowerUp(true);
            _animator.SetBool("isPicked", true);
            _isPicked = true;
        }
        if(_isPicked) {
            _recoveryTimer += Time.deltaTime;
            if(_recoveryTimer >= _recoveryTime) {
                _animator.SetBool("isPicked", false);
            }
        }
    }

    public void SetRecovered() {
        _isPicked = false;
        _isSpawned = true;
        _recoveryTimer = 0;
    }
}
