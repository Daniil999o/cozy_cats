using UnityEngine;

[System.Serializable]
public class YG_StoryPrompt
{
    public YG_StoryCharacter character;
    public string characterId;
    
    public string text;
    
    [Space]
    
    [Header("Duration by clip or fixed duration")]
    public AudioClip clip;
    public float fixedDuration = 3f;

    [Space]

    [Header("Delay After")]
    public float delayAfterPrinting = 1f;
}