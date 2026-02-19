using UnityEngine;

public class MainGameMenu : MonoBehaviour
{
    [SerializeField] private MenuElement mainMenu;
    [SerializeField] private MenuElement gameMenu;

    [System.Serializable]
    private struct MenuElement
    {
        public GameObject window;
        public GameObject level;
    }

    public void Play() {
        mainMenu.level.SetActive(false);
        gameMenu.level.SetActive(true);
        YG_GameMenu.Instance.EnableWindow(gameMenu.window);
    }

    public void Menu() {
        gameMenu.level.SetActive(false);
        mainMenu.level.SetActive(true);
        YG_GameMenu.Instance.EnableWindow(mainMenu.window);
    }

    private void Start() {
        Menu();
    }
}