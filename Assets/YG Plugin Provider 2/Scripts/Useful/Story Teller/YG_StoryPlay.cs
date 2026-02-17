using UnityEngine;

public class YG_StoryPlay : MonoBehaviour
{
    [SerializeField] private YG_StoryPromptGroup[] storyPromptGroups;
    [SerializeField] private YG_StoryPromptGroup[] storyPromptRandomGroups;

    public void Play() {
        if (storyPromptGroups.Length > 0)
            YG_StoryTeller.Instance.PrintPrompts(storyPromptGroups);
        else if (storyPromptRandomGroups.Length > 0)
            YG_StoryTeller.Instance.PrintPrompt(storyPromptRandomGroups.Choice());
    }
}