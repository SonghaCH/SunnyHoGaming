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

    public void AssignSpecificTask(FixerViewModel fixer, WorkStation targetStation, float baseWorkDuration)
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

        float efficiency = fixer.GetWorkEfficiency(targetStation.StationWorkType);
        if (efficiency <= 0f) efficiency = 100f;

        float efficiencyMultiplier = efficiency / 100f;
        float actualDuration = baseWorkDuration / efficiencyMultiplier;

        ActiveManager.Instance.AssignFixerToTask(targetStation.TaskType, fixer.InstanceId, actualDuration);

        fixer.TargetStation = targetStation;
        targetStation.AssignTaskToFixer(fixer, actualDuration);
        targetStation.LockStation();

        _pendingOrders[fixer] = new WorkOrder { Station = targetStation, Duration = actualDuration };

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

            ActiveManager.Instance.OnFixerWorkCompleted(station.TaskType, fixer.DataId);

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
                Debug.Log($"작업장({station.gameObject.name})의 픽서({fixerId}) 강제 작업 취소 및 복귀 처리 완료.");
            }
        }
    }
}