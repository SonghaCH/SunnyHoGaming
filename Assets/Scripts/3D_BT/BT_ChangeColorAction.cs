using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChangeColor", story: "[Self] To [ChangeColorTarget] [ChangeColor]", category: "Action", id: "3ff3bd33a93cdf9b10807f5e510b1918")]
public partial class BT_ChangeColorAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> ChangeColorTarget;
    [SerializeReference] public BlackboardVariable<Color> ChangeColor;

    protected override Status OnStart()
    {
        var renderer = ChangeColorTarget.Value.GetComponent<Renderer>();
        renderer.material.color = ChangeColor;

        return Status.Success;
    }

}

