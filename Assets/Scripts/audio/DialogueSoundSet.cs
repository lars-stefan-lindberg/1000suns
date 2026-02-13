using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Dialogue Sound Set")]
public class DialogueSoundSet : ScriptableObject
{
    public EventReference open;
    public EventReference close;
    public EventReference confirm;
}
