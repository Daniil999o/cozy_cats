using UnityEngine;
using System.Collections;

public class TimerEventsPlayer : MonoBehaviour
{
    [SerializeField] private bool invokeOnStart;

    [Space]
    
    [SerializeField] private bool invokeOnce = true;
    [SerializeField] private bool invokeOrdered = true;
    [SerializeField] private bool waitDialogues = true;
    
    [Space]
    
    [SerializeField] private TimerEvent[] timerEvents;

    private IEnumerator _invokeEventsCoroutine;
    private bool _invoked;

    private void Start() {
        if (invokeOnStart) InvokeEvents();
    }
    
    public void InvokeEvents() {
        if (_invoked) return;

        if (invokeOnce) _invoked = true;
        
        if (_invokeEventsCoroutine != null) StopCoroutine(_invokeEventsCoroutine);

        _invokeEventsCoroutine = IE_InvokeEvents();
        StartCoroutine(_invokeEventsCoroutine);
    }

    public void StopEvents() {
        StopAllCoroutines();
    }
    
    private IEnumerator IE_InvokeEvents() {
        for (int i = 0; i < timerEvents.Length; i++) {
            if (invokeOrdered)
                yield return StartCoroutine(IE_InvokeEvent(timerEvents[i]));
            else
                StartCoroutine(IE_InvokeEvent(timerEvents[i]));
        }
    }

    private IEnumerator IE_InvokeEvent(TimerEvent timerEvent) {
        float delay = timerEvent.delay;
        float delayAfter = timerEvent.delayAfter;
        YG_StoryPromptGroup promptGroup = timerEvent.promptGroup;

        if (delay > 0f) yield return new WaitForSeconds(delay);
        
        timerEvent.myEvent?.Invoke();

        if (promptGroup) {
            YG_StoryTeller.Instance.PrintPrompt(promptGroup);

            yield return null;

            if (waitDialogues)
                yield return new WaitUntil(() => !YG_StoryTeller.InDialogue);
        }

        if (delayAfter > 0f) yield return new WaitForSeconds(delayAfter);
    }
}