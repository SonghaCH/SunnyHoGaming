using UnityEngine;
using Unity.Behavior;

public class FixerModel : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent behaviorAgent;
    [SerializeField] private int fixerID;

    [SerializeField] private GameObject mainRoomSpot;

    private void Start()
    {
        if (mainRoomSpot != null)
        {
            behaviorAgent.SetVariableValue("MainRoomSpot", mainRoomSpot);
        }

        behaviorAgent.SetVariableValue("FixerState", FixerState.Rampaging);
    }
}
