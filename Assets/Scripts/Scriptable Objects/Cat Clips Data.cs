using UnityEngine;

[CreateAssetMenu(fileName = "Cat Clips Data", menuName = "Scriptable Objects/Cat Clips Data")]
public class CatClipsData : ScriptableObject
{
    public AudioClip[] enterClips;
    public AudioClip[] exitClips;
    
    [Space]
    
    public AudioClip[] pickUpClips;
    public AudioClip[] dropClips;
}