using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Audio/UI Sound Library")]
public class UISoundLibrary : ScriptableObject
{
    public EventReference browse;
    public EventReference select;
    public EventReference back;
    public EventReference playGame;
    public EventReference sliderTick;
}
