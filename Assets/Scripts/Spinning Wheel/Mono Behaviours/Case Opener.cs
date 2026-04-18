using AutoLocalization;
using TMPro;
using UnityEditor;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class CaseOpener : MonoBehaviour
{
    [SerializeField] private Animator caseAnimator;
    
    [Space]
    
    [SerializeField] private GameObject[] showWhenSpinning;
    [SerializeField] private GameObject[] hideWhenSpinning;

    [Space]

    [SerializeField] private SpinningWheel fortuneWheel;

    [Space]

    [SerializeField] private Sprite fishSprite;

    [Space]
    
    [SerializeField] private CustomMaterialsSO[] customMaterials;
    [SerializeField] private CustomSkinsSO[] customSkins;
    [SerializeField] private ShopItemsSO[] shopItems;

    [Space]

    [Header("Won")]
    [SerializeField] private GameObject rewardWindow;
    [SerializeField] private GameObject fishWindow;
    [SerializeField] private GameObject anotherWindow;
    [SerializeField] private TMP_Text fishPriceInfo;
    [SerializeField] private Image anotherImage;
    [SerializeField] private TMP_Text anotherNameInfo;
    [SerializeField] private TMP_Text anotherPriceInfo;

    [Button("Load all resources")]
    private void LoadAllResources() {
        customMaterials = AssetDatabase.FindAssets("t:CustomMaterialsSO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<CustomMaterialsSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .OrderBy(s => s.name)
            .ToArray();
        
        customSkins = AssetDatabase.FindAssets("t:CustomSkinsSO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<CustomSkinsSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .OrderBy(s => s.name)
            .ToArray();
        
        shopItems = AssetDatabase.FindAssets("t:ShopItemsSO")
            .Select(guid => AssetDatabase.LoadAssetAtPath<ShopItemsSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .OrderBy(s => s.name)
            .ToArray();
    }

    private void OnEnable() {
        foreach (var x in showWhenSpinning) x.SetActive(false);
        foreach (var x in hideWhenSpinning) x.SetActive(true);
        
        rewardWindow.SetActive(false);
    }
    
    public void OpenCase() {
        if (RoomShop.Megas < 1) return;
        
        RoomShop.Megas -= 1;
        
        foreach (var x in showWhenSpinning) x.SetActive(true);
        foreach (var x in hideWhenSpinning) x.SetActive(false);
        
        caseAnimator.SetTrigger("Open");

        GenerateRewards();
    }

    public void StopCase(RewardData reward) {
        rewardWindow.SetActive(true);
        
        fishWindow.SetActive(reward.isFish);
        anotherWindow.SetActive(!reward.isFish);

        fishPriceInfo.text = $"{reward.count} <sprite name=coin>";

        anotherImage.sprite = reward.preview;

        string rewardName = "???";

        // if (reward.material)
        //     rewardName = AutoLocalizationHandler.GetLocalizedText(reward.material.id);
        // else if (reward.skin)
        //     rewardName = AutoLocalizationHandler.GetLocalizedText(reward.skin.id);
        // else if (reward.item)
        //     rewardName = AutoLocalizationHandler.GetLocalizedText(reward.item.id);

        anotherNameInfo.text = rewardName;
        anotherPriceInfo.text = $"{reward.price} <sprite name=coin>";

        if (reward.material) RoomShop.BuyCustomMaterial(reward.material.id);
        else if (reward.skin) RoomShop.BuyCustomSkin(reward.skin.id);
        else if (reward.item) RoomObjectsLoader.Instance.SpawnObject(reward.item);

        if (reward.isFish) RoomShop.Coins += reward.count;
    }

    public void CloseReward() {
        rewardWindow.SetActive(false);
        caseAnimator.SetTrigger("Idle");
        
        foreach (var x in showWhenSpinning) x.SetActive(false);
        foreach (var x in hideWhenSpinning) x.SetActive(true);
    }

    private void GenerateRewards() {
        List<RewardData> rewards = new List<RewardData>();
        
        for (int i = 0; i < fortuneWheel.NeedRewardsCount; i++) {
            float chance = Random.Range(0f, 1f);

            RewardRare rare = chance switch {
                < 0.06f => RewardRare.Legendary,
                < 0.11f => RewardRare.Epic,
                < 0.36f => RewardRare.Rare,
                var _ => RewardRare.Simple
            };

            RewardData reward = GetReward(rare);
            
            rewards.Add(reward);
        }

        if (!rewards.Any(x => x.rare is RewardRare.Epic or RewardRare.Legendary)) {
            float chance = Random.Range(0f, 1f);

            RewardRare rare = chance switch {
                < 0.15f => RewardRare.Legendary,
                var _ => RewardRare.Epic
            };

            RewardData reward = GetReward(rare);

            rewards[0] = reward;
        }

        foreach (RewardData x in rewards) {
            Sprite preview = fishSprite;
            int price = 0;

            if (x.material) {
                preview = x.material.Preview;
                price = x.material.price;
            } else if (x.skin) {
                preview = x.skin.Preview;
                price = x.skin.price;
            } else if (x.item) {
                preview = x.item.Preview;
                price = x.item.price;
            } else {
                x.isFish = true;
            }

            x.preview = preview;
            x.price = price;
        }
        
        fortuneWheel.SetRewards(rewards.ToArray());
    }

    private RewardData GetReward(RewardRare rare) {
        RewardData reward = new RewardData {
            rare = rare
        };

        bool isFish;

        switch (rare) {
            case RewardRare.Simple:
                isFish = Random2.Chance(0.6f);
                
                reward.count = Random2.Chance(0.1f) ? 20 : Random2.Chance(0.4f) ? 10 : 5;

                if (!isFish) reward = GetRewardWithItems(reward, 1, 20);
                
                break;
            case RewardRare.Rare:
                isFish = Random2.Chance(0.6f);
                
                reward.count = Random2.Chance(0.3f) ? 50 : Random2.Chance(0.3f) ? 30 : 15;

                if (!isFish) reward = GetRewardWithItems(reward, 21, 50);
                
                break;
            case RewardRare.Epic:
                isFish = Random2.Chance(0.4f);
                
                reward.count = Random2.Chance(0.2f) ? 150 : Random2.Chance(0.3f) ? 100 : 50;

                if (!isFish) reward = GetRewardWithItems(reward, 51, 200);
                
                break;
            case RewardRare.Legendary:
                isFish = Random2.Chance(0.2f);
                
                reward.count = Random2.Chance(0.1f) ? 600 : Random2.Chance(0.2f) ? 350 : 200;

                if (!isFish) reward = GetRewardWithItems(reward, 201, 10000);
                
                break;
        }
        
        return reward;
    }

    private RewardData GetRewardWithItems(RewardData data, int minPrice, int maxPrice) {
        int[] availableTypes = {
            0, 1, 2
        };
        
        availableTypes = availableTypes.Shuffle();

        foreach (int type in availableTypes) {
            bool success = false;

            switch (type) {
                case 0:
                    CustomMaterialSO mat = GetCustomMaterial(minPrice, maxPrice);
                    if (mat) {
                        data.material = mat;
                        success = true;
                    }

                    break;
                case 1:
                    CustomSkinSO skin = GetCustomSkin(minPrice, maxPrice);
                    if (skin) {
                        data.skin = skin;
                        success = true;
                    }

                    break;
                case 2:
                    ShopItemSO shopItem = GetCustomShopItem(minPrice, maxPrice);
                    if (shopItem) {
                        data.item = shopItem;
                        success = true;
                    }

                    break;
            }

            if (success) break;
        }

        return data;
    }

    private CustomMaterialSO GetCustomMaterial(int minPrice, int maxPrice) {
        CustomMaterialSO[] myMats = customMaterials.SelectMany(x => x.All)
            .Where(x => x.price >= minPrice && x.price <= maxPrice && !RoomShop.HasCustomMaterial(x.id)).ToArray();

        return myMats.Length > 0 ? myMats.Choice() : null;
    }
    
    private CustomSkinSO GetCustomSkin(int minPrice, int maxPrice) {
        CustomSkinSO[] mySkins = customSkins.SelectMany(x => x.All)
            .Where(x => x.price >= minPrice && x.price <= maxPrice && !RoomShop.HasCustomSkin(x.id)).ToArray();

        return mySkins.Length > 0 ? mySkins.Choice() : null;
    }
    
    private ShopItemSO GetCustomShopItem(int minPrice, int maxPrice) {
        ShopItemSO[] myShopItems = shopItems.SelectMany(x => x.All)
            .Where(x => x.price >= minPrice && x.price <= maxPrice).ToArray();

        return myShopItems.Length > 0 ? myShopItems.Choice() : null;
    }
}

public enum RewardRare
{
    Simple,
    Rare,
    Epic,
    Legendary
}

public class RewardData
{
    public RewardRare rare;
    public Sprite preview;

    [Space]

    public bool isFish;
    public int count;

    [Space]

    public int price;
    public CustomMaterialSO material;
    public CustomSkinSO skin;
    public ShopItemSO item;
}