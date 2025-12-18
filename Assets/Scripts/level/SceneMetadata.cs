using UnityEngine;

public class SceneMetadata : MonoBehaviour
{
    [SerializeField] private bool ReloadOnDeath = false;

    public bool ShouldReloadOnDeath() => ReloadOnDeath;
}
