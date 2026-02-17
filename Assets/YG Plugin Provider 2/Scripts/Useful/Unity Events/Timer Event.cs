using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TimerEvent
{
    public float delay = 0f;
    
    [Space]
    
    [Header("My Event")]
    public UnityEvent myEvent;
    public YG_StoryPromptGroup promptGroup;
    
    [Space]
    
    public float delayAfter = 0f;
}