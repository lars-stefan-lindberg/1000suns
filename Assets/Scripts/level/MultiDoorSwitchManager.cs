using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDoorSwitchManager : MonoBehaviour
{
    [SerializeField] private List<MultiDoorSwitch> _doorSwitches;
    [SerializeField] private GameObject _door;
    [SerializeField] private bool _isPermanent;  //Permanent -> when all switches are activated at the same time. Open door permanently. Else -> if any of the switches are triggered open door

    void FixedUpdate()
    {
        if(_isPermanent) {
            bool allTriggered = true;
            foreach(var sw in _doorSwitches) {
                if(!sw.IsTriggered) {
                    allTriggered = false;
                    break;
                }
            }
            if(allTriggered) {
                _door.SetActive(false);
            }
        } else {
            bool anyTriggered = false;
            foreach(var sw in _doorSwitches) {
                if(sw.IsTriggered) {
                    anyTriggered = true;
                    break;;
                }
            }
            _door.SetActive(!anyTriggered);
        }
    }
}
