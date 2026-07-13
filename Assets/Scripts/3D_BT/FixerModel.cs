using UnityEngine;
using Unity.Behavior;

public class FixerModel : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent behaviorAgent;
    [SerializeField] private int fixerID;

    private void Start()
    {
        if (behaviorAgent != null && behaviorAgent.BlackboardReference != null)
        {
            behaviorAgent.SetVariableValue("FixerState", FixerState.Rampaging);
        }
    }
}
