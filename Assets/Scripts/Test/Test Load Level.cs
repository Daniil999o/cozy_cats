using TMPro;
using UnityEngine;
using CozyPolyomino;

public class TestLoadLevel : MonoBehaviour
{
    [SerializeField] private TMP_InputField levelInput;

    public void OnChangeLevelInput(string text) {
        LevelSpawner.CurrentLevel = int.Parse(text);
    }
}