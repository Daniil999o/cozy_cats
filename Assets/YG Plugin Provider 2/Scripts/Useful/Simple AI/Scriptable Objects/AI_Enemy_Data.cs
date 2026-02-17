using UnityEngine;

[CreateAssetMenu(fileName = "AI Enemy PlayerData", menuName = "Scriptable Objects/AI Enemy PlayerData", order = 0)]
public class AI_Enemy_Data : ScriptableObject
{
    [Header("Distances")]
    public float attackDistance = 3f;
    public float runDistance = 6f;

    [Space]

    [Header("Animator")]
    public string animatorAttackTriggerName = "Attack";
    public string animatorAttackIdName = "Attack Id";
    public int animatorAttackClipsCount = 3;

    [Space]

    [Header("Weapons")]
    public AI_MeleeWeapon[] meleeWeapons;
    public AI_RifleWeapon[] rifleWeapons;

    [Space]

    [Header("Voice")]
    public string attackVoiceType = "Attack";
    public string damagedVoiceType = "Damaged";
}