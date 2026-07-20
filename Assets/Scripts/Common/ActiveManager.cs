using System;
using System.Collections.Generic;
using UnityEngine;



public enum ActiveTaskType
{
    OxygenSupply,  //active_01
    PowerSupply,   //active_02
    TemperatureControl, //active_03
    RouteControl  //active_04
}


// 확인용으로 [Serializable] 붙였음 나중에 없앨 예정
[Serializable]
public class ActiveAssignment
{
    public ActiveTaskType TaskType;  // 수행 중인 작업 종류
    public int FixerID;              // 배정된 픽서 ID (FixerModel.FixerID)
    public bool IsWorking;           // 현재 작업 중인지 여부 (true: 작업 중, false: 대기 중)
}


public class ActiveManager : MonoBehaviour
{
    public static ActiveManager Instance { get; private set; }

    [Header("기본 밸런스 설정값")]
    [SerializeField] private float miniGameSuccessRestoreValue = 25f; // 미니게임 성공 시 복구량
    [SerializeField] private float fixerBaseRepairValue = 20f;  // 픽서 수리 기본 복구량

    // 시스템별 현재 진행도 저장 (Key: TaskType, Value: 0~100% 진척도 수치)
    private Dictionary<ActiveTaskType, float> _systemProgressDict = new Dictionary<ActiveTaskType, float>();

    // 당일 미니게임 성공 클리어 여부(Key: TaskType, Value: true=클리어 / false=미클리어)
    private Dictionary<ActiveTaskType, bool> _miniGameClearedTodayDict = new Dictionary<ActiveTaskType, bool>();

    // 각 수리 구역별 픽서 배정 현황 (Key: TaskType, Value: 배정 상태 객체)
    private Dictionary<ActiveTaskType, ActiveAssignment> _assignmentDict = new Dictionary<ActiveTaskType, ActiveAssignment>();

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

    private void OnEnable()
    {
        TimeViewModel.OnDayChanged += HandleNewDay;
    }

    private void OnDisable()
    {
        TimeViewModel.OnDayChanged -= HandleNewDay;
    }

    private void InitializeData()
    {
        _systemProgressDict.Clear();
        _miniGameClearedTodayDict.Clear();
        _assignmentDict.Clear();

        // Enum에 등록된 4개 타입(Oxygen, Power, Temp, Route)을 순회하며 초기화
        Array types = Enum.GetValues(typeof(ActiveTaskType));
        foreach (ActiveTaskType type in types)
        {
            _systemProgressDict.Add(type, 100f); // 초기 진행도 100% 시작
            _miniGameClearedTodayDict.Add(type, false); // 하루 시작 시 미클리어 상태
        } 
    }

    private void HandleNewDay()
    {
        Array types = Enum.GetValues(typeof(ActiveTaskType));


        // 4가지 미니게임을 전부 클리어하지 못하면 게임오버
        foreach (ActiveTaskType type in types)
        {
            if (!_miniGameClearedTodayDict[type])
            {
                // 하나라도 클리어(true)가 안 되어있다면 게임오버 실행 후 함수 즉시 종료
                TriggerGameOver("하루 동안 " + type.ToString() + " 미니게임을 완료하지 못했습니다!");
                return;
            }
        }

        // 모두 클리어 시 다음 날 위해 미니게임 기회 초기화
        foreach (ActiveTaskType type in types)
        {
            _miniGameClearedTodayDict[type] = false;
        }

        // 진행도가 100% 되더라도 하루에 일정량(TimeToConsume = 3) 감소
        foreach (ActiveTaskType type in types)
        {
            float consumeAmount = GetTimeToConsumeValue(type);
            float currentProgress = _systemProgressDict[type];

            float nextProgress = Mathf.Clamp(currentProgress - consumeAmount, 0f, 100f);
            _systemProgressDict[type] = nextProgress;
        }

        Debug.Log("모든 미니게임을 완수하여 다음 날로 넘어가면서 진행도가 감소.");
    }

    private void TriggerGameOver(string reason)
    {
        Debug.LogError("★ [GAME OVER] " + reason);
        // TODO: 게임오버 UI 팝업 연동 코드 작성 영역
    }

    public bool CanPlayMiniGame(ActiveTaskType type)
    {
        if (_miniGameClearedTodayDict.ContainsKey(type))
        {
            // 오늘 이미 성공(true)했다면 false 반환 (더 이상 오늘 플레이 불가)
            return !_miniGameClearedTodayDict[type];
        }

        return true;
    }

    // AirMiniGame, ElecMiniGame 등이 끝날 때 결과 알림
    public void OnMiniGameResult(ActiveTaskType type, bool isSuccess)
    {

        // 미니게임 실패 시: 수리량 상승 없고 성공 플래그도 세우지 않음 -> 재도전 가능
        if (!isSuccess)
        {
            Debug.Log(type.ToString() + " 미니게임 실패! 성공할 때까지 다시 도전할 수 있습니다.");
            return;
        }

        //이미 클리어한 게임인데 중복 호출된 경우 예외 처리
        if (!CanPlayMiniGame(type))
        {
            Debug.LogWarning("오늘 이미 클리어한 미니게임입니다: " + type.ToString());
            return;
        }

        //미니게임 성공 시: 하루 1회 클리어 처리 + 진행도 상승
        _miniGameClearedTodayDict[type] = true;
        AddProgress(type, miniGameSuccessRestoreValue);

        Debug.Log(type.ToString() + " 미니게임 성공! 오늘 성공 완료 처리됨.");
    }

    // WorkPopupUI / FixerPopupUI에서 픽서에게 작업 명령을 내릴 때 호출
    public void AssignFixerToTask(ActiveTaskType taskType, int fixerId)
    {
        if (!_assignmentDict.ContainsKey(taskType))
        {
            _assignmentDict[taskType] = new ActiveAssignment();
        }

        //배정 정보 업데이트
        ActiveAssignment assignment = _assignmentDict[taskType];
        assignment.TaskType = taskType;
        assignment.FixerID = fixerId;
        assignment.IsWorking = true; // 현재 작업 중인 상태로 설정 (중복 배정 방지)

        Debug.Log("픽서(" + fixerId + ")가 " + taskType.ToString() + " 작업에 배정되었습니다.");
    }

    //  Fixer_RepairAction 노드(BT)가 끝나는 시점에 호출
    public void OnFixerRepairCompleted(ActiveTaskType taskType, FixerViewModel fixerViewModel)
    {
        if (fixerViewModel == null)
        {
            return;
        }

        // 픽서 수리 배율(O2Repair = 100, WayRepair = 150 등)
        float repairStat = GetFixerRepairStat(taskType, fixerViewModel.FixerModel);

        //공식 적용: [기본 수리량 * (스탯 배율 / 100)]
        float finalAmount = fixerBaseRepairValue * (repairStat / 100f);

        // 수리가 끝났으면  해당 구역 픽서 배정 해제 (IsWorking = false)
        if (_assignmentDict.ContainsKey(taskType))
        {
            _assignmentDict[taskType].IsWorking = false;
        }

        Debug.Log("픽서(" + fixerViewModel.FixerModel.FixerID + ") 수리 완료! 가산 진행도: " + finalAmount);
    }

    // WorkPopupUI에서 "현재 이 작업장에 일하고 있는 픽서가 있는가?" 확인할 때
    public bool IsTaskCurrentlyAssigned(ActiveTaskType taskType)
    {
        if (_assignmentDict.TryGetValue(taskType, out ActiveAssignment info))
        {
            return info.IsWorking;
        }
        return false;
    }

    // 특정 작업장에 배정된 픽서 ID 가져오기
    public int GetAssignedFixerId(ActiveTaskType taskType)
    {
        if (_assignmentDict.TryGetValue(taskType, out ActiveAssignment info))
        {
            if (info.IsWorking)
            {
                return info.FixerID;
            }
        }
        return -1;
    }

    // FixerPopupUI에서 "이 픽서가 다른 작업 구역에서 일하고 있는가?" 검사할 때 (중복 배정 막기)
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

    // UI에서 현재 진척도(0~100%) 게이지 바를 그릴 때
    public float GetProgress(ActiveTaskType type)
    {
        if (_systemProgressDict.TryGetValue(type, out float val))
        {
            return val;
        }
        return 0f;
    }

    // 진행도 가산 메서드
    public void AddProgress(ActiveTaskType type, float amount)
    {
        if (!_systemProgressDict.ContainsKey(type))
        {
            _systemProgressDict[type] = 0f;
        }

        float current = _systemProgressDict[type];
        _systemProgressDict[type] = Mathf.Clamp(current + amount, 0f, 100f);
    }

    // 엑셀 active_01 ~ active_04의 TimeToConsume(감소 수치: 3) 매핑
    private float GetTimeToConsumeValue(ActiveTaskType type)
    {
        switch (type)
        {
            case ActiveTaskType.OxygenSupply:
                return 3f;
            case ActiveTaskType.PowerSupply:
                return 3f;
            case ActiveTaskType.TemperatureControl:
                return 3f;
            case ActiveTaskType.RouteControl:
                return 3f;
            default:
                return 3f;
        }
    }

    // 픽서 엑셀 데이터(FixerModel)의 수리 배율(O2Repair, ElectRepair 등) 매핑
    private float GetFixerRepairStat(ActiveTaskType type, FixerModel model)
    {
        if (model == null) return 100f;

        switch (type)
        {
            case ActiveTaskType.OxygenSupply:
                return model.O2Repair;      // 예: 픽스 = 100
            case ActiveTaskType.PowerSupply:
                return model.ElectRepair;   // 예: 볼트 = 150
            case ActiveTaskType.TemperatureControl:
                return model.TempRepair;    // 예: 암스트롱 = 150
            case ActiveTaskType.RouteControl:
                return model.ControlRepair; // 예: 픽스 = 150
            default:
                return 100f;
        }
    }

}
