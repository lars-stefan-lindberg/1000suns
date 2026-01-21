using UnityEngine;

[CreateAssetMenu(menuName = "Game/CaveTimeline")]
public class CaveTimelineId : ScriptableObject
{
    public Id id;

    public enum Id {
        Eli,
        Dee,
        Both
    }
}
