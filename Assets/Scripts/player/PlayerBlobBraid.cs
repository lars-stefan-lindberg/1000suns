using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlobBraid : MonoBehaviour
{
    public static PlayerBlobBraid obj;

    [SerializeField] private float _braidPartLerpSpeed = 0.04f;
    [SerializeField] private GameObject _braidHeadLeft;
    [SerializeField] private GameObject _braidHeadRight;
    [SerializeField] private GameObject[] _braidParts;

    void Awake() {
        obj = this;
    }

    void FixedUpdate()
    {
        Vector2 headTargetPosition;
        bool isPlayerFacingLeft = PlayerBlobMovement.obj.IsFacingLeft();
        if(!isPlayerFacingLeft) {
            headTargetPosition = _braidHeadLeft.transform.position;
        } else {
            headTargetPosition = _braidHeadRight.transform.position;
        }

        for (int i = 0; i < _braidParts.Length; i++) {
            if (i == 0) {
                _braidParts[i].transform.position = Vector2.Lerp(_braidParts[i].transform.position, headTargetPosition, _braidPartLerpSpeed);
            } else {
                _braidParts[i].transform.position = Vector2.Lerp(_braidParts[i].transform.position, _braidParts[i-1].transform.position, _braidPartLerpSpeed);
            }
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
