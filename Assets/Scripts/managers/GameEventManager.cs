using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager obj;

    //Cutscene events
    public bool PrisonerIntroSeen { get; set; }
    public bool BabyPrisonerAlerted { get; set; }
    public bool FirstPrisonerFightStarted { get; set; }
    public bool FirstPrisonerKilled { get; set; }
    public bool FirstCaveMiniBossKilled { get; set; }
    public bool CapePicked { get; set; }
    public bool FirstPowerUpPicked { get; set; }
    public bool PowerUpRoomsFloorBroken { get; set; }
    public bool CaveLevelStarted { get; set; }
    public bool AfterPowerUpRoomsCompletedWallBreak { get; set; }
    public bool PowerUpRoomCompletedWallBreak { get; set; }
    public bool C215WallBroken { get; set; }
    public bool FirstCaveCollectibleConversationEnded { get; set; }

    public bool IsPauseAllowed { get; set; }

    void Awake() {
        obj = this;
        ResetGameEvents();
    }

    public void ResetGameEvents() {
        PrisonerIntroSeen = false;
        BabyPrisonerAlerted = false;
        CapePicked = false;
        FirstPowerUpPicked = false;
        PowerUpRoomsFloorBroken = false;
        CaveLevelStarted = false;
        AfterPowerUpRoomsCompletedWallBreak = false;
        PowerUpRoomCompletedWallBreak = false;
        IsPauseAllowed = true;
        C215WallBroken = false;
        FirstCaveCollectibleConversationEnded = false;
        FirstPrisonerFightStarted = false;
        FirstPrisonerKilled = false;
        FirstCaveMiniBossKilled = false;
    }

    void OnDestroy() {
        obj = null;
    }
}
