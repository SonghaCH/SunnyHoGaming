using UnityEngine;
using Unity.Behavior;
using NUnit.Framework;
using System.Collections.Generic;

public class _BehaviorAgent : MonoBehaviour
{
    [SerializeField] private GameObject GObj_Target;
    [SerializeField] private BehaviorGraphAgent BehaviorAgent_Self;
    [SerializeField] private List<GameObject> PatrolSpotGameObjectList;

    private void Start()
    {
        if(PatrolSpotGameObjectList != null && PatrolSpotGameObjectList.Count > 0)
        {
            BehaviorAgent_Self.SetVariableValue("PatrolSpotList", PatrolSpotGameObjectList);
        }

        // 개선) 게임 매니저를 통해서 받아와도 되고, 게임오브젝트 매니저를 통해 받아와도 된다
        if(GObj_Target != null)
        {
            BehaviorAgent_Self.SetVariableValue("Target", GObj_Target.gameObject);
        }
    }



}
