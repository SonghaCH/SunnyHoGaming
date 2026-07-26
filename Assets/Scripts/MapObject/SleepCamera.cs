
using System.ComponentModel;
using UnityEngine;

public class SleepCamera : MonoBehaviour
{
    [SerializeField] private GameObject _mainCamera;
    private PlayerStatusViewModel _statusViewModel;


    public void BindStatusViewModel(PlayerStatusViewModel viewModel)
    {
        _statusViewModel = viewModel;
        _statusViewModel.PropertyChanged += OnPropertyChanged_View;
        _statusViewModel.InvokeOnceOnInit();
    }

    private void Start()
    {
        _mainCamera.SetActive(true);
        gameObject.SetActive(false);

        if (NetworkManager.Inst != null)
        {
            BindStatusViewModel(NetworkManager.Inst.PlayerService.GetStatusViewModel());
        }
    }

    private void OnDestroy()
    {
        if (_statusViewModel != null)
        {
            _statusViewModel.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerStatusViewModel.IsSleeping))
        {
            if (_statusViewModel.IsSleeping)
            {
                ActiveSleepCamera();
            }
            else
            {
                ActiveMainCamera();
            }
        }
    }

    private void ActiveSleepCamera()
    {
        gameObject.SetActive(true);
        _mainCamera.SetActive(false);
    }

    private void ActiveMainCamera()
    {
        _mainCamera.SetActive(true);
        gameObject.SetActive(false);
    }

}
