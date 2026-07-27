using UnityEngine;

public class QuestTargetMarkerHandler : MonoBehaviour
{
    [SerializeField] private ActiveTaskType _taskType;

    private void Start()
    {
        if (ActiveManager.Instance != null)
        {
            ActiveManager.Instance.OnActiveDataChanged += OnActiveDataChanged;
        }
    }

    private void OnDestroy()
    {
        if (ActiveManager.Instance != null)
        {
            ActiveManager.Instance.OnActiveDataChanged -= OnActiveDataChanged;
        }
    }

    private void OnActiveDataChanged()
    {
        if (ActiveManager.Instance == null)
        {
            return;
        }

        bool isCleared = ActiveManager.Instance.IsTaskClearedToday(_taskType);

        if (isCleared == true)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.RemoveQuestTargetMarker(transform);
            }
        }
    }
}