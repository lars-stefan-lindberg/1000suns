using System.Collections;
using UnityEngine;

public class Cave9RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _secretWallBroken;
    [SerializeField] private GameEventId _secretRoomConversationCompleted;
    [SerializeField] private GameObject _secretWall;
    [SerializeField] private Transform _sootFlyOff1;
    [SerializeField] private Transform _sootFlyOff2;
    [SerializeField] private Transform _sootFlyOff3;
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private GameObject _otherTorchLit1;
    [SerializeField] private GameObject _otherTorchLit2;
    [SerializeField] private GameObject _otherTorchUnlit1;
    [SerializeField] private GameObject _otherTorchUnlit2;

    private CaveTimelineId.Id _caveTimelineId;

    void Start()
    {
        _caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(GameManager.obj.HasEvent(_secretWallBroken))
            _secretWall.SetActive(false);
    }

    public void MaybeLightUpTorches() {
        if(GameManager.obj.HasEvent(_secretRoomConversationCompleted)) {
            _otherTorchLit1.SetActive(true);
            _otherTorchLit2.SetActive(true);
            _otherTorchUnlit1.SetActive(false);
            _otherTorchUnlit2.SetActive(false);
        } else if(_caveTimelineId != CaveTimelineId.Id.Eli) {
            _otherTorchLit1.SetActive(true);
            _otherTorchLit2.SetActive(true);
            _otherTorchUnlit1.SetActive(false);
            _otherTorchUnlit2.SetActive(false);
        }
    }

    public void OnSecretWallBroken() {
        GameManager.obj.RegisterEvent(_secretWallBroken);
        if(_caveTimelineId == CaveTimelineId.Id.Eli) {
            CaveAvatar.obj.SetTarget(_sootFlyOff2);
            StartCoroutine(SootFlyOffIntoRoom());
        }
    }

    private IEnumerator SootFlyOffIntoRoom() {
        yield return new WaitForSeconds(1.5f);
        CaveAvatar.obj.SetTarget(_sootFlyOff3);
    }

    public void MaybeSetSootFollow() {
        if(_caveTimelineId != CaveTimelineId.Id.Eli) {
            return;
        } 

        if(!GameManager.obj.HasEvent(_secretWallBroken) && !CaveAvatar.obj.IsFollowingPlayer) {
            CaveAvatar.obj.IsFollowingPlayer = true;
        } else if(!GameManager.obj.HasEvent(_secretRoomConversationCompleted) && !CaveAvatar.obj.IsFollowingPlayer) {
            CaveAvatar.obj.IsFollowingPlayer = true;
        }
    }

    public void TriggerSootFlyOff() {
        if(GameManager.obj.HasEvent(_secretWallBroken) || _caveTimelineId != CaveTimelineId.Id.Eli)
            return;
        StartCoroutine(FlySootOff());
    }

    private IEnumerator FlySootOff() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(0.5f);
        _zoomedCamera.SetActive(true);
        yield return new WaitForSeconds(1f);
        CaveAvatar.obj.SetTarget(_sootFlyOff1);
        if(!PlayerMovement.obj.IsFacingLeft())
            PlayerMovement.obj.FlipPlayer();
        yield return new WaitForSeconds(2f);
        _zoomedCamera.SetActive(false);
        PlayerMovement.obj.UnFreeze();
    }
}
