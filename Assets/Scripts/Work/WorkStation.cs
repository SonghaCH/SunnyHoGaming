using UnityEngine;

public class WorkStation : MonoBehaviour
{
    public string FieldObjectId;
    public TaskType StationTaskType;

    public float MaxGauge { get; protected set; }
    public float CurrentGauge { get; protected set; }
    public bool IsOccupied { get; private set; }

    public float CurrentProgress
    {
        get
        {
            if (MaxGauge > 0)
            {
                return CurrentGauge / MaxGauge;
            }
            return 0f;
        }
    }

    private void OnEnable()
    {
        if (WorkManager.Instance != null)
        {
            WorkManager.Instance.AllWorkStations.Add(this);
        }
    }

    private void OnDisable()
    {
        if (WorkManager.Instance != null)
        {
            WorkManager.Instance.AllWorkStations.Remove(this);
        }
    }

    public virtual bool ApplyWork(float workPower)
    {
        CurrentGauge += workPower;
        if (CurrentGauge >= MaxGauge)
        {
            CurrentGauge = MaxGauge;
            return true;
        }
        return false;
    }

    public void LockStation() { IsOccupied = true; }
    public void UnlockStation() { IsOccupied = false; }
}