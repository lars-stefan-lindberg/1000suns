using System.Collections.Generic;
using UnityEngine;

public class GameProgressDebugger : MonoBehaviour
{
    GameProgress progress;

    public IReadOnlyList<string> Events =>
        progress?.GetCompletedEventIds();

    public void Bind(GameProgress progress)
    {
        this.progress = progress;
    }
}