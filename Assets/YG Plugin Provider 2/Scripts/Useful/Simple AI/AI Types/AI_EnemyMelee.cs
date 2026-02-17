using UnityEngine;

public class AI_EnemyMelee : AI_Controller
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

    private bool _canAttack = true;

    private AI_MeleeWeapon _myWeapon;
    private AI_MeleeWeapon _myWeapon2;

    private void Awake() {
        FindGetComponentsFeatures.TryFindObjectByType(out _target);

        if (_target) _targetTransform = _target.transform;
    }

    private void Start() {
        SetHandPoint(MainAI.handPoint, out _myWeapon);
        SetHandPoint(MainAI.handPoint2, out _myWeapon2);
    }

    private void SetHandPoint(Transform point, out AI_MeleeWeapon meleeWeapon) {
        if (!point) {
            meleeWeapon = null;
            return;
        }
        
        AI_MeleeWeapon weapon = Instantiate(Data.meleeWeapons.Choice(), point.position, point.rotation);
        Transform weaponTransform = weapon.transform;

        weaponTransform.SetParent(point);
        weaponTransform.localPosition = weapon.data.pos;
        weaponTransform.localRotation = Quaternion.Euler(weapon.data.rot);

        meleeWeapon = weapon;
    }

    private void Update() {
        targetDistance = Vector3.Distance(_transform.position, _targetTransform.position);

        MainAI.Run = targetDistance > Data.runDistance;

        if (targetDistance < Data.attackDistance)
            Attack();
        else
            MainAI.SetDestination(_targetTransform.position);
    }

    private void Attack() {
        if (!_canAttack) return;

        _canAttack = false;
        
        MainAI.Stopped = true;
        MainAI.ResetPath();

        MainAI.PlayVoice(data.attackVoiceType);
        
        Animator.SetFloat(Data.animatorAttackIdName, Random.Range(0, Data.animatorAttackClipsCount));
        Animator.SetTrigger(Data.animatorAttackTriggerName);
    }

    public void StopAttack() {
        _canAttack = true;
    }

    public void Damage() {
        if (_myWeapon)
            _myWeapon.Damage();
        if (_myWeapon2)
            _myWeapon2.Damage();
    }

    public override void OnDamaged() {
        MainAI.PlayVoice(data.damagedVoiceType);
    }
}