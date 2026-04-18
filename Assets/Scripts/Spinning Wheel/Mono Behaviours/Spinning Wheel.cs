using UnityEngine;

public class SpinningWheel : MonoBehaviour
{
    [SerializeField] private CaseOpener caseOpener;

    [Space]

    [SerializeField] private FortuneWheelOptions options;

    [Space]

    [SerializeField] private GameObject speedUpButton;

    [Space]

    [SerializeField] private Transform spinningWheel;
    [SerializeField] private SpinPiece[] slices;

    [System.Serializable]
    private class FortuneWheelOptions
    {
        public float startRotateSpeedBase = 900f;
        public float upAccelModifierBase = 300f;
        public float downAccelModifierBase = 100f;

        [Space]

        public float randomSpeedRange = 300f;
        public float randomAccelRange = 200f;
        public float randomDecelRange = 150f;

        [Space]
        
        public bool enableBounce = true;
        public float bounceThresholdSpeed = 40f;
        public float bounceStrengthMin = 10f;
        public float bounceStrengthMax = 40f;
        public float bounceChancePerSecond = 0.3f;
    }
    
    [Space]
    
    [Header("Audio")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioSource fortuneSource;
    
    private int _lastHighlightedIndex = -1;

    private float _currentStartRotateSpeed;
    private float _currentUpAccel;
    private float _currentDownAccel;
    private float _timeSinceLastBounceCheck;

    private bool _spinning;
    private bool _moveDown;
    private float _currentSpeed;

    public int NeedRewardsCount => slices.Length;

    public void SetRewards(RewardData[] rewards) {
        if (rewards.Length < NeedRewardsCount) return;

        rewards = rewards.Shuffle();
        
        for (int i = 0; i < slices.Length; i++) {
            slices[i].SetData(rewards[i]);
        }
    }

    private void Update() {
        if (!_spinning) return;

        float accel = _moveDown ? -_currentDownAccel : _currentUpAccel;
        _currentSpeed += accel * 0.6f * Time.deltaTime;

        if (!_moveDown && _currentSpeed > _currentStartRotateSpeed)
            _moveDown = true;

        if (options.enableBounce && _moveDown && _currentSpeed > 0f
            && _currentSpeed < options.bounceThresholdSpeed) {
            _timeSinceLastBounceCheck += Time.deltaTime;
            
            if (_timeSinceLastBounceCheck >= 1f / options.bounceChancePerSecond) {
                _timeSinceLastBounceCheck = 0f;

                float bounce = Random.Range(options.bounceStrengthMin, options.bounceStrengthMax);

                _currentSpeed += bounce;
                _currentSpeed = Mathf.Min(_currentSpeed, _currentStartRotateSpeed * 0.5f);
            }
        } else {
            _timeSinceLastBounceCheck = 0f;
        }

        spinningWheel.Rotate(Vector3.forward * _currentSpeed * Time.deltaTime);
        
        int currentIndex = GetCurrentSliceIndex();
        
        if (currentIndex != _lastHighlightedIndex) {
            PlayClick();
            _lastHighlightedIndex = currentIndex;
        }

        if (!_moveDown) return;

        if (_currentSpeed <= 0f)
            StopSpin();
    }
    
    private int GetCurrentSliceIndex() {
        if (slices == null || slices.Length == 0) return -1;

        int numberOfSlices = slices.Length;
        float anglePerSlice = 360f / numberOfSlices;
        float currentAngle = spinningWheel.eulerAngles.z;
        
        currentAngle = (currentAngle % 360 + 360) % 360;
        
        float adjustedAngle = 360f - currentAngle;
        
        if (adjustedAngle >= 360f) adjustedAngle = 0f;

        int index = Mathf.FloorToInt(adjustedAngle / anglePerSlice);
        
        return Mathf.Clamp(index, 0, numberOfSlices - 1);
    }
    
    private void PlayClick() {
        if (!clickClip || !fortuneSource) return;
        
        fortuneSource.PlayOneShot(clickClip);
    }

    public void SpeedUp() {
        speedUpButton.SetActive(false);

        Time.timeScale = 2f;
    }
    
    public void Spin() {
        Time.timeScale = 1f;
        
        speedUpButton.SetActive(true);
        
        _spinning = true;
        _moveDown = false;
        _currentSpeed = 0f;

        _currentStartRotateSpeed = options.startRotateSpeedBase + options.randomSpeedRange.RandomRange();
        
        _currentUpAccel = options.upAccelModifierBase + options.randomAccelRange.RandomRange();
        _currentDownAccel = options.downAccelModifierBase + options.randomDecelRange.RandomRange();

        _currentStartRotateSpeed = Mathf.Max(100f, _currentStartRotateSpeed);
        _currentUpAccel = Mathf.Max(50f, _currentUpAccel);
        _currentDownAccel = Mathf.Max(20f, _currentDownAccel);

        _timeSinceLastBounceCheck = 0f;

        Vector3 rot = spinningWheel.localEulerAngles;
        rot.z = 0f;
        
        spinningWheel.localRotation = Quaternion.Euler(rot);
        
        _lastHighlightedIndex = GetCurrentSliceIndex();
    }

    private void StopSpin() {
        Time.timeScale = 1f;
        
        _spinning = false;
        speedUpButton.SetActive(false);

        SpinPiece winningPiece = GetWinningPiece();

        RewardData reward = winningPiece.Reward;
        
        caseOpener.StopCase(reward);
    }
    
    private SpinPiece GetWinningPiece() {
        if (slices is not { Length: > 0 }) return null;

        int numberOfSlices = slices.Length;
        float anglePerSlice = 360f / numberOfSlices;

        float currentAngle = spinningWheel.eulerAngles.z;

        currentAngle = (currentAngle % 360 + 360) % 360;

        float adjustedAngle = 360f - currentAngle;
        
        if (adjustedAngle >= 360f)
            adjustedAngle = 0f;

        int winningIndex = Mathf.FloorToInt(adjustedAngle / anglePerSlice);

        winningIndex = Mathf.Clamp(winningIndex, 0, numberOfSlices - 1);
        
        return slices[winningIndex];
    }
}