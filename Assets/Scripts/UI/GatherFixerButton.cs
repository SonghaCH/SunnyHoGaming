using UnityEngine;

public class GatherFixerButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIButton _btnGatherAll;

    private void Start()
    {
        if (_btnGatherAll != null)
        {
            _btnGatherAll.BindOnClickButtonEvent(OnClickGatherButton);
        }
    }

    private void OnClickGatherButton()
    {
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.GatherAllFixersToMainRoom();
        }
        else
        {
            Debug.LogWarning("[GatherButtonUI] WorldManager 인스턴스가 존재하지 않아 집합 명령을 내릴 수 없습니다.");
        }
    }
}
