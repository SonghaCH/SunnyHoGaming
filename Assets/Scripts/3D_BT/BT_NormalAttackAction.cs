using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BT_NormalAttack", story: "[Self] Try Attack", category: "Action", id: "782b80145363575d7269daf4f0dd9627")]
public partial class BT_NormalAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private BattleAgent BattleAgentSelf;
    private float _coolDownDuration = 3.0f;
    private float _lastAttackTime = -3.0f;

    protected override Status OnStart()
    {
        // 노드 진입 시 1번만 실행되는 초기화 로직
        if (BattleAgentSelf == null && Self.Value != null)
        {
            BattleAgentSelf = Self.Value.GetComponent<BattleAgent>();
        }

        // 1) 주석 해제 후 체크 - OnStart에서 작업이 즉시 끝나는 경우에만 Success를 반환 (즉시 끝나는게 아니고 Update로 넘어가야 한다면 Running)
        // return Status.Success;
        // 2) 즉시 끝나는게 아니라 OnUpdate로 넘어가는 경우 
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (BattleAgentSelf == null) return Status.Failure;

        // 쿨타임이 안 지났다면 Failure를 반환하여 상위 노드가 다른 행동을 찾게 함
        // (제자리에 서서 대기하게 하려면 Running을 반환해도 됨)
        if (Time.time - _lastAttackTime < _coolDownDuration)
        {
            // 3) 주석 해제 후 체크 - 이때도 Failure를 반환하면 상위 노드는 "공격 불가"로 인식하고 다음 우선순위 노드(예: 이동 노드)로 넘어감
            return Status.Failure; // 공격 쿨타임 중에 추격을 할 수 있는 경우
            // return Status.Running; // 공격 쿨타임 중에는 이 노드에 머물게 하고 싶은 경우
        }

        // [실행] 쿨타임이 지났다면 공격 호출 후 타이머 갱신
        _lastAttackTime = Time.time;

        // 공격 명령 성공
        return Status.Success;
    }
}

