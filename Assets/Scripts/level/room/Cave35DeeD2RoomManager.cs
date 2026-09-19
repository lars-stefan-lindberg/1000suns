using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.InputSystem;

public class Cave35DeeD2RoomManager : MonoBehaviour
{
    [SerializeField] private TutorialStrip _tutorialStrip;
    [SerializeField] private LocalizedString _basicMovementString;
    [SerializeField] private List<InputActionReference> _basicMovementActions;
    [SerializeField] private List<InputIconManager.Direction> _basicMovementDirections;
    
    [SerializeField] private LocalizedString _wallLatchString;
    [SerializeField] private List<InputActionReference> _wallLatchActions;
    [SerializeField] private List<InputIconManager.Direction> _wallLatchDirections;

    void Start() {
        var steps = new List<ConditionalTutorialStep>
        {
            // Step 0: Basic movement (default, shown when no other condition is true)
            new ConditionalTutorialStep
            {
                step = new TutorialStep
                {
                    localizedString = _basicMovementString,
                    inputActions = _basicMovementActions,
                    inputDirections = _basicMovementDirections
                },
                condition = null // No condition = default step
            },
            
            // Step 1: Wall latch tutorial (shown when latched to wall)
            new ConditionalTutorialStep
            {
                step = new TutorialStep
                {
                    localizedString = _wallLatchString,
                    inputActions = _wallLatchActions,
                    inputDirections = _wallLatchDirections
                },
                condition = () => ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.IsLatchedToWall()
            }
        };
        
        _tutorialStrip.InitializeConditional(steps);
    }
    
    public void ShowTutorialStrip() {
        _tutorialStrip.Show();
    }

    public void HideTutorialStrip() {
        _tutorialStrip.Hide();
    }
    
    public void HideTutorialStripQuick() {
        _tutorialStrip.HideQuick();
    }
}
