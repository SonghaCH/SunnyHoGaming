using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BT_Wander", story: "[Self] Navigate To WanderSpotList", category: "Action", id: "f4b7e69fc7e5edd0b67a2155cd870364")]
public partial class BT_WanderAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private NavMeshAgent _agent;
    private Vector3 _wanderPosition;
    private float _currentWanderTime = 0.0f;
    private float _maxWanderTime = 5.0f;

    protected override Status OnStart()
    {
        int jitterMin = 0;
        int jitterMax = 360;
        float wanderRadius = UnityEngine.Random.Range(2.5f, 6.0f);
        int wanderJitter = UnityEngine.Random.Range(jitterMin, jitterMax);

        // 목표 위치를 = 자신의 위치 + 각도 반지름 크기의 원 둘레 위치만큼 가져옴
        _wanderPosition = Self.Value.transform.position + GetPositionFromAngle(wanderRadius, wanderJitter);
        _agent = Self.Value.GetComponent<NavMeshAgent>();
        _agent.SetDestination(_wanderPosition);
        _currentWanderTime = Time.time;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // 최대 재생시간 동안  목표에 도달했거나 재생 시간이 다 지나면 완료로 노드 종료
        if ((_wanderPosition - Self.Value.transform.position).sqrMagnitude < 0.1f
            || Time.time - _currentWanderTime > _maxWanderTime)
        {
            return Status.Success;
        }

        // 도달하지 않았다면 계속 실행중 반환
        return Status.Running;
    }

    private Vector3 GetPositionFromAngle(float radius, float angle)
    {
        // 각도를 기준으로 원의 둘레 위치 구함
        var position = Vector3.zero;
        angle = DegreeToRadian(angle);

        position.x = Mathf.Cos(angle) * radius;
        position.z = Mathf.Sin(angle) * radius;

        return position;
    }

    private float DegreeToRadian(float angle)
    {
        // Degree 값을 Radian 값으로 변환
        return Mathf.PI * angle / 180;
    }

    protected override void OnEnd()
    {
    }
}

