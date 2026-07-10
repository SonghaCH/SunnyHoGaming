using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fixer_Rampaging", story: "[Self] wander within [RoomArea] at spped of [Speed]", category: "Action/Fixer", id: "fixer-Rampage-action")]
public partial class Fixer_RampagingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Collider> RoomArea;
    [SerializeReference] public BlackboardVariable<float> Speed;

    private NavMeshAgent _navMeshAgent;
    private float _currentWanderTime = 0.0f;
    private float _maxWanderTime = 5.0f;

    protected override Status OnStart()
    {
        if (Self.Value == null) 
        {
            return Status.Failure; 
        }

        if (RoomArea.Value == null)
        {
            return Status.Failure;
        }

        if (!Self.Value.TryGetComponent(out _navMeshAgent))
        {
            return Status.Failure;
        }

        if (Self.Value.TryGetComponent(out FixerViewModel viewModel))
        {
            viewModel.ChangeStateFromBrain(FixerState.Rampaging);
        }

        if (Speed != null) _navMeshAgent.speed = Speed.Value;

        if (TryGetValidRoomDestination(RoomArea.Value.bounds, out Vector3 targetDestination))
        {
            _navMeshAgent.SetDestination(targetDestination);
            _currentWanderTime = Time.time;
            return Status.Running;
        }

        Debug.LogWarning($"[{Self.Value.name}] 방 안에서 유효한 NavMesh 바닥 좌표를 찾지 못했습니다.");
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

    private bool TryGetValidRoomDestination(Bounds roomBounds, out Vector3 destination)
    {
        destination = Vector3.zero;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(roomBounds.min.x, roomBounds.max.x),
                Self.Value.transform.position.y,
                Random.Range(roomBounds.min.z, roomBounds.max.z)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2.0f, NavMesh.AllAreas))
            {
                if (hit.position.x >= roomBounds.min.x && hit.position.x <= roomBounds.max.x &&
                    hit.position.z >= roomBounds.min.z && hit.position.z <= roomBounds.max.z)
                {
                    destination = hit.position;
                    return true; 
                }
            }
        }

        return false; 
    }
}