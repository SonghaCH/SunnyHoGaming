using System.ComponentModel;
using UnityEngine;

public class PlayerAnimationView : ViewBase
{
    [SerializeField] private Animator _animator;

    private PlayerMovementViewModel _movementViewModel;
    private PlayerStatusViewModel _statusViewModel;

    private void Start()
    {
        if (NetworkManager.Inst != null)
        {
            BindMovementViewModel(NetworkManager.Inst.PlayerService.GetMovementViewModel());
            BindStatusViewModel(NetworkManager.Inst.PlayerService.GetStatusViewModel());
        }
    }

    public void BindMovementViewModel(PlayerMovementViewModel viewModel)
    {
        _movementViewModel = viewModel;
        _movementViewModel.PropertyChanged += OnPropertyChanged_MovementView;
    }

    public void BindStatusViewModel(PlayerStatusViewModel viewModel)
    {
        _statusViewModel = viewModel;
        _statusViewModel.PropertyChanged += OnPropertyChanged_StatusView;
    }

    private void OnDestroy()
    {
        if (_movementViewModel != null)
        {
            _movementViewModel.PropertyChanged -= OnPropertyChanged_MovementView;
        }
        if (_statusViewModel != null)
        {
            _statusViewModel.PropertyChanged -= OnPropertyChanged_StatusView;
        }
    }

    private void OnPropertyChanged_MovementView(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerMovementViewModel.IsMoving) ||
            e.PropertyName == nameof(PlayerMovementViewModel.IsRunning))
        {
            UpdateMoveAnimation();
        }
    }

    private void OnPropertyChanged_StatusView(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerStatusViewModel.IsSleeping))
        {
            UpdateSleepAnimation();
        }
    }

    private void UpdateMoveAnimation()
    {
        if (_movementViewModel == null || _animator == null)
        {
            return;
        }

        _animator.SetBool("isMoving", _movementViewModel.IsMoving);
        _animator.SetBool("isRunning", _movementViewModel.IsRunning);
    }


    private void UpdateSleepAnimation()
    {
        if(_statusViewModel == null || _animator == null)
        {
            return;
        }

        _animator.SetBool("isSleeping", _statusViewModel.IsSleeping);
    }
}