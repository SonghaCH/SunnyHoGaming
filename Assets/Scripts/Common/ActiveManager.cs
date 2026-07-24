using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;



public enum ActiveTaskType
{
    OxygenSupply,  //active_01
    PowerSupply,   //active_02
    TemperatureControl, //active_03
    RouteControl,  //active_04
    Farming,            // active_05 (3일차 해금, 픽서 전용)
    FoodSupply          // active_06 (4일차 해금, 픽서 전용)
}


// 확인용으로 [Serializable] 붙였음 나중에 없앨 예정
[Serializable]
public class ActiveAssignment
{
    public ActiveTaskType TaskType;  // 수행 중인 작업 종류
    public int FixerID;              // 배정된 픽서 ID (FixerModel.FixerID)
    public bool IsWorking;           // 현재 작업 중인지 여부 (true: 작업 중, false: 대기 중)
    public float CurrentWorkTime;   // 경과 작업시간
    public float TotalTimeTaken;    // 총 소요 시간

    public float WorkProgressRatio
    {
        get
        {
            float maxTime = Mathf.Max(TotalTimeTaken, 0.01f);
            return Mathf.Clamp01(CurrentWorkTime / maxTime) * 100f;
        }
    }
}


public class ActiveManager : MonoBehaviour
{
    public static ActiveManager Instance { get; private set; }

    [Header("Default Balance Setting")]
    [SerializeField] private float miniGameSuccessRestoreValue = 25f; // 미니게임 성공 시 복구량
    [SerializeField] private float fixerBaseRepairValue = 20f;  // 픽서 수리 기본 복구량

    // 1~4번 작업 수리 진척도 게이지 (Key: TaskType, Value: 0~100% 진척도 수치)
    private Dictionary<ActiveTaskType, float> _systemProgressDict = new Dictionary<ActiveTaskType, float>();

    // 당일 작업 완료 여부 통합 관리 (플레이어/픽서 공유 클리어 플래그)
    private Dictionary<ActiveTaskType, bool> _miniGameClearedTodayDict = new Dictionary<ActiveTaskType, bool>();

    // 픽서 배정 및 실시간 작업 진행 정보
    private Dictionary<ActiveTaskType, ActiveAssignment> _assignmentDict = new Dictionary<ActiveTaskType, ActiveAssignment>();

    private TimeViewModel _timeViewModel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BindTimeViewModel();
    }

    private void BindTimeViewModel()
    {
        if (_timeViewModel == null && NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            _timeViewModel = NetworkManager.Inst.TimeService.GetViewModel();
            if (_timeViewModel != null)
            {
                _timeViewModel.PropertyChanged -= OnPropertyChanged;
                _timeViewModel.PropertyChanged += OnPropertyChanged;
            }
        }
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeViewModel.CurrentDay))
        {
            HandleNewDay();
        }
    }

    private void InitializeData()
    {
        _systemProgressDict.Clear();
        _miniGameClearedTodayDict.Clear();
        _assignmentDict.Clear();

        // 1~4번 작업 진행도 및 일일 클리어 플래그 초기화
        Array types = Enum.GetValues(typeof(ActiveTaskType));
        foreach (ActiveTaskType type in types)
        {
            // 1~4번 생존 수리 작업만 수리 진척도 게이지(100%) 등록
            if (type <= ActiveTaskType.RouteControl)
            {
                _systemProgressDict.Add(type, 100f);
            }
            _miniGameClearedTodayDict.Add(type, false); // 하루 시작 시 작업 완료 상태 초기화
        } 
    }

    public string GetActiveDataId(ActiveTaskType type)
    {

        switch (type)
        {
            case ActiveTaskType.OxygenSupply: return "active_01";
            case ActiveTaskType.PowerSupply: return "active_02";
            case ActiveTaskType.TemperatureControl: return "active_03";
            case ActiveTaskType.RouteControl: return "active_04";
            case ActiveTaskType.Farming: return "active_05";
            case ActiveTaskType.FoodSupply: return "active_06";
            default: return "active_01";
        }
    }
    public string GetRequiredResourceId(ActiveTaskType type)
    {
        switch (type)
        {
            case ActiveTaskType.OxygenSupply: return "Item_Resources_01";
            case ActiveTaskType.PowerSupply: return "Item_Resources_02";
            case ActiveTaskType.TemperatureControl: return "Item_Resources_03";
            case ActiveTaskType.RouteControl: return "Item_Resources_04";
            default: return string.Empty;
        }
    }
    public int GetRequiredResourcesCost(ActiveTaskType type)
    {
        string activeDataId = GetActiveDataId(type);

        if (GameDataManager.Instance != null)
        {
            ActiveData activeData = GameDataManager.Instance.GetActiveData(activeDataId);
            if (activeData != null)
            {
                return activeData.ResourcesCost;
            }
        }
        return 1; 
    }

    private int GetInventoryItemCount(string itemId)
    {
        if (NetworkManager.Inst == null || NetworkManager.Inst.InventoryService == null)
            return 0;

        var invenVM = NetworkManager.Inst.InventoryService.GetLocalInventoryViewModel();
        if (invenVM == null || invenVM.ItemList == null) return 0;

        int total = 0;
        foreach (var slot in invenVM.ItemList.Values)
        {
            if (slot != null && slot.ItemDataId == itemId)
            {
                total += slot.ItemStackCount;
            }
        }
        return total;
    }

    private bool ConsumeInventoryItem(string itemId, int count)
    {
        if (NetworkManager.Inst == null || NetworkManager.Inst.InventoryService == null)
            return false;

        var invenVM = NetworkManager.Inst.InventoryService.GetLocalInventoryViewModel();
        if (invenVM == null || invenVM.ItemList == null) return false;

        int remaining = count;
        List<long> removeList = new List<long>();

        foreach (var kvp in invenVM.ItemList)
        {
            var slot = kvp.Value;
            if (slot != null && slot.ItemDataId == itemId)
            {
                if (slot.ItemStackCount <= remaining)
                {
                    remaining -= slot.ItemStackCount;
                    slot.ItemStackCount = 0;
                    removeList.Add(kvp.Key);
                }
                else
                {
                    slot.ItemStackCount -= remaining;
                    remaining = 0;
                    break;
                }
            }
        }

        foreach (long uniqueId in removeList)
        {
            invenVM.RemoveItemSlotViewModel(uniqueId);
        }

        invenVM.RefreshItemList();
        return true;
    }

    public int GetTaskUnlockDate(ActiveTaskType type)
    {
        string dataId = GetActiveDataId(type);

        if (GameDataManager.Instance != null)
        {
            ActiveData activeData = GameDataManager.Instance.GetActiveData(dataId);
            if (activeData != null)
            {
                return activeData.UnlockDate;
            }
        }
        return 1; // 예외 발생 시 기본 1일차
    }

    public float GetTimeToConsumeValue(ActiveTaskType type)
    {
        string dataId = GetActiveDataId(type);

        if (GameDataManager.Instance != null)
        {
            ActiveData activeData = GameDataManager.Instance.GetActiveData(dataId);
            if (activeData != null)
            {
                return activeData.TimeToConsume;
            }
        }
        return 3; // 예외 발생 시 기본 차감량 3
    }

    //현재 게임 날짜 기준으로 해당 작업이 해금되었는지 판단
    public bool IsTaskUnlocked(ActiveTaskType type)
    {
        if (_timeViewModel == null)
        {
            return true;
        }
        return _timeViewModel.CurrentDay >= GetTaskUnlockDate(type);
    }

    // 오늘 플레이어/픽서 통틀어 해당 작업이 완수되었는지 확인
    public bool IsTaskClearedToday(ActiveTaskType type)
    {
        if (_miniGameClearedTodayDict.TryGetValue(type, out bool cleared))
        {
            return cleared;
        }
        return false;
    }
    

    // 플레이어 구간
    public bool CanPlayMiniGame(ActiveTaskType type, out string reason)
    {
        reason = string.Empty;

        // 날짜 해금 여부 체크
        if (!IsTaskUnlocked(type))
        {
            int unlockDay = GetTaskUnlockDate(type);
            reason = $"해당 작업은 {unlockDay}일차에 해금됩니다.";
            return false;
        }

        // 픽서 전용 작업 여부 체크 (파밍/음식 등)
        if (type > ActiveTaskType.RouteControl)
        {
            reason = "해당 작업은 픽서 전용 작업입니다.";
            return false;
        }

        // 당일 완료 여부 체크 (픽서 or 플레이어가 이미 클리어함)
        if (IsTaskClearedToday(type))
        {
            reason = "오늘 이미 수리가 완료된 구역입니다!";
            return false;
        }

        // 현재 픽서 작업 중 여부 체크
        if (IsTaskCurrentlyAssigned(type))
        {
            reason = "현재 픽서가 작업 중인 구역입니다.";
            return false;
        }
        string requiredResourceId = GetRequiredResourceId(type);
        if (!string.IsNullOrEmpty(requiredResourceId))
        {
            int requiredCost = GetRequiredResourcesCost(type);
            int currentResourceCount = GetInventoryItemCount(requiredResourceId);

            if (currentResourceCount < requiredCost)
            {
                reason = $"수리에 필요한 자원이 부족합니다! ({currentResourceCount}/{requiredCost})";
                return false;
            }
        }

        return true;

    }

    public bool IsPlayerMiniGame(ActiveTaskType type)
    {
        return CanPlayMiniGame(type, out _);
    }

    public void OnPlayerMiniGameResult(ActiveTaskType type, bool isSuccess)
    {
        if (!IsPlayerMiniGame(type))
        {
            return;
        }
        if (!isSuccess)
        {
            return;
        }

        string requiredResourceId = GetRequiredResourceId(type);
        if (!string.IsNullOrEmpty(requiredResourceId))
        {
            int requiredCost = GetRequiredResourcesCost(type);
            ConsumeInventoryItem(requiredResourceId, requiredCost);
        }

        // 플레이어 성공 시: 당일 클리어 플래그 설정 및 기지 수리 진척도 가산
        _miniGameClearedTodayDict[type] = true;
        AddSystemProgress(type, miniGameSuccessRestoreValue);
        Debug.Log($"[플레이어] {type} 미니게임 완수! 수리 진척도 +{miniGameSuccessRestoreValue}% (오늘 완료 처리됨)");
    }

    // 픽서 구간

    // 픽서에게 작업 배정 (해금 날짜 및 오늘 완료 여부 검사)
    public void AssignFixerToTask(ActiveTaskType taskType, int fixerId, float actualTimeTaken)
    {
        // 해금 날짜 조건 검사 (3/4일차 해금 파밍 등 차단)
        if (!IsTaskUnlocked(taskType))
        {
            int unlockDay = GetTaskUnlockDate(taskType);
            Debug.LogWarning($"[{taskType}] 작업은 {unlockDay}일차 해금 항목입니다. 배정할 수 없습니다.");
            return;
        }

        // 오늘 이미 완료된 작업이면 배정 불가
        if (IsTaskClearedToday(taskType))
        {
            Debug.LogWarning($"오늘 이미 완료된 작업([{taskType}])에는 픽서를 배정할 수 없습니다.");
            return;
        }

        if (!_assignmentDict.ContainsKey(taskType))
        {
            _assignmentDict[taskType] = new ActiveAssignment();
        }

        ActiveAssignment assignment = _assignmentDict[taskType];
        assignment.TaskType = taskType;
        assignment.FixerID = fixerId;
        assignment.IsWorking = true;
        assignment.CurrentWorkTime = 0f;

        assignment.TotalTimeTaken = actualTimeTaken;

        Debug.Log($"픽서({fixerId})가 [{taskType}] 작업장에 배정되었습니다. (효율이 적용된 소요 시간: {actualTimeTaken}초)");
    }



    public float GetFixerWorkProgressRatio(ActiveTaskType taskType)
    {
        if (_assignmentDict.TryGetValue(taskType, out ActiveAssignment info) && info.IsWorking)
        {
            return info.WorkProgressRatio;
        }
        return 0f;
    }

    public void OnFixerWorkCompleted(ActiveTaskType taskType, string fixerId)
    {
        // 픽서 배정 상태 해제
        if (_assignmentDict.ContainsKey(taskType))
        {
            _assignmentDict[taskType].IsWorking = false;
        }

        // 오늘 완료 플래그 설정 (누가 하든 1회 완료)
        _miniGameClearedTodayDict[taskType] = true;

        // 1~4번 수리 작업인 경우 수리 진척도 가산
        if (taskType <= ActiveTaskType.RouteControl)
        {
            float repairStat = GetFixerRepairStat(taskType, fixerId);
            float finalAmount = fixerBaseRepairValue * (repairStat / 100f);
            AddSystemProgress(taskType, finalAmount);

            Debug.Log($"픽서({fixerId}) [{taskType}] 수리 완료! 수리 진척도 +{finalAmount}% (오늘 완료 처리됨)");
        }
        else
        {

            Debug.Log($"픽서({fixerId}) [{taskType}] 업무 완료 ➔ 정산 및 아이템 지급 처리");

            string dataId = GetActiveDataId(taskType);
            if (GameDataManager.Instance != null)
            {
                ActiveData activeData = GameDataManager.Instance.GetActiveData(dataId);

                if (activeData != null && !string.IsNullOrEmpty(activeData.ItemId))
                {
                    string[] possibleItems = activeData.ItemId.Split(',');

                    List<ItemSlotViewModel> resultList = new List<ItemSlotViewModel>();

                    if (taskType == ActiveTaskType.Farming)
                    {
                        foreach (string itemStr in possibleItems)
                        {
                            string selectedItem = itemStr.Trim();
                            int rewardAmount = UnityEngine.Random.Range(100, 131); 

                            NetworkManager.Inst.InventoryService?.AddItem(selectedItem, rewardAmount);

                            resultList.Add(new ItemSlotViewModel { ItemDataId = selectedItem, ItemStackCount = rewardAmount });
                        }
                    }
                    else if (taskType == ActiveTaskType.FoodSupply)
                    {
                        string selectedItem = possibleItems[0].Trim();
                        int rewardAmount = 2;

                        NetworkManager.Inst.InventoryService?.AddItem(selectedItem, rewardAmount);

                        resultList.Add(new ItemSlotViewModel { ItemDataId = selectedItem, ItemStackCount = rewardAmount });
                    }

                    var popupUI = UIManager.Instance.OpenUI(UIRootType.VeryFrontUI, UIType.JobcompletedPopupUI) as JobcompletedPopupUI;
                    if (popupUI != null)
                    {
                        string taskNameStr = taskType == ActiveTaskType.Farming ? "자원 파밍" : "음식 보급";

                        popupUI.SetResultData(taskNameStr, resultList);
                    }
                }
            }
        }
    }

    

    public void OnHourPassed()
    {
        for (int i = 0; i <= (int)ActiveTaskType.RouteControl; i++)
        {
            ActiveTaskType repairTask = (ActiveTaskType)i;

            float currentProgress = GetSystemProgress(repairTask);
            float consumeAmount = GetTimeToConsumeValue(repairTask);

            // 1시간당 차감 적용 (0% ~ 100% 범주 유지)
            _systemProgressDict[repairTask] = Mathf.Clamp(currentProgress - consumeAmount, 0f, 100f);
        }
        Debug.Log("인게임 1시간 경과: 엑셀 TimeToConsume 수치만큼 기지 수리 진척도가 감소했습니다.");
    }

    private void HandleNewDay()
    {
        for (int i = 0; i <= (int)ActiveTaskType.RouteControl; i++)
        {
            ActiveTaskType repairTask = (ActiveTaskType)i;
            if (!_miniGameClearedTodayDict[repairTask])
            {
                TriggerGameOver($"하루 동안 필수 수리 작업 [{repairTask}]을 완수하지 못했습니다!");
                return;
            }
        }

        // 2. 당일 클리어 플래그 초기화
        Array types = Enum.GetValues(typeof(ActiveTaskType));
        foreach (ActiveTaskType type in types)
        {
            _miniGameClearedTodayDict[(ActiveTaskType)type] = false;
        }

        Debug.Log("모든 미니게임을 완수하여 다음 날로 넘어가면서 진행도가 감소.");
    }

    private void TriggerGameOver(string reason)
    {
        Debug.LogError("[GAME OVER] " + reason);
        // TODO: 게임오버 UI 팝업 연동 코드 작성 영역
    }

    // 기지 수리 진척도 조회
    public float GetSystemProgress(ActiveTaskType type)
    {
        if (_systemProgressDict.TryGetValue(type, out float val))
        {
            return val;
        }
        return 0f;
    }

    // 기지 수리 진척도 가산
    public void AddSystemProgress(ActiveTaskType type, float amount)
    {
        if (!_systemProgressDict.ContainsKey(type)) return;

        float current = _systemProgressDict[type];
        _systemProgressDict[type] = Mathf.Clamp(current + amount, 0f, 100f);
    }


    // 픽서 엑셀 데이터(FixerModel)의 수리 배율(O2Repair, ElectRepair 등) 매핑
    private float GetFixerRepairStat(ActiveTaskType type, string fixerId)
    {
        if (GameDataManager.Instance != null && !string.IsNullOrEmpty(fixerId))
        {
            FixerData fixerData = GameDataManager.Instance.GetFixerData(fixerId);
            if (fixerData != null)
            {
                switch (type)
                {
                    case ActiveTaskType.OxygenSupply: return fixerData.O2Repair;
                    case ActiveTaskType.PowerSupply: return fixerData.ElectRepair;
                    case ActiveTaskType.TemperatureControl: return fixerData.TempRepair;
                    case ActiveTaskType.RouteControl: return fixerData.ControlRepair;
                    default: return 100f;
                }
            }
        }

        return 100f;
    }

    // 특정 작업장에 현재 일하고 있는 픽서가 있는지 확인
    public bool IsTaskCurrentlyAssigned(ActiveTaskType taskType)
    {
        if (_assignmentDict.TryGetValue(taskType, out ActiveAssignment info))
        {
            return info.IsWorking;
        }
        return false;
    }

    // 특정 작업장에 배정된 픽서 ID(string) 가져오기
    public int GetAssignedFixerId(ActiveTaskType taskType)
    {
        if (_assignmentDict.TryGetValue(taskType, out ActiveAssignment info))
        {
            if (info.IsWorking)
            {
                return info.FixerID;
            }
        }
        return -1; // -1 = 값이 없음(null 대신 사용)
    }

    // 해당 픽서가 어느 작업장에서든 작업 중인지 확인 (중복 배정 방지용)
    public bool IsFixerWorkingAnywhere(int fixerId)
    {
        foreach (KeyValuePair<ActiveTaskType, ActiveAssignment> pair in _assignmentDict)
        {
            ActiveAssignment info = pair.Value;
            if (info.FixerID == fixerId && info.IsWorking)
            {
                return true;
            }
        }
        return false;
    }
    public void CancelFixerTask(ActiveTaskType taskType)
    {
        if (_assignmentDict.ContainsKey(taskType) && _assignmentDict[taskType].IsWorking)
        {
            _assignmentDict[taskType].IsWorking = false;

            Debug.Log($"[{taskType}] 작업장의 픽서 배정이 해제되었습니다.");
        }
    }
}
