using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpinPiece : MonoBehaviour
{
    [SerializeField] private Image bgImage;
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text costInfo;

    private static readonly Dictionary<RewardRare, Color> RewardColorsByRare = new Dictionary<RewardRare, Color> {
        [RewardRare.Simple] = Color.lightSkyBlue,
        [RewardRare.Rare] = Color.royalBlue,
        [RewardRare.Epic] = Color.rebeccaPurple,
        [RewardRare.Legendary] = Color.yellowNice
    };

    public RewardData Reward => _data;
    
    private RewardData _data;
    
    public void SetData(RewardData data) {
        _data = data;

        bgImage.color = RewardColorsByRare[data.rare];
        previewImage.sprite = data.preview;

        costInfo.text = data.isFish ? $"{data.count}" : $"{data.price} <sprite name=coin>";
    }
}