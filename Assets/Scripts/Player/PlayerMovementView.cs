using System.ComponentModel;
using UnityEngine;
using static UnityEditor.Profiling.HierarchyFrameDataView;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class PlayerMovementView : ViewBase
{
    [SerializeField] private Transform _cameraTransform;

    private Rigidbody _rigidbody;
    private PlayerMovementViewModel _movementViewModel;
    private PlayerStatusViewModel _statusViewModel;
    private Transform _target;

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
            BindMovementViewModel(NetworkManager.Inst.PlayerService.GetMovementViewModel());
            BindStatusViewModel(NetworkManager.Inst.PlayerService.GetStatusViewModel());
        }
    }

    public void BindMovementViewModel(PlayerMovementViewModel viewModel)
    {
        _movementViewModel = viewModel;
        _movementViewModel.InvokeOnceOnInit();
    }

    public void BindStatusViewModel(PlayerStatusViewModel viewModel)
    {
        _statusViewModel = viewModel;
        _statusViewModel.PropertyChanged += OnPropertyChanged_StatusView;
        _statusViewModel.InvokeOnceOnInit();
    }

    private void OnDestroy()
    {
        if (_statusViewModel != null)
        {
            _statusViewModel.PropertyChanged -= OnPropertyChanged_StatusView;
        }
    }

    private void OnPropertyChanged_StatusView(object sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(PlayerStatusViewModel.IsSleeping))
        {
            ChangePlayerPositionAndRotation();
        }
    }

    private void Update()
    {
        if (_movementViewModel == null)
        {
            return;
        }

        if (!_movementViewModel.CanMove)
        {
            _inputX = 0.0f;
            _inputZ = 0.0f;
            return;
        }

        _inputX = Input.GetAxisRaw("Horizontal");
        _inputZ = Input.GetAxisRaw("Vertical");

        if (_inputX != 0.0f || _inputZ != 0.0f)
        {
            _movementViewModel.IsMoving = true;
        }
        else
        {
            _movementViewModel.IsMoving = false;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _movementViewModel.IsRunning = true;
        }
        else
        {
            _movementViewModel.IsRunning = false;
        }

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Look(mouseX, mouseY);
    }

    private void FixedUpdate()
    {
        if (_movementViewModel == null)
        {
            return;
        }

        if (!_movementViewModel.CanMove)
        {
            return;
        }

        Move(_inputX, _inputZ);
    }

    private void Move(float moveX, float moveZ)
    {
        Vector3 rightDirection = transform.right * moveX;
        Vector3 forwardDirection = transform.forward * moveZ;

        Vector3 moveDirection = (rightDirection + forwardDirection).normalized;

        float finalSpeed = _movementViewModel.CurrentSpeed;

        if (_movementViewModel.IsRunning)
        {
            finalSpeed = finalSpeed * _movementViewModel.RunSpeedMultiplier;
        }

        Vector3 currentPosition = _rigidbody.position;
        Vector3 targetPosition = currentPosition + (moveDirection * finalSpeed * Time.fixedDeltaTime);

        _rigidbody.MovePosition(targetPosition);

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    private void Look(float mouseX, float mouseY)
    {
        float lookX = mouseX * _movementViewModel.MouseSensitivity;
        float lookY = mouseY * _movementViewModel.MouseSensitivity;

        _xRotation -= lookY;
        _xRotation = Mathf.Clamp(_xRotation, -90.0f, 90.0f);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * lookX);
    }

    public void ChangePlayerPositionAndRotation()
    {
        transform.SetPositionAndRotation(_target.position, _target.rotation);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}