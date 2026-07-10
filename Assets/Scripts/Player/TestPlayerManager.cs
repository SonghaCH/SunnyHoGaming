using UnityEngine;

public class TestPlayerManager : MonoBehaviour
{
    [SerializeField]
    private PlayerMovementView _playerMovementView;

    private PlayerMovementViewModel _playerMovementViewModel;

    private void Start()
    {
        if (_playerMovementView != null)
        {
            _playerMovementViewModel = new PlayerMovementViewModel();
            _playerMovementView.BindViewModel(_playerMovementViewModel);
        }
        else
        {
            Debug.LogError("PlayerMovementView is missing.");
        }
    }
}