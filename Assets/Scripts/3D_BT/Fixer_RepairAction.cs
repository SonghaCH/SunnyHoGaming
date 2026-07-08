using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Fixer_DoRepair", story: "[Self] repair for [Duration]", category: "Action/Fixer", id: "fixer-repair-action")]

public partial class Fixer_RepairAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Duration; // TODO : JSON파일 Active Data 소요 시간 연동

    private float _startTime;

    protected override Status OnStart()
    {
        _startTime = Time.time;

        // TODO : 애니메이션 재생 등 시각적 처리 (MVVM이나 Animator 연동)
        // Self.Value.GetComponent<Animator>().SetTrigger("Repair");

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Time.time - _startTime < Duration.Value)
        {
            return Status.Running;
        }

        // TODO: DataManager를 통해 수리 목표물의 체력을 회복시키고 플레이어 자원 차감

        return Status.Success; 
    }
}