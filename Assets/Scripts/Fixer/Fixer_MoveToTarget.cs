using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fixer_MoveToWork", story: "[Self] moves to [WorkTargetPosition]", category: "Action/Fixer", id: "fixer-move-work-action")]
public partial class Fixer_MoveToTarget : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> WorkTargetPosition;

    private NavMeshAgent _navMeshAgent;
    private FixerViewModel _viewModel;

    protected override Status OnStart()
    {
        if (Self.Value == null)
        {
            return Status.Failure;
        }

        if (Self.Value.TryGetComponent(out _navMeshAgent) == false)
        {
            return Status.Failure;
        }

        Self.Value.TryGetComponent(out _viewModel);

        _navMeshAgent.SetDestination(WorkTargetPosition.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_navMeshAgent.pathPending == false && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            if (_viewModel != null)
            {
                _viewModel.TriggerArrivalEvent();
            }

            return Status.Success;
        }

        return Status.Running;
    }
}