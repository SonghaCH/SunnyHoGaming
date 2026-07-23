using UnityEngine;
using UnityEngine.AI;

public class FixerPlayerDetector : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float rotationSpeed = 5f;

    [SerializeField]
    private FixerState[] reactableStates =
    {
        FixerState.Wandering,
        FixerState.Idle,
        FixerState.Returning,
        FixerState.MoveToTarget
    };

    private NavMeshAgent _navMeshAgent;
    private FixerViewModel _viewModel;
    private Transform _player;

    private bool _isLookingAtPlayer;
    private bool _hasStoredPreviousState;
    private FixerState _previousState;

    private void Awake()
    {
        _navMeshAgent = GetComponentInParent<NavMeshAgent>();
        _viewModel = GetComponentInParent<FixerViewModel>();

        if (_navMeshAgent == null)
        {
            Debug.LogError($"[FixerPlayerDetector] {name} - NavMeshAgent를 찾을 수 없습니다.");
        }

        if (_viewModel == null)
        {
            Debug.LogError($"[FixerPlayerDetector] {name} - FixerViewModel을 찾을 수 없습니다.");
        }
    }

    private void OnEnable()
    {
        if (_viewModel != null)
        {
            _viewModel.OnStateChanged += HandleExternalStateChange;
        }
    }

    private void OnDisable()
    {
        if (_viewModel != null)
        {
            _viewModel.OnStateChanged -= HandleExternalStateChange;
        }
    }

    public void ReleaseControl()
    {
        if (_isLookingAtPlayer == false)
        {
            return;
        }

        _isLookingAtPlayer = false;
        _hasStoredPreviousState = false;
        _player = null;

        if (_viewModel != null)
        {
            _viewModel.SetInteractingLock(false);
        }

        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.updateRotation = true;
        }
    }

    
    public void RestoreControl()
    {
        if (_isLookingAtPlayer == false)
        {
            return;
        }

        _isLookingAtPlayer = false;
        _player = null;

        if (_viewModel != null)
        {
            _viewModel.SetInteractingLock(false);
        }

        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.updateRotation = true;
        }

        if (_hasStoredPreviousState && _viewModel != null)
        {
            _viewModel.CurrentState = _previousState;
            _hasStoredPreviousState = false;
        }
    }

    private void HandleExternalStateChange(FixerState newState)
    {
        if (newState == FixerState.Idle)
        {
            return; 
        }

        ReleaseControl();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) == false)
        {
            return;
        }

        if (_viewModel == null || _navMeshAgent == null)
        {
            return;
        }

        if (IsReactableState(_viewModel.CurrentState) == false)
        {
            return;
        }

        _player = other.transform;
        _isLookingAtPlayer = true;

        _previousState = _viewModel.CurrentState;
        _hasStoredPreviousState = true;
        _viewModel.SetInteractingLock(true);

        _viewModel.CurrentState = FixerState.Idle;

        if (_navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
            _navMeshAgent.updateRotation = false; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) == false)
        {
            return;
        }

        _isLookingAtPlayer = false;
        _player = null;

        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.updateRotation = true;
        }

        if (_hasStoredPreviousState && _viewModel != null)
        {
            _viewModel.CurrentState = _previousState;
            _hasStoredPreviousState = false;
        }
    }

    private void Update()
    {
        if (_isLookingAtPlayer == false || _player == null)
        {
            return;
        }

        Vector3 direction = _player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private bool IsReactableState(FixerState state)
    {
        for (int i = 0; i < reactableStates.Length; i++)
        {
            if (reactableStates[i] == state)
            {
                return true;
            }
        }

        return false;
    }
}