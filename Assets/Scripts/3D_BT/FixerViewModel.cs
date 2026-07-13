using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class FixerViewModel : MonoBehaviour
{
    public event Action<FixerState> OnStateChanged;

    public event Action<FixerState> OnAnimationStateChanged;

    [SerializeField] private FixerState currentState = FixerState.Rampaging;

    private bool hasBeenRepaired = false;
    private BehaviorGraphAgent _behaviorGraphAgent;
    private NavMeshAgent _navMeshAgent;

    private void Awake()
    {
        TryGetComponent(out _behaviorGraphAgent);
        TryGetComponent(out _navMeshAgent);
    }

    public FixerState CurrentState
    {
        get
        {
            return currentState;
        }

        set
        {
            if (hasBeenRepaired && value == FixerState.Rampaging)
            {
                return;
            }

            if (currentState != value)
            {
                currentState = value;
                SyncToBehaviorTree(currentState);

                if (currentState != FixerState.Rampaging)
                {
                    hasBeenRepaired = true;
                }

                OnAnimationStateChanged?.Invoke(currentState);
                OnStateChanged.Invoke(currentState);
            }
        }
    }

    public void ChangeStateFromBrain(FixerState newState, bool isSilent = false)
    {
        if (hasBeenRepaired && newState == FixerState.Rampaging) return;

        if (currentState != newState)
        {
            currentState = newState;

            if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
            {
                _behaviorGraphAgent.BlackboardReference.SetVariableValue("FixerState", currentState);
            }

            if (currentState != FixerState.Rampaging) hasBeenRepaired = true;

            OnAnimationStateChanged?.Invoke(currentState);

            if (!isSilent)
            {
                OnStateChanged?.Invoke(currentState);
            }
        }
    }

    private void SyncToBehaviorTree(FixerState state)
    {
        if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
        {
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("FixerState", state);
            _behaviorGraphAgent.Restart();
        }

        OnAnimationStateChanged?.Invoke(state);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SyncToBehaviorTree(currentState);
        }
    }
}