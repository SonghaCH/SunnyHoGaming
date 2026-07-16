using System.ComponentModel;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementView : ViewBase
{
    [SerializeField]
    private Transform _cameraTransform;

    private CharacterController _characterController;
    private PlayerMovementViewModel _viewModel;
    private float _xRotation = 0.0f;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }
    private void Start()
    {
        if (NetworkManager.Inst != null)
        {
            BindViewModel(NetworkManager.Inst.PlayerService.GetMovementViewModel());
        }
    }

    public void BindViewModel(PlayerMovementViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged_View;
        _viewModel.InvokeOnceOnInit();
    }

    private void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        // 이동 관련 UI 추가 시 사용 예정
    }

    private void Update()
    {
        if (_viewModel == null)
        {
            return;
        }

        if (!_viewModel.CanMove)
        {
            return;
        }

        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        Move(inputX, inputZ);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Look(mouseX, mouseY);
    }

    public void Move(float moveX, float moveZ)
    {
        Vector3 rightDirection = transform.right * moveX;
        Vector3 forwardDirection = transform.forward * moveZ;
        Vector3 moveDirection = rightDirection + forwardDirection;

        float frameSpeed = _viewModel.CurrentSpeed * Time.deltaTime;
        _characterController.Move(moveDirection * frameSpeed);
    }

    public void Look(float mouseX, float mouseY)
    {
        float lookX = mouseX * _viewModel.MouseSensitivity;
        float lookY = mouseY * _viewModel.MouseSensitivity;

        _xRotation -= lookY;
        _xRotation = Mathf.Clamp(_xRotation, -90.0f, 90.0f);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * lookX);
    }
}