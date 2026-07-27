using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFootstepView : ViewBase
{
    [SerializeField] private string _footstepSoundPath = "Sound/WalkPlayer";

    [SerializeField] private float _walkStepInterval = 0.5f;
    [SerializeField] private float _runStepInterval = 0.3f;

    private PlayerMovementViewModel _movementViewModel;
    private float _stepTimer = 0.0f;

    private void Start()
    {
        if (NetworkManager.Inst != null)
        {
            BindMovementViewModel(NetworkManager.Inst.PlayerService.GetMovementViewModel());
        }
    }

    public void BindMovementViewModel(PlayerMovementViewModel viewModel)
    {
        _movementViewModel = viewModel;
    }

    private void Update()
    {
        if (_movementViewModel == null)
        {
            return;
        }

        if (!_movementViewModel.IsMoving || !_movementViewModel.CanMove)
        {
            _stepTimer = 0.0f;
            return;
        }

        float interval = _movementViewModel.IsRunning ? _runStepInterval : _walkStepInterval;

        _stepTimer += Time.deltaTime;
        if (_stepTimer >= interval)
        {
            _stepTimer = 0.0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        AudioManager.Instance.PlaySFX(_footstepSoundPath);
    }
}