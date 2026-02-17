using UnityEngine;

public class AI_EnemyRifle : AI_Controller
{
    [Header("PlayerData")]
    [SerializeField] private AI_Enemy_Data data;

    public AI_Enemy_Data Data {
        private get => data;
        set => data = value;
    }
    
    [Header("Information")]
    [SerializeField] private float targetDistance;
    
    private AI_Target _target;
    private Transform _targetTransform;

    private bool _canShoot = true;
    public bool CanShoot {
        private get => _canShoot;
        set => _canShoot = value;
    }

    private bool _noShoot;
    public bool NoShoot {
        private get => _noShoot;
        set => _noShoot = value;
    }

    private AI_RifleWeapon _myWeapon;

    private void Awake() {
        FindGetComponentsFeatures.TryFindObjectByType(out _target);

        if (_target) _targetTransform = _target.transform;
    }

    private void Start() {
        AI_RifleWeapon weapon = Instantiate(Data.rifleWeapons.Choice(), MainAI.handPoint.position,
            MainAI.handPoint.rotation
        );
        Transform weaponTransform = weapon.transform;

        weaponTransform.SetParent(MainAI.handPoint);
        weaponTransform.localPosition = weapon.data.pos;
        weaponTransform.localRotation = Quaternion.Euler(weapon.data.rot);

        _myWeapon = weapon;
    }

    private void Update() {
        targetDistance = Vector3.Distance(_transform.position, _targetTransform.position);

        MainAI.Run = targetDistance > Data.runDistance;

        if (!NoShoot && targetDistance < Data.attackDistance)
            Shoot();
        else
            MainAI.SetDestination(_targetTransform.position);
    }

    private void Shoot() {
        if (!CanShoot) return;
        
        CanShoot = false;
        
        MainAI.Stopped = true;
        MainAI.ResetPath();
        
        _myWeapon.Shoot(this);
    }
    
    public void StartMove() => MainAI.Stopped = false;

    public void PlayShootAnimation() => Animator.SetTrigger(Data.animatorAttackTriggerName);
    public void PlayDefaultAnimation() => Animator.SetTrigger("Default");

    public override void OnDamaged() {
        MainAI.PlayVoice(data.damagedVoiceType);
    }
}