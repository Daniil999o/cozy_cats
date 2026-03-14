using System;
using TMPro;
using UnityEngine;

public class UIModularDialogueWindow : MonoBehaviour, IUIModalWindow
{
    [SerializeField] private GameObject windowRoot;

    [Space]

    [SerializeField] private TMP_Text windowTitle;

    public bool Active => windowRoot.activeSelf;
    
    public Action<UIModularDialogueAnswer> OnAnswerReturn { get; set; }
    
    
    public Action OnClose { get; set; }
    
    public void Close() {
        windowRoot.SetActive(false);
    }

    public void CloseButton() {
        OnClose?.Invoke();
        Close();
    }
    
    public void Show() {
        windowRoot.SetActive(true);
    }

    public void Show(string title) {
        windowTitle.text = title;
        Show();
    }

    public void ReturnAnswer(UIModularDialogueAnswer answer) {
        OnAnswerReturn?.Invoke(answer);
        Close();

        OnAnswerReturn = null;
    }

    public void ReturnYes() => ReturnAnswer(UIModularDialogueAnswer.Yes);
    public void ReturnNo() => ReturnAnswer(UIModularDialogueAnswer.No);
    public void ReturnCancel() => ReturnAnswer(UIModularDialogueAnswer.Cancel);
    public void ReturnInformation() => ReturnAnswer(UIModularDialogueAnswer.Information);
    public void ReturnClose() => ReturnAnswer(UIModularDialogueAnswer.Close);
    public void ReturnOther() => ReturnAnswer(UIModularDialogueAnswer.Other);
}

public enum UIModularDialogueAnswer
{
    Yes,
    No,
    Cancel,
    Information,
    Close,
    Other
}