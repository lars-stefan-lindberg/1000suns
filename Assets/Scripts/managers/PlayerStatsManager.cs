using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager obj;
    public int numberOfDeaths = 0;
    private GameTimer _gameTimer;

    void Awake()
    {
        obj = this;    
        _gameTimer = gameObject.AddComponent<GameTimer>();
    }

    void Start()
    {
       ResetStats(); 
    }

    public void ResetStats() {
        _gameTimer.Reset();
        numberOfDeaths = 0;
    }

    public void ResumeTimer() {
        _gameTimer.ResumeTimer();
    }
    public void PauseTimer() {
        _gameTimer.PauseTimer();
    }
    [ContextMenu("Get time display string")]
    public string GetTimeDisplayString() {
        return _gameTimer.GetTimeDisplayString();
    }

    void OnDestroy()
    {
        obj = null;
    }
}
