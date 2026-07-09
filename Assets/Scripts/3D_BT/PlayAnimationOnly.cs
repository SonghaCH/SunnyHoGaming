using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Play Animation Only", story: "[Self] plays animation for [AnimState]", category: "Action/Fixer", id: "fixer-play-animation-only")]
public partial class PlayAnimationOnlyNode : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<FixerState> AnimState;

    protected override Status OnStart()
    {
        if (Self.Value == null) return Status.Failure;

        var viewModel = Self.Value.GetComponent<FixerViewModel>();
        if (viewModel != null)
        {
            viewModel.PlayAnimationFor(AnimState.Value);
        }

        return Status.Success;
    }
}