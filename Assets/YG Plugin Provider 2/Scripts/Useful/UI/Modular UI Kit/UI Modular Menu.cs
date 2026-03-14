using System.Linq;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

public class UIModularMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] menus;

    [Space]
    
    [SerializeField] private GameObject[] otherUIElements;

    public bool UIElementsEnabled {
        set {
            foreach (var x in otherUIElements) x.SetActive(value);
        }
    }

    private GameObject _oldMenu;

    private void Start() {
        EnableMenu(0);
    }
    
#if ODIN_INSPECTOR
    
    [Button]
    
#endif
    public void EnableMenu(int index) {
        if (!menus.InLength(index)) return;
        
        EnableMenu(menus[index]);
    }
    
    public void EnableMenu(GameObject menu) {
        if (!menu || !menus.Contains(menu)) return;
        
        foreach (var x in menus) {
            if (x.activeSelf) _oldMenu = x;
            
            x.SetActive(false);
        }
        
        menu.SetActive(true);
    }

    public void BackMenu() => EnableMenu(_oldMenu);
}