using System.ComponentModel;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class PlayerMovementView : ViewBase
{
    [SerializeField] private Transform _cameraTransform;

    private Rigidbody _rigidbody;
    private PlayerMovementViewModel _viewModel;

    private float _xRotation = 0.0f;
    private float _inputX = 0.0f;
    private float _inputZ = 0.0f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
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
    }

    private void Update()
    {
        if (_viewModel == null)
        {
            return;
        }

        if (!_viewModel.CanMove)
        {
            _inputX = 0.0f;
            _inputZ = 0.0f;
            return;
        }

        _inputX = Input.GetAxisRaw("Horizontal");
        _inputZ = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _viewModel.IsRunning = true;
        }
        else
        {
            _viewModel.IsRunning = false;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Look(mouseX, mouseY);
    }

    private void FixedUpdate()
    {
        if (_viewModel == null)
        {
            return;
        }

        if (!_viewModel.CanMove)
        {
            return;
        }

        Move(_inputX, _inputZ);
    }

    public void Move(float moveX, float moveZ)
    {
        Vector3 rightDirection = transform.right * moveX;
        Vector3 forwardDirection = transform.forward * moveZ;

        Vector3 moveDirection = (rightDirection + forwardDirection).normalized;

        float finalSpeed = _viewModel.CurrentSpeed;

        if (_viewModel.IsRunning)
        {
            finalSpeed = finalSpeed * _viewModel.RunSpeedMultiplier;
        }

        Vector3 currentPosition = _rigidbody.position;
        Vector3 targetPosition = currentPosition + (moveDirection * finalSpeed * Time.fixedDeltaTime);

        _rigidbody.MovePosition(targetPosition);
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