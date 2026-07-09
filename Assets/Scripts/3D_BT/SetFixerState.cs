using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Set Fixer State",
    story: "[Self] sets Fixer State to [NewState]",
    category: "Action",
    id: "SetFixerStateNode_v1"
)]
public partial class SetFixerState : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<FixerState> NewState;

    protected override Status OnStart()
    {
        if (Self.Value == null)
        {
            Debug.LogWarning("[SetFixerStateNode] Self is null.");
            return Status.Failure;
        }

        var viewModel = Self.Value.GetComponent<FixerViewModel>();
        if (viewModel == null)
        {
            Debug.LogWarning("[SetFixerStateNode] FixerViewModel not found on Self.");
            return Status.Failure;
        }

        viewModel.CurrentState = NewState.Value;

        return Status.Success;
    }
}