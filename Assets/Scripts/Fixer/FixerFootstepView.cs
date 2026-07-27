using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FixerFootstepView : MonoBehaviour
{
    [SerializeField] private string _footstepSoundPath = "Sound/WalkFixer";
    [SerializeField] private float _stepInterval = 0.5f;
    [SerializeField] private float _moveThreshold = 0.1f;

    [SerializeField] private float _minDistance = 0.5f;
    [SerializeField] private float _maxDistance = 8.0f;

    private NavMeshAgent _agent;
    private AudioSource _audioSource;
    private float _stepTimer = 0.0f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1.0f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
        _audioSource.minDistance = _minDistance;
        _audioSource.maxDistance = _maxDistance;
    }

    private void Update()
    {
        bool isMoving = _agent.velocity.magnitude > _moveThreshold;

        if (!isMoving)
        {
            _stepTimer = 0.0f;
            return;
        }

        _stepTimer += Time.deltaTime;
        if (_stepTimer >= _stepInterval)
        {
            _stepTimer = 0.0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        AudioManager.Instance.PlaySFX(_audioSource, _footstepSoundPath);
    }
}