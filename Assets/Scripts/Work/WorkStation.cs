using UnityEngine;

public abstract class WorkStation : MonoBehaviour
{
    public string FieldObjectId;
    private int _lastWorkedDay = -1;
    public WorkType StationTaskType;

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

        if (UserInputManager.instance != null)
        {
            UserInputManager.instance.OnInteractionKey -= Interact;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UserInputManager.instance != null)
            {
                UserInputManager.instance.OnInteractionKey += Interact;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UserInputManager.instance != null)
            {
                UserInputManager.instance.OnInteractionKey -= Interact;
            }
        }
    }

    private void Interact()
    {
        if (FixerInteractController.FixersInRange > 0)
        {
            return;
        }
       
        Debug.Log($"[{gameObject.name}] 작업대 상호작용 실행!");
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

    public void AssignTaskToFixer(FixerViewModel fixerViewModel)
    {
        fixerViewModel.SetWorkTarget(this.transform.position, this.StationTaskType);

        fixerViewModel.CurrentState = FixerState.MoveToTarget;
    }
    public bool CanWorkToday(int currentDay)
    {
        // 누군가 점유 중이 아니고, 마지막 작업일이 오늘이 아니라면 작업 가능
        return !IsOccupied && _lastWorkedDay != currentDay;
    }

    public void MarkWorkedCompleted(int currentDay)
    {
        _lastWorkedDay = currentDay;
    }

    public void LockStation() { IsOccupied = true; }
    public void UnlockStation() { IsOccupied = false; }
}