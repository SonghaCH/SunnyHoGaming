using Unity.Behavior;
using UnityEngine;

public class _NewBehaviorAgent : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent BehaviorAgent_Self;

    private void Start()
    {
        BehaviorAgent_Self.SetVariableValue("IsAAA", true);

        BlackboardVariable<bool> aaa;
        var boolValue = BehaviorAgent_Self.GetVariable("IsAAA", out aaa);
    }

}
