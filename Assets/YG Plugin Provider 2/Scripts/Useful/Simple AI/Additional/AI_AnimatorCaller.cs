using UnityEngine;

public class AI_AnimatorCaller : MonoBehaviour
{
    private SimpleAI _ai;
    private AI_EnemyMelee _enemy;

    private void Awake() {
        _ai = GetComponentInParent<SimpleAI>();
    }

    public void UpdateController() {
        _enemy = GetComponentInParent<AI_EnemyMelee>();
    }

    public void AIStartMove() {
        if (_ai) _ai.Stopped = false;
    }
    
    public void AIStopMove() {
        if (_ai) _ai.Stopped = true;
    }

    public void EnemyStopAttack() {
        if (_enemy) _enemy.StopAttack();

        AIStartMove();
    }

    public void EnemyDamage() {
        if (_enemy) _enemy.Damage();
    }
}