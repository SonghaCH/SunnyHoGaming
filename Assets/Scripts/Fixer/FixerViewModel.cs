using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class FixerViewModel : MonoBehaviour
{
    private FixerModel _fixerModel;

    public int InstanceId { get; private set; }
    public string DataId { get; private set; }

    public event Action<FixerState> OnStateChanged;
    public event Action<FixerState> OnAnimationStateChanged;

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
            _behaviorGraphAgent.BlackboardReference.SetVariableValue("FixerState", currentState);

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

        FixerData masterData = GameDataManager.Instance.GetFixerData(dataId);

        if (masterData != null)
        {
            _fixerModel.Name = masterData.Name;
            _fixerModel.O2Repair = masterData.O2Repair;
            _fixerModel.ElectRepair = masterData.ElectRepair;
            _fixerModel.WayRepair = masterData.WayRepair;
            _fixerModel.FarmingRepair = masterData.FarmingRepair;
            _fixerModel.TempRepair = masterData.TempRepair;

            Debug.Log($"[FixerViewModel] {masterData.Name} 초기화 완료 - 수리력 셋팅됨");
        }
        else
        {
            Debug.LogError($"[FixerViewModel] ID가 {dataId}인 FixerData 마스터 데이터를 찾을 수 없습니다!");
        }

        CurrentState = initialState;
    }
}