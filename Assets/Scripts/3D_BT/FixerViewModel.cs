using System;
using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FixerViewModel : MonoBehaviour
{
    public event Action<FixerState> OnStateChanged;

    [SerializeField] private FixerState currentState = FixerState.Rampaging;

    private bool hasBeenRepaired = false;
    private BehaviorGraphAgent _behaviorGraphAgent;

    private Animator _animator;

    private void Awake()
    {
        TryGetComponent(out _behaviorGraphAgent);
        TryGetComponent(out _animator);
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

                OnStateChanged.Invoke(currentState);
            }
        }
    }

    public void ChangeStateFromBrain(FixerState newState)
    {
        if (hasBeenRepaired && newState == FixerState.Rampaging)
        {
            return;
        }

        if (currentState != newState)
        {
            currentState = newState;

            if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
            {
                _behaviorGraphAgent.BlackboardReference.SetVariableValue("FixerState", currentState);
            }

            PlayStateAnimation(currentState);

            if (currentState != FixerState.Rampaging)
            {
                hasBeenRepaired = true;
            }

            OnStateChanged?.Invoke(currentState);
        }
    }

    private void SyncToBehaviorTree(FixerState state)
    {
        if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
        {
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("FixerState", state);
            _behaviorGraphAgent.Restart();
        }

        PlayStateAnimation(state);
    }

    private void PlayStateAnimation(FixerState state)
    {
        if (_animator == null) return;

        switch (state)
        {
            case FixerState.Idle: 
                _animator.CrossFade("Idle", 0.1f);
                break;
            case FixerState.Rampaging: 
                _animator.CrossFade("CrashRun", 0.1f);
                break;
            case FixerState.Executing: 
                _animator.CrossFade("Fix", 0.1f);
                break;
            case FixerState.Returning:
                _animator.Play("Run");
                break;
            case FixerState.Wandering:
                _animator.CrossFade("Walk", 0.1f);
                break;
            case FixerState.MoveToTarget:
                _animator.CrossFade("Run", 0.1f);
                break;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SyncToBehaviorTree(currentState);
        }
    }
}