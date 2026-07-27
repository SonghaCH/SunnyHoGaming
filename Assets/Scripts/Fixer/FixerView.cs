using UnityEngine;
[RequireComponent(typeof(Animator), typeof(FixerViewModel))]
public class FixerView : MonoBehaviour
{
    [SerializeField] private string _repairSoundPath = "Sound/Repair";
    [SerializeField] private float _minDistance = 1.0f;
    [SerializeField] private float _maxDistance = 8.0f;
    private Animator _animator;
    private FixerViewModel _viewModel;
    private AudioSource _repairAudioSource;

    private void Awake()
    {
        bool hasAnimator = TryGetComponent(out _animator);
        if (hasAnimator == false)
        {
            Debug.LogError($"[FixerView] Animator 컴포넌트가 없습니다.");
        }
        bool hasViewModel = TryGetComponent(out _viewModel);
        if (hasViewModel == false)
        {
            Debug.LogError($"[FixerView] FixerViewModel 컴포넌트가 없습니다.");
        }
        InitRepairAudioSource();
    }

    private void OnEnable()
    {
        if (_viewModel != null)
        {
            _viewModel.OnAnimationStateChanged += PlayStateAnimation;
        }
        if (AudioController.Instance != null)
        {
            AudioController.Instance.OnSFXVolumeChanged += OnSFXVolumeChanged;
        }
    }

    private void OnDisable()
    {
        if (_viewModel != null)
        {
            _viewModel.OnAnimationStateChanged -= PlayStateAnimation;
        }
        if (AudioController.Instance != null)
        {
            AudioController.Instance.OnSFXVolumeChanged -= OnSFXVolumeChanged;
        }
    }

    private void OnSFXVolumeChanged(float volume)
    {
        if (_repairAudioSource != null)
        {
            _repairAudioSource.volume = volume;
        }
    }

    private void InitRepairAudioSource()
    {
        _repairAudioSource = gameObject.AddComponent<AudioSource>();
        _repairAudioSource.playOnAwake = false;
        _repairAudioSource.spatialBlend = 1.0f;
        _repairAudioSource.rolloffMode = AudioRolloffMode.Linear;
        _repairAudioSource.minDistance = _minDistance;
        _repairAudioSource.maxDistance = _maxDistance;
    }

    private void PlayStateAnimation(FixerState state)
    {
        if (_animator == null)
        {
            return;
        }
        switch (state)
        {
            case FixerState.Idle:
                _animator.CrossFade("Idle", 0.1f);
                break;
            case FixerState.Rampaging:
                _animator.CrossFade("CrashRun", 0.1f);
                break;
            case FixerState.Executing:
                _animator.CrossFade("Fix", 0.1f);
                AudioManager.Instance.PlaySFX(_repairAudioSource, _repairSoundPath, isLoop: true);
                break;
            case FixerState.Returning:
                _animator.Play("Run");
                _repairAudioSource.Stop();
                break;
            case FixerState.Wandering:
                _animator.CrossFade("Walk", 0.1f);
                break;
            case FixerState.MoveToTarget:
                _animator.CrossFade("Run", 0.1f);
                break;
        }
    }
}