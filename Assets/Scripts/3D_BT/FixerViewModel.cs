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
        _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        _animator = GetComponent<Animator>();
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

                if (OnStateChanged != null) OnStateChanged.Invoke(currentState);
            }
        }
    }

    private void SyncToBehaviorTree(FixerState state)
    {
        if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
        {
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("CurrentState", state);

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
                _animator.CrossFade("Walk", 0.1f);
                break;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) SyncToBehaviorTree(currentState);
    }
}