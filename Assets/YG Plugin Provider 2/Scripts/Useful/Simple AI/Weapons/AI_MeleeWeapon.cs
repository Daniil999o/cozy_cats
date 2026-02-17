using UnityEngine;

public class AI_MeleeWeapon : MonoBehaviour
{
    [Header("PlayerData")]
    public WeaponData data;

    [Space]

    [Header("Point")]
    [SerializeField] private AI_HitPoint point;
    
    [System.Serializable]
    public class WeaponData
    {
        [Header("In Hand")]
        public Vector3 pos;
        public Vector3 rot;

        [Space]

        [Header("Settings")]
        public float damage = 10f;
    }

    public void Damage() => point.CheckCollisions();
}