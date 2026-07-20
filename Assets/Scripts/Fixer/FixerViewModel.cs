using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class FixerViewModel : MonoBehaviour
{
    private FixerModel _fixerModel;

    public int InstanceId { get; private set; }
    public string DataId { get; private set; }

    public WorkStation TargetStation { get; set; }

    public event Action<FixerState> OnStateChanged;
    public event Action<FixerState> OnAnimationStateChanged;
    public event Action<FixerViewModel> OnArrivedAtWorkStation;

    [SerializeField] private FixerState currentState = FixerState.Rampaging;

    private bool hasBeenRepaired = false;
    private BehaviorGraphAgent _behaviorGraphAgent;
    private NavMeshAgent _navMeshAgent;

    private void Awake()
    {
        bool hasBehavior = TryGetComponent(out _behaviorGraphAgent);
        if (hasBehavior == false)
        {
            Debug.LogError($"[FixerViewModel] BehaviorGraphAgent 컴포넌트가 없습니다.");
        }

        bool hasNavMesh = TryGetComponent(out _navMeshAgent);
        if (hasNavMesh == false)
        {
            Debug.LogError($"[FixerViewModel] NavMeshAgent 컴포넌트가 없습니다.");
        }
    }

    public FixerState CurrentState
    {
        get
        {
            return currentState;
        }

        set
        {
            UpdateStateInternal(value, isSilent: false, isFromBrain: false);
        }
    }

    public void ChangeStateFromBrain(FixerState newState, bool isSilent = false)
    {
        UpdateStateInternal(newState, isSilent, isFromBrain: true);
    }

    private void UpdateStateInternal(FixerState newState, bool isSilent, bool isFromBrain)
    {
        if (hasBeenRepaired == true && newState == FixerState.Rampaging)
        {
            return;
        }

        if (currentState == newState)
        {
            return;
        }

        currentState = newState;

        if (currentState != FixerState.Rampaging)
        {
            hasBeenRepaired = true;
        }

        if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
        {
            bool isSuccess = _behaviorGraphAgent.BlackboardReference.SetVariableValue("FixerState", currentState);

            if (isSuccess == false)
            {
                Debug.LogError($" [동기화 에러] BT 블랙보드에 값을 넣지 못했습니다! C# 상태: {currentState}");
            }

            if (isFromBrain == false)
            {
                _behaviorGraphAgent.Restart();
            }
        }

        if (OnAnimationStateChanged != null)
        {
            OnAnimationStateChanged.Invoke(currentState);
        }

        if (isSilent == false)
        {
            if (OnStateChanged != null)
            {
                OnStateChanged.Invoke(currentState);
            }
        }
    }

    private void SyncToBehaviorTree(FixerState state)
    {
        if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
        {
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("FixerState", state);
            _behaviorGraphAgent.Restart();
        }

        if (OnAnimationStateChanged != null)
        {
            OnAnimationStateChanged.Invoke(state);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            SyncToBehaviorTree(currentState);
        }
    }

    public void InitFixer(int instanceId, string dataId, FixerState initialState = FixerState.Rampaging)
    {
        InstanceId = instanceId;
        DataId = dataId;

        _fixerModel = new FixerModel();
        _fixerModel.FixerID = instanceId;

        FixerData FixerData = GameDataManager.Instance.GetFixerData(dataId);

        if (FixerData != null)
        {
            _fixerModel.Name = FixerData.Name;
            _fixerModel.O2Repair = FixerData.O2Repair;
            _fixerModel.ElectRepair = FixerData.ElectRepair;
            _fixerModel.ControlRepair = FixerData.ControlRepair;
            _fixerModel.TempRepair = FixerData.TempRepair;
            _fixerModel.FarmingFood = FixerData.FarmingFood;
            _fixerModel.FarmingScrap = FixerData.FarmingScrap;


            Debug.Log($"[FixerViewModel] {FixerData.Name} 초기화 완료 - 수리력 셋팅됨");
        }
        else
        {
            Debug.LogError($"[FixerViewModel] ID가 {dataId}인 FixerData 마스터 데이터를 찾을 수 없습니다!");
        }

        CurrentState = initialState;
    }
    public float GetWorkEfficiency(WorkType taskType)
    {
        if (_fixerModel == null) return 1f;

        switch (taskType)
        {
            case WorkType.O2Repair: return _fixerModel.O2Repair;
            case WorkType.ElectRepair: return _fixerModel.ElectRepair;
            case WorkType.ControlRepair: return _fixerModel.ControlRepair;
            case WorkType.TempRepair: return _fixerModel.TempRepair;
            case WorkType.FarmingFood: return _fixerModel.FarmingFood;
            case WorkType.FarmingScrap: return _fixerModel.FarmingScrap;
            default: return 1f;
        }
    }
    public void SetWorkTarget(Vector3 targetPosition, WorkType taskType)
    {
        if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
        {
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("WorkTargetPosition", targetPosition);
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("CurrentTaskType", (int)taskType);
        }

        CurrentState = FixerState.MoveToTarget;
    }

    public void TriggerArrivalEvent()
    {
        CurrentState = FixerState.Executing;

        OnArrivedAtWorkStation?.Invoke(this);
    }

    public void SetMainRoomTransformToBlackboard(Transform mainRoomTransform)
    {
        if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
        {
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("MainRoomTransform", mainRoomTransform);
            Debug.Log($"[{gameObject.name}] 메인 룸 좌표 블랙보드 등록 완료!");
        }
    }

    public void SetRoomAreaToBlackboard(Collider roomAreaCollider)
    {
        if (_behaviorGraphAgent != null && _behaviorGraphAgent.BlackboardReference != null)
        {
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("RoomArea", roomAreaCollider);
            Debug.Log($"[{gameObject.name}] 룸 에리어 구역 블랙보드 등록 완료!");
        }
    }

    public void OrderReturn()
    {
        CurrentState = FixerState.Returning;
    }

    public void TriggerReturnComplete()
    {
        CurrentState = FixerState.Idle;
    }
}