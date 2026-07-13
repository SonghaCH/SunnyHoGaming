using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fixer_Wait", story: "[Self] wait for [Duration] seconds", category: "Action/Fixer", id: "fixer-Wait-action")]

public class Fixer_WaitAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Duration;

    private float _startTime;

    protected override Status OnStart()
    {
        _startTime = Time.time;

        if (Self.Value != null && Self.Value.TryGetComponent(out FixerViewModel viewModel)) 
        {
            viewModel.ChangeStateFromBrain(FixerState.Idle, true);
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Time.time - _startTime >= Duration) 
        {
            return Status.Success;
        }
        return Status.Running;
    }
}
