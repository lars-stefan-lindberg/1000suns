using UnityEngine;

public class RemoveHiddenArea : MonoBehaviour
{
    private bool _hasBeenTriggered = false;
    public GameObject hiddenArea;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(_hasBeenTriggered)
            return;
        if (collision.gameObject.CompareTag("Player"))
        { 
            hiddenArea.SetActive(false);
            _hasBeenTriggered = true;
        }
    }   
}
