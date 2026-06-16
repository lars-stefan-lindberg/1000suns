using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Blob Sound Set")]
public class BlobSoundSet : ScriptableObject
{
    public EventReference land;
    public EventReference jump;
    public EventReference footstep;
    public EventReference shapeshift;
    public EventReference chargeStart;
    public EventReference chargeRelease;
}
