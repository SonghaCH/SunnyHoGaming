using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class WorkManager : MonoBehaviour
{
    public static WorkManager Instance { get; private set; }

    public List<WorkStation> AllWorkStations { get; private set; } = new List<WorkStation>();
    private Dictionary<FixerViewModel, CancellationTokenSource> _workCancellationTokens = new Dictionary<FixerViewModel, CancellationTokenSource>();

    private struct WorkOrder
    {
        public WorkStation Station;
        public float Duration;
    }
    private Dictionary<FixerViewModel, WorkOrder> _pendingOrders = new Dictionary<FixerViewModel, WorkOrder>();
    public event Action OnWorkStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterWorkStations(WorkStation[] stations)
    {
        AllWorkStations.Clear();
        AllWorkStations.AddRange(stations);
    }

    public void AssignSpecificTask(FixerViewModel fixer, WorkStation targetStation, float workDuration)
    {
        if (targetStation.IsOccupied)
        {
            Debug.LogWarning("이미 점유된 작업장입니다.");
            return;
        }

        if (fixer.TargetStation != null)
        {
            CancelSpecificTask(fixer, fixer.TargetStation);
        }

        ActiveManager.Instance.AssignFixerToTask(targetStation.TaskType, fixer.InstanceId);

        fixer.TargetStation = targetStation;
        targetStation.AssignTaskToFixer(fixer);
        targetStation.LockStation(); 

        _pendingOrders[fixer] = new WorkOrder { Station = targetStation, Duration = workDuration };

        fixer.OnArrivedAtWorkStation -= OnFixerArrived;
        fixer.OnArrivedAtWorkStation += OnFixerArrived;

        OnWorkStateChanged?.Invoke();
    }

    private void OnFixerArrived(FixerViewModel fixer)
    {
        fixer.OnArrivedAtWorkStation -= OnFixerArrived;

        if (_pendingOrders.TryGetValue(fixer, out WorkOrder order))
        {
            _pendingOrders.Remove(fixer); 

            ProcessWorkTickAsync(fixer, order.Station, order.Duration).Forget();
        }
    }

    private async UniTaskVoid ProcessWorkTickAsync(FixerViewModel fixer, WorkStation station, float workDuration)
    {
        var cts = new CancellationTokenSource();
        _workCancellationTokens[fixer] = cts;

        try
        {
            float repairPower = fixer.GetWorkEfficiency(station.StationWorkType);
            if (repairPower <= 0f) repairPower = 1f;

            await UniTask.Delay(TimeSpan.FromSeconds(workDuration), cancellationToken: cts.Token);

            
            float totalWorkPower = repairPower * (workDuration / 1.5f);
            station.ApplyWork(totalWorkPower); 

            int currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;
            station.MarkWorkedCompleted(currentDay);

            ActiveManager.Instance.OnMiniGameResult(station.TaskType, true);

            Debug.Log($"[{fixer.gameObject.name}] {station.gameObject.name} 작업 완료! (1일 제한 적용)");
        }
        catch (OperationCanceledException)
        {
            
        }
        finally
        {
            ActiveManager.Instance.CancelFixerTask(station.TaskType);
            station.UnlockStation(); 
            
            _workCancellationTokens.Remove(fixer);
            fixer.TargetStation = null; 
            fixer.OrderReturn();

            OnWorkStateChanged?.Invoke();
        }
    }

    public void CancelWork(FixerViewModel fixer)
    {
        if (_workCancellationTokens.TryGetValue(fixer, out var cts))
        {
            cts.Cancel();
        }

        _pendingOrders.Remove(fixer);
        fixer.OnArrivedAtWorkStation -= OnFixerArrived;
    }

    public void CancelSpecificTask(FixerViewModel fixer, WorkStation station)
    {
        ActiveManager.Instance.CancelFixerTask(station.TaskType);

        CancelWork(fixer); 
        station.UnlockStation(); 

        fixer.TargetStation = null;

        OnWorkStateChanged?.Invoke();
    }

    public void CancelTaskByStation(WorkStation station)
    {
        int fixerId = ActiveManager.Instance.GetAssignedFixerId(station.TaskType);

        if (fixerId != -1)
        {
            FixerViewModel fixer = GameObjectManager.Instance.GetFixerFromInstanceId(fixerId);
            if (fixer != null)
            {
                CancelSpecificTask(fixer, station);
                Debug.Log($"플레이어가 작업을 끝내서 픽서({fixerId})의 작업을 취소하고 돌려보냅니다.");
            }
        }
    }
}