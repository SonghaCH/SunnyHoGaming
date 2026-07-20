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

        targetStation.LockStation();
        fixer.SetWorkTarget(targetStation.transform.position, targetStation.StationTaskType);

        fixer.CurrentState = FixerState.MoveToTarget;

        _pendingOrders[fixer] = new WorkOrder { Station = targetStation, Duration = workDuration };

        fixer.OnArrivedAtWorkStation -= OnFixerArrived;
        fixer.OnArrivedAtWorkStation += OnFixerArrived;
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
            float repairPower = fixer.GetWorkEfficiency(station.StationTaskType);
            if (repairPower <= 0f) repairPower = 1f;

            // 1. 실시간 틱(1.5초 루프) 제거. 전체 작업 시간(workDuration)만큼 대기합니다.
            await UniTask.Delay(TimeSpan.FromSeconds(workDuration), cancellationToken: cts.Token);

            // ---------- [대기 종료 = 작업 완료] ----------

            // 2. 작업이 100% 완료된 시점에 한 번에 작업량(보상) 증가
            // (기존 1.5초 주기였던 것을 감안하여 총합 workPower를 계산하거나 기획 수치에 맞게 변경)
            float totalWorkPower = repairPower * (workDuration / 1.5f);
            station.ApplyWork(totalWorkPower); // 파밍 시설은 여기서 아이템을 1회 지급하고 끝납니다[cite: 7, 8].

            // 3. 일일 1회 제한 적용 (오늘 날짜를 기록하여 CanWorkToday를 false로 만듦)
            int currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;
            station.MarkWorkedCompleted(currentDay);

            Debug.Log($"[{fixer.gameObject.name}] {station.gameObject.name} 작업 완료! (1일 제한 적용)");
        }
        catch (OperationCanceledException)
        {
            
        }
        finally
        {
            station.UnlockStation(); // IsOccupied = false 처리
            _workCancellationTokens.Remove(fixer);
            fixer.OrderReturn();
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
}