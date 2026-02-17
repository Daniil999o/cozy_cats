using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleAI : MonoBehaviour
{
    [Header("AI Type")]
    [SerializeField] private AI_Type aiType;

    public AI_Type AIType {
        get => aiType;
        set {
            aiType = value;
            
            foreach (var x in GetComponents<AI_Controller>())
                Destroy(x);

            switch (value) {
                case AI_Type.EnemyMelee:
                    gameObject.AddComponent<AI_EnemyMelee>().Data = enemyData;
                    break;
                case AI_Type.EnemyRifle:
                    gameObject.AddComponent<AI_EnemyRifle>().Data = enemyData;
                    break;
            }
            
            GetComponentInChildren<AI_AnimatorCaller>().UpdateController();
        }
    }

    [Space]

    [Header("Agent Parameters")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    [Space]

    [Header("Animator")]
    [SerializeField] private Animator animator;
    public Animator Animator => animator;
    [SerializeField] private string animatorSpeedName = "Speed";

    [Space]

    [Header("Hand")]
    public Transform handPoint;
    public Transform handPoint2;

    [Space]

    [Header("Datas")]
    [SerializeField] private AI_Enemy_Data enemyData;

    [Space]

    [Header("Voice")]
    [SerializeField] private AI_VoiceController voiceController;

    public void PlayVoice(string type) {
        if (!voiceController) return;
        
        voiceController.PlayVoice(type);
    }
    
    private NavMeshAgent _agent;

    private bool _run;
    public bool Run {
        get => _run;
        set => _run = value;
    }
    
    private bool _stopped;
    public bool Stopped {
        get => _stopped;
        set => _stopped = value;
    }

    private Vector3 _oldDestination;

    public void SetDestination(Vector3 pos) {
        if (_agent.hasPath && _oldDestination == pos) return;

        _oldDestination = pos;
        _agent.SetDestination(pos);
    }

    public void ResetPath() => _oldDestination = Vector3.zero;

    private void Awake() {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start() {
        AIType = aiType;
    }

    private void LateUpdate() {
        UpdateValues();
        UpdateAnimator();
    }

    private void UpdateValues() {
        _agent.speed = Mathf.Lerp(_agent.speed, Run ? runSpeed : walkSpeed, 1f * Time.deltaTime);
        _agent.isStopped = Stopped;
    }

    private void UpdateAnimator() {
        Animator.SetFloat(animatorSpeedName, _agent.velocity.magnitude);
    }
}

public enum AI_Type
{
    EnemyMelee,
    EnemyRifle
}