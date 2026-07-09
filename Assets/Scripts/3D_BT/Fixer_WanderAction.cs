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
        if (Self.Value == null)
        {
            return Status.Failure;
        }

        var viewModel = Self.Value.GetComponent<FixerViewModel>();
        if (viewModel != null)
        {
            viewModel.CurrentState = FixerState.Wandering;
        }

        _navMeshAgent = Self.Value.GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
        {
            return Status.Failure;
        }
        if (Speed != null) _navMeshAgent.speed = Speed.Value;
        Vector3 randomDirection = Random.insideUnitSphere * Radius.Value;
        Vector3 rawTargetPosition = Self.Value.transform.position + randomDirection;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawTargetPosition, out hit, Radius.Value, NavMesh.AllAreas))
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
            return Status.Success;
        }
        if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
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