using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class AnimatorDeepCopy
{
    [MenuItem("Assets/Create/Animator Deep Copy (with Clips)")]
    public static void DeepCopy()
    {
        Object obj = Selection.activeObject;
        if (!(obj is AnimatorController))
        {
            Debug.LogError("Select an AnimatorController first.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(obj);
        string dir = Path.GetDirectoryName(path);
        string name = Path.GetFileNameWithoutExtension(path);

        // Duplicate the controller
        string newControllerPath = Path.Combine(dir, name + "_Copy.controller");
        AssetDatabase.CopyAsset(path, newControllerPath);
        AnimatorController newController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newControllerPath);

        // Copy layers properly
        var layers = newController.layers;

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].stateMachine = CopyStateMachineRecursive(
                layers[i].stateMachine,
                dir
            );
        }

        newController.layers = layers;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Animator deep copy completed successfully!");
    }

    private static AnimatorStateMachine CopyStateMachineRecursive(AnimatorStateMachine sm, string dir)
    {
        // Copy all states inside this state machine
        foreach (var childState in sm.states)
        {
            if (childState.state.motion is AnimationClip clip)
            {
                string clipPath = AssetDatabase.GetAssetPath(clip);
                string newClipPath = Path.Combine(
                    dir,
                    Path.GetFileNameWithoutExtension(clipPath) + "_Copy.anim"
                );

                AssetDatabase.CopyAsset(clipPath, newClipPath);
                AnimationClip newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath);

                childState.state.motion = newClip;
            }
        }

        // Recursively copy nested state machines
        for (int i = 0; i < sm.stateMachines.Length; i++)
        {
            sm.stateMachines[i].stateMachine = CopyStateMachineRecursive(
                sm.stateMachines[i].stateMachine,
                dir
            );
        }

        return sm;
    }
}
