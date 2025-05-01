using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager obj;

    //Cutscene events
    public bool CaveLevelStarted { get; set; }
    public bool C1MonologueEnded { get; set; }
    public bool CaveAvatarFreed { get; set; }
    public bool CapePicked { get; set; }
    public bool FirstCaveCollectibleConversationEnded { get; set; }
    public bool BabyPrisonerAlerted { get; set; }
    public bool PrisonerIntroSeen { get; set; }
    public bool FirstPrisonerFightStarted { get; set; }
    public bool FirstPrisonerKilled { get; set; }
    public bool FirstCaveMiniBossKilled { get; set; }
    public bool PowerUpRoomsFloorBroken { get; set; }
    public bool FirstPowerUpPicked { get; set; }
    public bool PowerUpRoomCompletedWallBreak { get; set; }
    public bool AfterPowerUpRoomsCompletedWallBreak { get; set; }
    public bool C215WallBroken { get; set; }
    public bool C275FloorBroken { get; set; }
    public bool C26CutsceneCompleted { get; set; }
    public bool C27CutsceneCompleted { get; set; }
    public bool C30CutsceneCompleted { get; set; }

    public bool IsPauseAllowed { get; set; }

    void Awake() {
        obj = this;
        ResetGameEvents();
    }

    public void ResetGameEvents() {
        CaveAvatarFreed = false;
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
        C275FloorBroken = false;
        FirstCaveCollectibleConversationEnded = false;
        FirstPrisonerFightStarted = false;
        FirstPrisonerKilled = false;
        FirstCaveMiniBossKilled = false;
        C1MonologueEnded = false;
        C26CutsceneCompleted = false;
        C27CutsceneCompleted = false;
        C30CutsceneCompleted = false;
    }

    void OnDestroy() {
        obj = null;
    }
}
