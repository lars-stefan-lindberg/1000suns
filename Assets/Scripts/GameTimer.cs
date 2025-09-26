using UnityEngine;

public class GameTimer : MonoBehaviour
{
    private float elapsedTime = 0f;
    private bool isPaused = true;

    void Update()
    {
        if (!isPaused)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    public string GetTimeDisplayString()
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600);
        int minutes = Mathf.FloorToInt((elapsedTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        string displayString = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        Debug.Log("Actively played time: " + displayString);
        return displayString;
    }

    public float GetElapsedTime() {
        return elapsedTime;
    }
    public void SetElapsedTime(float time) {
        elapsedTime = time;
    }

    public void Reset() {
        elapsedTime = 0f;
    }

    public void PauseTimer() => isPaused = true;
    public void ResumeTimer() => isPaused = false;
}
