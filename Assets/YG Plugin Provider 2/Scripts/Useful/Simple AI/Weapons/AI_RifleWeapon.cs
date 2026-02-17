using System.Collections;
using UnityEngine;

public class AI_RifleWeapon : MonoBehaviour
{
    [Header("PlayerData")]
    public WeaponData data;

    [Space]

    [Header("Point")]
    [SerializeField] private Transform point;
    
    [System.Serializable]
    public class WeaponData
    {
        [Header("In Hand")]
        public Vector3 pos;
        public Vector3 rot;

        [Space]

        [Header("Settings")]
        public float damage = 10f;
        public float bulletImpulse = 1f;
        public Vector2 ratio = new Vector2(-1f, 1f);
        public Vector2Int onceShootCount = new Vector2Int(3, 5);
        public float onceShootDelay = 0.2f;
        public float shootDelay = 1f;
        public AI_Bullet bullet;
    }

    [Space]

    [Header("Particles")]
    [SerializeField] private ParticleSystem muzzleParticle;

    [Space]

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] fireClips;

    private AI_EnemyRifle _enemy;
    private PlayerInfo _player;

    private void Awake() {
        _player = FindFirstObjectByType<PlayerInfo>(FindObjectsInactive.Include);
    }

    public void Shoot(AI_EnemyRifle enemy) {
        _enemy = enemy;
        StartCoroutine(IE_Shoot());
    }

    private IEnumerator IE_Shoot() {
        for (int i = Random.Range(data.onceShootCount.x, data.onceShootCount.y) + 1; i > 0; i--) {
            _enemy.PlayShootAnimation();
            ShootOnce();
            yield return new WaitForSeconds(data.onceShootDelay);
        }
        
        _enemy.PlayDefaultAnimation();
        
        _enemy.NoShoot = true;
        _enemy.StartMove();
        
        yield return new WaitForSeconds(data.shootDelay * Random.Range(0.8f, 1.1f));
        
        _enemy.NoShoot = false;
        _enemy.CanShoot = true;
    }

    private void ShootOnce() {
        point.LookAt(_player.Center);
        point.localRotation *= Quaternion.Euler(new Vector3(Random.Range(-data.ratio.x, data.ratio.x),
                Random.Range(-data.ratio.x, data.ratio.x), 0f
            ) * 2f
        );
        
        Instantiate(data.bullet, point.position, point.rotation).ApplyImpulse(data.damage, data.bulletImpulse);

        source.pitch = Random.Range(0.9f, 1.1f);
        source.PlayOneShot(fireClips.Choice());
        
        muzzleParticle.Play();
    }
}