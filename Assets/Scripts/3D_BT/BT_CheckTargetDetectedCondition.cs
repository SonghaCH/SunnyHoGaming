using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "BT_CheckTargetDetected", story: "Compare values of [CurrentDistance] and [ChaseDistance]", category: "Conditions", id: "6014c3a3c6e3703d2c1340e402beee50")]
public partial class BT_CheckTargetDetectedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;
    [SerializeReference] public BlackboardVariable<float> ChaseDistance;

    public override bool IsTrue()
    {
        bool conditionResult = CurrentDistance.Value <= ChaseDistance.Value;
        return conditionResult;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
