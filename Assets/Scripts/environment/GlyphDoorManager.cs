using UnityEngine;

public class GlyphDoorManager : MonoBehaviour
{
    [SerializeField] private GlyphStone _glyphStone;
    [SerializeField] private PillarDoor _pillarDoor;

    public void Activate() {
        _glyphStone.Activate();
        _pillarDoor.Open();
    }

    public void Deactivate() {
        _glyphStone.Deactivate();
        _pillarDoor.Close();
    }
}
