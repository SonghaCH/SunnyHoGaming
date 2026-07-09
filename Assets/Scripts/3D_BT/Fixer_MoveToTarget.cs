using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fixer_MoveTOTarget", story: "[Self] to [Target]", category: "Action/Fixer", id: "fixer-move-action")]

public partial class Fixer_MoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<FixerState> TargetState;

    private NavMeshAgent _navMeshAgent;

    protected override Status OnStart()
    {
        _navMeshAgent = Self.Value.GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null || Target.Value == null) return Status.Failure;

        var viewModel = Self.Value.GetComponent<FixerViewModel>();
        if (viewModel != null)
        {
            viewModel.PlayAnimationFor(FixerState.Wandering);
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

