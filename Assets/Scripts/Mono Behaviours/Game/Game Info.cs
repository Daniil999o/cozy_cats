using TMPro;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text goldInfo;
    [SerializeField] private TMP_Text brilliantInfo;

    private int _currentGold;
    private int _currentBrilliant;

    public void ResetAll() {
        _currentGold = 0;
        _currentBrilliant = 0;
    }
    
    public void SetGolds(int value) => _currentGold = value;
    public void SetBrilliants(int value) => _currentBrilliant = value;

    public void ShowWinInfo() {
        RoomShop.Coins += _currentGold;
        RoomShop.Megas += _currentBrilliant;

        goldInfo.text = $"{_currentGold} <sprite name=coin>";
        brilliantInfo.text = $"{_currentBrilliant} <sprite name=mega>";
    }
}