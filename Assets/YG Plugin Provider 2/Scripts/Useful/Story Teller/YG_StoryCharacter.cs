using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "YG Plugin Provider/Story Character")]
public class YG_StoryCharacter : ScriptableObject
{
    public string id = "character";
    public new string name;
    public Sprite avatar;
}