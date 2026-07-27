using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EndingTriggerSpot : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("엔딩이 발동될 목표 일수")]
    [SerializeField] private int _targetDay = 6;

    private bool _hasTriggered = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered || !other.CompareTag("Player")) return;

        if (NetworkManager.Inst == null || NetworkManager.Inst.TimeService == null) return;

        var timeVM = NetworkManager.Inst.TimeService.GetViewModel();
        if (timeVM == null) return;

        if (timeVM.CurrentDay >= _targetDay)
        {
            _hasTriggered = true; 

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OpenEndingVideoPlayerUI();
            }
        }
    }
}