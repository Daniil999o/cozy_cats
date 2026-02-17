using UnityEngine;

public class PlayerInfo : AI_Damageable
{
    [Space]

    [Header("UI")]
    [SerializeField] private GameObject dieWindow;
    [SerializeField] private YG_SlicedSlider healthSlider;

    [Space]

    [Header("Center")]
    [SerializeField] private Transform center;

    public Transform Center => center;

    protected override void OnHealthUpdated() {
        healthSlider.Value = HealthDifference;
    }

    protected override void OnDie() {
        YG_GameMenu.Paused = true;
        YG_GameMenu.Instance.EnableWindow(dieWindow);
    }
}