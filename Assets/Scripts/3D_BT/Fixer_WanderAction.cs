using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fixer_Wandering", story: "[Self] wander within [Radius] at spped of [Speed]", category: "Action/Fixer", id: "fixer-wander-radius-action")]
public partial class Fixer_WanderAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<float> Speed;

    private NavMeshAgent _navMeshAgent;
    private float _currentWanderTime;
    private float _maxWanderTime = 7.0f;

    protected override Status OnStart()
    {
        if (Self.Value == null || !Self.Value.TryGetComponent(out _navMeshAgent))
        {
            return Status.Failure;
        }

        if (Self.Value.TryGetComponent(out FixerViewModel viewModel))
        {
            viewModel.ChangeStateFromBrain(FixerState.Wandering);
        }

        if (Speed != null) _navMeshAgent.speed = Speed.Value;

        Vector3 rawTargetPosition = Self.Value.transform.position + (Random.insideUnitSphere * Radius.Value);
        if (NavMesh.SamplePosition(rawTargetPosition, out NavMeshHit hit, Radius.Value, NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(hit.position);
            _currentWanderTime = Time.time;
            return Status.Running;
        }
        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (Time.time - _currentWanderTime > _maxWanderTime)
        {
            if (Self.Value != null && Self.Value.TryGetComponent(out FixerViewModel viewModel))
            {
                viewModel.ChangeStateFromBrain(FixerState.Idle);
            }

            return Status.Success;
        }

        if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            if (Self.Value != null && Self.Value.TryGetComponent(out FixerViewModel viewModel))
            {
                viewModel.ChangeStateFromBrain(FixerState.Idle);
            }

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.ResetPath();
        }
    }
}