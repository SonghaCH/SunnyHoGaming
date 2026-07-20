using UnityEngine;

public abstract class WorkStation : MonoBehaviour
{
    public string FieldObjectId;
    private int _lastWorkedDay = -1;
    public WorkType StationWorkType;

    public ActiveTaskType TaskType;

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
        if (FixerInteractController.FixersInRange.Count > 0)
        {
            return;
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

    public void AssignTaskToFixer(FixerViewModel fixerViewModel)
    {
        fixerViewModel.SetWorkTarget(this.transform.position, this.StationWorkType);
        fixerViewModel.CurrentState = FixerState.MoveToTarget;
    }

    public bool CanWorkToday(int currentDay)
    {
        return !IsOccupied && _lastWorkedDay != currentDay;
    }

    public void MarkWorkedCompleted(int currentDay)
    {
        _lastWorkedDay = currentDay;
    }

    public void LockStation() { IsOccupied = true; }
    public void UnlockStation() { IsOccupied = false; }
}