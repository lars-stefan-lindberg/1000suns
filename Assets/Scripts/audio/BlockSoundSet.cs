using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Block Sound Set")]
public class BlockSoundSet : ScriptableObject
{
    public EventReference slide;
    public EventReference land;
    public EventReference slideOffEdge;
    public EventReference wallHit;
}
