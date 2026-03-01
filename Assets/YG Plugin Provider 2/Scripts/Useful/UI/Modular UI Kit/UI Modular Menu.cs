using System.Linq;
using UnityEngine;

public class UIModularMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] menus;

    private GameObject _oldMenu;

    private void Start() {
        EnableMenu(0);
    }

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