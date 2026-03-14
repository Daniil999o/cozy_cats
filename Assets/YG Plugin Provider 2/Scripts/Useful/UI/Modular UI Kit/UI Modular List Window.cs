using TMPro;
using System;
using System.Linq;
using UnityEngine;

public class UIModularListWindow : MonoBehaviour, IUIModalWindow
{
    [SerializeField] private GameObject windowRoot;
    
    [Space]
    
    [SerializeField] private TMP_Text windowTitleInfo;

    [Space]

    [SerializeField] private Transform listContent;
    [SerializeField] private UIModularListElement elementPrefab;

    private UIModularListElementInfo[] _oldInfos;
    
    private void Start() {
        Close();
    }

    public void Show(UIModularListElementInfo[] infos) {
        windowRoot.SetActive(true);
        UpdateList(infos);
    }

    public void Show() {
        Show(_oldInfos);
    }

    public void UpdateList() => UpdateList(_oldInfos);

    public bool Active => windowRoot.activeSelf;
    
    public Action OnClose { get; set; }

    public void CloseButton() {
        OnClose?.Invoke();
        Close();
    }
    
    public void Close() {
        ClearList();
        windowRoot.SetActive(false);
    }

    private void UpdateList(UIModularListElementInfo[] infos) {
        while (listContent.childCount > 0)
            DestroyImmediate(listContent.GetChild(0).gameObject);

        infos = infos.OrderBy(x => x.Price).ToArray();
        
        foreach (var x in infos)
            SpawnListElement(x);
        
        _oldInfos = infos;
    }

    public void ClearList() {
        while (listContent.childCount > 0)
            DestroyImmediate(listContent.GetChild(0).gameObject);
    }

    private void SpawnListElement(UIModularListElementInfo info) {
        UIModularListElement element = Instantiate(elementPrefab, listContent);
        
        element.SetInfo(info);
    }
}