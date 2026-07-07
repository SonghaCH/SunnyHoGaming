using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BT_ChangeAnimationState", story: "[Self] to [StateEnum]", category: "Action", id: "ac6820a98a05daaa6beefccbd960f10c")]
public partial class BT_ChangeAnim : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<BattleState> ChangeStateEnum;

    protected override Status OnStart()
    {
        var battleAgentSelf = Self.Value.GetComponent<BattleAgent>();
        if (battleAgentSelf)
        {
            battleAgentSelf.ChangeState(ChangeStateEnum);
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

