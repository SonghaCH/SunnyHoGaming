using UnityEngine;
using Unity.Behavior;
using System.Collections.Generic;

public class FixerAgent : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent behaviorAgent;
    [SerializeField] private int fixerID;

    [SerializeField] private List<GameObject> mainRoomSpots;

    private void Start()
    {
        if (mainRoomSpots != null && mainRoomSpots.Count > 0)
        {
            behaviorAgent.SetVariableValue("MainRoomSpots", mainRoomSpots);
        }

        behaviorAgent.SetVariableValue("FixerState", FixerState.Rampaging);
    }
}
