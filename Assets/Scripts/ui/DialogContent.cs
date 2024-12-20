using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/ New Dialogue Content")]
public class DialogueContent : ScriptableObject
{
    [TextArea(5, 10)]
    public string[] paragraphs;
}
