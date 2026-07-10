using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Fixer State", story: "[Self] sets Fixer State to [NewState]", category: "Action", id: "SetFixerStateNode_v1")]
public partial class SetFixerState : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<FixerState> NewState;

    protected override Status OnStart()
    {
        if (Self.Value == null) return Status.Failure;

        if (Self.Value.TryGetComponent(out FixerViewModel viewModel))
        {
            viewModel.ChangeStateFromBrain(NewState.Value);
            return Status.Success;
        }

        return Status.Failure;
    }
}