using UnityEngine;

namespace CozyPolyomino
{
    public class GameEffects : MonoBehaviour
    {
        [SerializeField] private ParticleSystem groundEffectPrefab;

        private ParticleSystem _myGroundEffect;

        private static GameEffects _instance;

        internal static GameEffects Instance {
            get {
                if (!_instance) _instance = FindFirstObjectByType<GameEffects>(FindObjectsInactive.Include);

                return _instance;
            }
        }

        private void Awake() {
            Initialize();
        }
        
        private void Initialize() {
            _myGroundEffect = Instantiate(groundEffectPrefab, transform);
            _myGroundEffect.Stop();
        }

        internal static void Drop(DraggableShape shape, bool isCorrect) {
            if (Instance) Instance._Drop(shape, isCorrect);
        }

        private void _Drop(DraggableShape shape, bool isCorrect) {
            if (_myGroundEffect) {
                Vector3 pos = shape.Center;
                pos.y -= 0.1f;
                
                _myGroundEffect.transform.position = pos;
                _myGroundEffect.Play();
            }
        }
    }
}