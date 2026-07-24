using UnityEngine;

public class ParticleObject : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private float _particleOn = 40f;
    [SerializeField] private ActiveTaskType _activeTaskType;

    private void Update()
    {
        if (_particle == null || ActiveManager.Instance == null)
        {
            return;
        }

        float progress = ActiveManager.Instance.GetSystemProgress(_activeTaskType);
        bool shouldPlay = progress < _particleOn;

        if (shouldPlay && !_particle.isPlaying)
        {
            _particle.Play();
        }
        else if (!shouldPlay && _particle.isPlaying)
        {
            _particle.Stop();
        }
    }
}
