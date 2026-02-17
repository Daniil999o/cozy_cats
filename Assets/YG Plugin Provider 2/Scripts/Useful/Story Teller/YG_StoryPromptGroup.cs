using UnityEngine;

[CreateAssetMenu(fileName = "Prompt", menuName = "YG Plugin Provider/Story Prompt Group")]
public class YG_StoryPromptGroup : ScriptableObject
{
    public YG_StoryCharacter character;

    [Space]

    [Header("Delay After")]
    public float delayAfterPrinting = 1f;

    [Space]

    public YG_StoryPromptGroupText[] prompts;
}

[System.Serializable]
public class YG_StoryPromptGroupText
{
    public string id = "Text";
    
    [Space]
    
    [TextArea(4, 8)] public string text;
    
    [Space]
    
    [Header("Duration by clip or fixed duration")]
    public AudioClip clip;
    public float fixedDuration;

    [System.NonSerialized]
    public string displayedText = ""; 
}