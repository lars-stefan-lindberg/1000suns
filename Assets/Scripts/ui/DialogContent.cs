using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Dialogue/ New Dialogue Content")]
public class DialogueContent : ScriptableObject
{
    public DialogueActor actor;
    public List<ParagraphEntry> paragraphEntries;
    public bool IsLeft = true;
    public bool IsFlipped = false;

    public enum DialogueActor {
        Player,
        CaveAvatar,
        Dee,
    }

    public enum Emotion {
        Idle,
        Surprised,
        Angry,
        Annoyed,
        Eh,
        Puzzled,
        VeryAngry,
        Hurt,
    }

    [System.Serializable]
    public struct ParagraphEntry {
        public LocalizedString localizedString;
        [TextArea(5, 10)]
        public string text;
        public Sprite portrait;
        public Emotion faceExpression;
        public Emotion eyesExpression;
        public DialogueSoundEffect soundEffect;
        public PortraitVFX vfx;
        public HoverSpeed hoverSpeed;  //Soot specific
    }
}
