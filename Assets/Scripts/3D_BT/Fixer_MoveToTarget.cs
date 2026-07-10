using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fixer_MoveTOTarget", story: "[Self] [TargetState] to [Target]", category: "Action/Fixer", id: "fixer-move-action")]

public partial class Fixer_MoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<FixerState> TargetState;

    private NavMeshAgent _navMeshAgent;

    protected override Status OnStart()
    {
        if (Self.Value == null || Target.Value == null)
        {
            return Status.Failure;
        }
        if (!Self.Value.TryGetComponent(out _navMeshAgent))
        {
            return Status.Failure;
        }

        if (Self.Value.TryGetComponent(out FixerViewModel viewModel))
        {
            if (TargetState != null)
            {
                viewModel.ChangeStateFromBrain(TargetState.Value);
            }
        }

        _navMeshAgent.SetDestination(Target.Value.transform.position);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            return Status.Success; 
        }
        return Status.Running;
    }
}

