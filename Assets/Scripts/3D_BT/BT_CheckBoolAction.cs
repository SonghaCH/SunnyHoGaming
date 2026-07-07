using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BT_CheckBool", story: "[Self] check [IsAAA]", category: "Action", id: "9fd3cadf752893f982bfbabd3d7696cc")]
public partial class BT_CheckBoolAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> IsAAA;


    protected override Status OnStart()
    {
        var aaa = (IsAAA.Value == true) ? Status.Failure : Status.Success;
        return aaa;
    }
}

