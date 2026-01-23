using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class PlayerPowersManager : MonoBehaviour
{
    public static PlayerPowersManager obj;

    public bool EliCanShadowDash {get; set;} //Deprecated, but might use in future
    public bool EliCanForcePushJump {get; set;} //Deprecated, but might use in future
    public bool EliBlobCanExtraJump { get; set; } //Deprecated, but might use in future
    public bool EliCanForcePush { get; set; }
    public bool EliCanTurnFromHumanToBlob { get; set; }
    public bool EliCanTurnFromBlobToHuman { get; set; }
    public bool EliBlobCanJump { get; set; }
    public bool DeeCanForcePull { get; set; }
    public bool CanSwitchBetweenTwinsMerged { get; set; }
    public bool CanSeparate { get; set; }

    void Awake() {
        obj = this;
    }

    void Start()
    {
        ResetGameEvents();
        if(GameManager.obj != null && GameManager.obj.isDevMode) {
            EliCanForcePush = true;
            EliCanTurnFromBlobToHuman = true;
            EliCanTurnFromHumanToBlob = true;
            EliBlobCanJump = true;
            DeeCanForcePull = true;
            CanSwitchBetweenTwinsMerged = true;
            CanSeparate = true;
        }      
    }

    void OnEnable()
    {
        if(GameManager.obj != null && GameManager.obj.isDevMode) {
            EliCanForcePush = true;
            EliCanTurnFromBlobToHuman = true;
            EliCanTurnFromHumanToBlob = true;
            EliBlobCanJump = true;
            DeeCanForcePull = true;
            CanSwitchBetweenTwinsMerged = true;
            CanSeparate = true;
        }     
    }

    public void ResetGameEvents() {
        EliCanForcePush = false;
        EliCanTurnFromHumanToBlob = false;
        EliCanTurnFromBlobToHuman = false;
        EliBlobCanJump = false;
        DeeCanForcePull = false;
        CanSwitchBetweenTwinsMerged = false;
        CanSeparate = false;
    }

    // Returns a list of power keys (by property name) that are enabled
    public List<string> GetUnlockedPowers()
    {
        var unlocked = new List<string>();
        var props = typeof(PlayerPowersManager).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var prop in props)
        {
            if (prop.PropertyType == typeof(bool) && prop.CanRead && prop.CanWrite)
            {
                var value = (bool)(prop.GetValue(this) ?? false);
                if (value && prop.Name != nameof(EliCanForcePushJump))
                {
                    unlocked.Add(prop.Name);
                }
            }
        }
        return unlocked;
    }

    // Applies a list of power keys (by property name) to enable powers
    public void ApplyUnlockedPowers(List<string> powers)
    {
        // If in dev mode, keep everything enabled and ignore loaded powers to avoid overriding dev settings
        if (GameManager.obj != null && GameManager.obj.isDevMode)
        {
            var propsAll = typeof(PlayerPowersManager).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var prop in propsAll)
            {
                if (prop.PropertyType == typeof(bool) && prop.CanWrite && prop.CanRead)
                {
                    prop.SetValue(this, true);
                }
            }
            return;
        }

        // Start from a clean slate
        ResetGameEvents();

        if (powers == null || powers.Count == 0)
        {
            return;
        }

        var toSet = new HashSet<string>(powers);
        var props = typeof(PlayerPowersManager).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var prop in props)
        {
            if (prop.PropertyType == typeof(bool) && prop.CanWrite && prop.CanRead)
            {
                if (toSet.Contains(prop.Name))
                {
                    prop.SetValue(this, true);
                }
            }
        }
    }

    void OnDestroy() {
        obj = null;
    }
}

