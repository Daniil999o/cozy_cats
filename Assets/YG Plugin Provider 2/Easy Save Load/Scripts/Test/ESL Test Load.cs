using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EasySaveLoad.Test
{
    public class ESL_TestLoad : MonoBehaviour
    {
        [SerializeField] private Transform saveTarget;
        
        private static string _save;

        private void Start() {
            print($"Save: {_save}");
            if (_save != null)
                SaveLoadHandler.Instance.Load(_save);
        }
        
        private void Update() {
            if (Keyboard.current.wKey.isPressed)
                saveTarget.Translate(Vector3.up * 4f * Time.deltaTime);
            else if (Keyboard.current.sKey.isPressed)
                saveTarget.Translate(Vector3.down * 4f * Time.deltaTime);
            
            if (Keyboard.current.aKey.isPressed)
                saveTarget.Rotate(Vector3.right * 40f * Time.deltaTime);
            else if (Keyboard.current.dKey.isPressed)
                saveTarget.Rotate(Vector3.left * 40f * Time.deltaTime);
            
            if (Keyboard.current.eKey.isPressed)
                saveTarget.localScale += new Vector3(1f, 1f, 1f) * 2f * Time.deltaTime;
            else if (Keyboard.current.qKey.isPressed)
                saveTarget.localScale -= new Vector3(1f, 1f, 1f) * 2f * Time.deltaTime;

            if (!Keyboard.current.f5Key.wasPressedThisFrame) return;
            
            _save = SaveLoadHandler.Instance.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}