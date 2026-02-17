using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class YG_StoryTeller : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup storyBlockGroup;
    [SerializeField] private Image characterAvatarImage;
    [SerializeField] private TMP_Text characterNameInfo;
    [SerializeField] private TMP_Text promptTextInfo;

    [Space]

    [Header("Options")]
    [SerializeField] private bool useRealtime = true;
    [SerializeField] private bool usePrinting = true;
    [SerializeField] private bool showAllOnSkip = true;
    
    [Space]
    
    [Header("Print Options")]
    [SerializeField] private AudioClip printClip;

    [Space]

    [Header("Audio")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource printSource;

    [Space]

    [Header("Characters")]
    [SerializeField] private YG_StoryCharacter[] characters;

    [Space]

    [Header("Show On Start")]
    [SerializeField] private float showOnStartTimer = 3f;
    [SerializeField] private YG_StoryPromptGroup[] showOnStartStoryGroups;
    [SerializeField] private bool useAnotherForMobile;
    [SerializeField] private YG_StoryPromptGroup[] showOnStartStoryGroupsMobile;

    private YG_StoryCharacter GetCharacterById(string id) => characters.First(x => x.id == id);

    private YG_StoryPrompt[] FixPrompts(YG_StoryPrompt[] prompts) {
        for (int i = 0; i < prompts.Length; i++) {
            YG_StoryPrompt myPrompt = prompts[i];
            
            if (!myPrompt.character && !myPrompt.characterId.IsEmptyOrWhiteSpaces())
                myPrompt.character = GetCharacterById(myPrompt.characterId);

            prompts[i] = myPrompt;
        }

        return prompts;
    }
    
    private static YG_StoryTeller _instance;

    public static YG_StoryTeller Instance {
        get {
            if (!_instance) _instance = FindFirstObjectByType<YG_StoryTeller>();

            return _instance;
        }
    }

    public static bool InDialogue => Instance && Instance._inDialogue;

    private IEnumerator _printPromptCoroutine;

    private const float BlockShowSpeed = 2f;

    private bool _inDialogue;

    private bool _skip;

    private void Awake() {
        storyBlockGroup.alpha = 0f;
    }

    private void Start() {
        if (showOnStartStoryGroups.Length > 0) Invoke(nameof(ShowStartPrompts), showOnStartTimer);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E)) Skip();
    }

    public void Skip() {
        _skip = true;
        print("Skp");
    }

    private void ShowStartPrompts() =>
        PrintPrompts(YG_Provider.IsMobile && useAnotherForMobile ? showOnStartStoryGroupsMobile : showOnStartStoryGroups
        );

    public void PrintPrompt(YG_StoryPromptGroup promptGroup) =>
        PrintPrompts(new[] {
                promptGroup
            }
        );

    public void PrintPrompts(YG_StoryPromptGroup[] promptGroups) {
        List<YG_StoryPrompt> prompts = new List<YG_StoryPrompt>();
        
        for (int i = 0; i < promptGroups.Length; i++) {
            YG_StoryPromptGroup group = promptGroups[i];

            foreach (var x in group.prompts) {
                YG_StoryPrompt prompt = new YG_StoryPrompt {
                    character = group.character,
                    characterId = group.character.id,
                    clip = x.clip,
                    fixedDuration = x.fixedDuration,
                    text = x.text,
                    delayAfterPrinting = group.delayAfterPrinting
                };
                
                prompts.Add(prompt);
            }
        }
        
        PrintPrompts(prompts.ToArray());
    }

    public void PrintPrompt(YG_StoryPrompt prompt) =>
        PrintPrompts(new[] {
                prompt
            }
        );

    public void PrintPrompts(YG_StoryPrompt[] prompts) {
        if (_printPromptCoroutine != null) StopCoroutine(_printPromptCoroutine);

        _printPromptCoroutine = IE_PrintPrompts(FixPrompts(prompts));

        StartCoroutine(_printPromptCoroutine);
    }

    private IEnumerator IE_PrintPrompts(YG_StoryPrompt[] prompts) {
        if (prompts.Length <= 0) yield break;

        _inDialogue = true;
        
        promptTextInfo.text = "";
        
        for (int i = 0; i < prompts.Length; i++)
            yield return IE_PrintPrompt(prompts[i]);

        while (storyBlockGroup.alpha > 0f) {
            if (useRealtime)
                storyBlockGroup.alpha -= BlockShowSpeed * Time.unscaledDeltaTime;
            else
                storyBlockGroup.alpha -= BlockShowSpeed * Time.deltaTime;

            yield return null;
        }
        
        _inDialogue = false;
    }

    private IEnumerator IE_PrintPrompt(YG_StoryPrompt prompt) {
        if (prompt.text.IsEmptyOrWhiteSpaces()) yield break;

        _skip = false;
        
        characterAvatarImage.sprite = prompt.character.avatar;
        characterNameInfo.text = prompt.character.name;

        promptTextInfo.text = "";

        while (storyBlockGroup.alpha < 1f) {
            if (useRealtime)
                storyBlockGroup.alpha += BlockShowSpeed * Time.unscaledDeltaTime;
            else
                storyBlockGroup.alpha += BlockShowSpeed * Time.deltaTime;

            yield return null;
        }

        float delay = prompt.fixedDuration;
        string text = prompt.text;

        if (usePrinting) delay /= text.Length;
        
        if (prompt.clip) {
            float duration = prompt.clip.length;

            if (usePrinting)
                delay = duration / text.Length;
            else
                delay = duration;

            if (voiceSource) {
                voiceSource.clip = prompt.clip;
                voiceSource.Play();

                yield return new WaitUntil(() => voiceSource.isPlaying);
            }
        }

        float myAudioTime = 0f;
        float audioTime = prompt.clip.length;

        if (usePrinting) {
            for (int i = 0; i < text.Length; i++) {
                while (enabled && (YG_GameMenu.Instance && YG_GameMenu.Paused || YG_AdTimer.Instance && YG_AdTimer.Instance.InAd())) {
                    voiceSource.Pause();
                    
                    yield return null;
                }

                if (!voiceSource.isPlaying && myAudioTime < audioTime * 0.9f) {
                    voiceSource.Play();
                    
                    yield return new WaitUntil(() => voiceSource.isPlaying);
                }
                
                promptTextInfo.text += text[i];
                if (printSource && printClip)
                    printSource.PlayOneShot(printClip);
                
                if (useRealtime)
                    yield return new WaitForSecondsRealtime(delay);
                else
                    yield return new WaitForSeconds(delay);

                myAudioTime += delay;

                if (_skip) {
                    if (voiceSource) voiceSource.Stop();
                    promptTextInfo.text = text;

                    if (showAllOnSkip) {
                        _skip = false;

                        float shortDelay = prompt.delayAfterPrinting / 10f;

                        for (int j = 10; j > 0; j--) {
                            yield return new WaitForSecondsRealtime(shortDelay);
                            
                            if (_skip) yield break;
                        }
                    }

                    yield break;
                }
            }
        } else {
            promptTextInfo.text = text;

            if (useRealtime)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return new WaitForSeconds(delay);
        }

        if (useRealtime)
            yield return new WaitForSecondsRealtime(prompt.delayAfterPrinting);
        else
            yield return new WaitForSeconds(prompt.delayAfterPrinting);
        
        promptTextInfo.text = "";
    }
}