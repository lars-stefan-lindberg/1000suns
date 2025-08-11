using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/ New Dialogue Content")]
public class DialogueContent : ScriptableObject
{
    public DialogueActor actor;
    public List<ParagraphEntry> paragraphEntries;

    public enum DialogueActor {
        Player,
        CaveAvatar,
    }

    [System.Serializable]
    public struct ParagraphEntry {
        [TextArea(5, 10)]
        public string text;
        public Sprite portrait;
    }
}
