using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fixer_ReturnToMainRoom", story: "[Self] returns to [TargetTransform]", category: "Action/Fixer", id: "fixer-return-action")]
public partial class Fixer_Return : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> TargetTransform;

    private NavMeshAgent _navMeshAgent;
    private FixerViewModel _viewModel;

    protected override Status OnStart()
    {
        if (Self.Value == null || !Self.Value.TryGetComponent(out _navMeshAgent))
            return Status.Failure;

        Self.Value.TryGetComponent(out _viewModel);
        
        if (TargetTransform.Value != null)
        {
            _navMeshAgent.SetDestination(TargetTransform.Value.position);
            return Status.Running;
        }
        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            if (_viewModel != null)
            {
                _viewModel.TriggerReturnComplete();
            }
            return Status.Success;
        }
        return Status.Running;
    }
}