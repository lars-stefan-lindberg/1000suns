using UnityEngine;

public class Forest2GlyphTriggeredManager : MonoBehaviour
{
    [SerializeField] private GlyphStone _glyphStone;
    [SerializeField] private GameObject _rainSystems;
    [SerializeField] private ThunderLight _thunderLight;

    public void Activate() {
        _glyphStone.Activate();
        _rainSystems.SetActive(true);

        _thunderLight.Flash();
        
        //Play rain ambience
        //Play music
    }
}
