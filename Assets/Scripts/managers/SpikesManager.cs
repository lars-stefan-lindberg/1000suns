using UnityEngine;

public class SpikesManager : MonoBehaviour
{
    [SerializeField] private Spike[] _spikes;

    [ContextMenu("Release Spikes")]
    public void ReleaseSpikes() {
        foreach (var spike in _spikes) {
            spike.InitiateFall();
        }
    }
}
