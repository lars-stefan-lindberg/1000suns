using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Dee Sound Set")]
public class DeeSoundSet : ScriptableObject
{
    public EventReference shadowPullGrab;
    public EventReference shadowPullMoveLoop;
    public EventReference shadowPullLoop;
    public EventReference shadowPullRelease;
    public EventReference anchorReached;
}
