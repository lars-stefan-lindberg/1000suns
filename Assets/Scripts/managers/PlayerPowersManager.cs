using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class PlayerPowersManager : MonoBehaviour
{
    public static PlayerPowersManager obj;

    public bool CanFallDash {get; set;}
    public bool CanForcePushJump {get; set;}
    public bool CanTurnFromHumanToBlob { get; set; }
    public bool CanTurnFromBlobToHuman { get; set; }
    public bool BlobCanJump { get; set; }
    public bool BlobCanExtraJump { get; set; }

    void Awake() {
        obj = this;
        ResetGameEvents();
        if(PlayerMovement.obj.isDevMode) {
            CanFallDash = true;
            CanForcePushJump = true;
            CanTurnFromBlobToHuman = true;
            CanTurnFromHumanToBlob = true;
            BlobCanJump = true;
            BlobCanExtraJump = true;
        }     
    }

    void OnEnable()
    {
        if(PlayerMovement.obj.isDevMode) {
            CanFallDash = true;
            CanForcePushJump = true;
            CanTurnFromBlobToHuman = true;
            CanTurnFromHumanToBlob = true;
            BlobCanJump = true;
            BlobCanExtraJump = true;
        }     
    }

    public void ResetGameEvents() {
        CanFallDash = false;
        CanForcePushJump = false;
        CanTurnFromHumanToBlob = false;
        CanTurnFromBlobToHuman = false;
        BlobCanJump = false;
        BlobCanExtraJump = false;
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
                if (value)
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
        if (PlayerMovement.obj != null && PlayerMovement.obj.isDevMode)
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

