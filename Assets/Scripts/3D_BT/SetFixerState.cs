using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Fixer State", story: "[Self] sets Fixer State to [NewState] (Silent: [IsSilent])", category: "Action", id: "SetFixerStateNode_v1")]
public partial class SetFixerState : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<FixerState> NewState;

    [SerializeReference] public BlackboardVariable<bool> IsSilent;
    protected override Status OnStart()
    {
        if (Self.Value == null)
        {
            return Status.Failure;
        }

        if (Self.Value.TryGetComponent(out FixerViewModel viewModel))
        {
            bool silent;

            if (IsSilent != null)
            {
                silent = IsSilent.Value;
            }
            else
            {
                silent = false;
            }

            viewModel.ChangeStateFromBrain(NewState.Value, silent);

            return Status.Success;
        }

        return Status.Failure;
    }
}