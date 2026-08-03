using Cysharp.Threading.Tasks;
using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [Header("Cheat Settings")]
    [Tooltip("체크 해제 시 치트키 미작동")]
    [SerializeField] private bool _enableCheat = true;

    private void Update()
    {
        if (!_enableCheat) return;

        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha1)) || Input.GetKeyDown(KeyCode.F1))
        {
            Cheat_ForcePassDay();
        }

        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2)) || Input.GetKeyDown(KeyCode.F2))
        {
            Cheat_CompleteAllQuests();
        }

        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha3)) || Input.GetKeyDown(KeyCode.F3))
        {
            Cheat_MaxOutAllActiveSystems();
        }

        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha4)) || Input.GetKeyDown(KeyCode.F4))
        {
            Cheat_WarpToDay6();
        }
    }

    private void Cheat_ForcePassDay()
    {
        if (NetworkManager.Inst == null) return;

        if (NetworkManager.Inst.PlayerService != null)
        {
            NetworkManager.Inst.PlayerService.Sleep();
        }

        var statusVM = NetworkManager.Inst.PlayerService?.GetStatusViewModel();
        if (statusVM != null)
        {
            statusVM.Hunger = 100f;
        }

        if (NetworkManager.Inst.TimeService != null)
        {
            NetworkManager.Inst.TimeService.SkipToNextDay();
            Debug.Log("[Cheat 1] 다음 날로 스킵 및 일차 변경 완료!");
        }

        RestorePlayerControlAsync().Forget();
    }

    private async UniTaskVoid RestorePlayerControlAsync()
    {
        await UniTask.DelayFrame(3);

        if (NetworkManager.Inst != null && NetworkManager.Inst.PlayerService != null)
        {
            NetworkManager.Inst.PlayerService.WakeUp();
            NetworkManager.Inst.PlayerService.SetCanMove(true);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseFPopupUI();
        }
    }
    private void Cheat_CompleteAllQuests()
    {
        if (QuestManager.Instance == null) return;

        int currentDay = 1;
        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;
        }

        if (QuestManager.Instance.activeQuests != null)
        {
            for (int i = 0; i < QuestManager.Instance.activeQuests.Count; i++)
            {
                var quest = QuestManager.Instance.activeQuests[i];

                if (quest.UnlockDay <= currentDay)
                {
                    if (quest.subTaskList != null)
                    {
                        for (int j = 0; j < quest.subTaskList.Count; j++)
                        {
                            var subTask = quest.subTaskList[j];
                            subTask.isCompleted = true;
                            quest.subTaskList[j] = subTask; 
                        }
                    }
                }
            }
        }

        Debug.Log($"[Cheat 2] {currentDay}일차 당일 퀘스트 완료 처리!");
    }

    private void Cheat_MaxOutAllActiveSystems()
    {
        if (ActiveManager.Instance == null) return;

        for (int i = 0; i <= (int)ActiveTaskType.RouteControl; i++)
        {
            ActiveTaskType type = (ActiveTaskType)i;
            ActiveManager.Instance.ForceSetSystemProgress(type, 100f);
        }

        ActiveManager.Instance.RefreshAllActiveObjects();
        Debug.Log("[Cheat 3] 모든 액티브 Progress 100%!");
    }

    private void Cheat_WarpToDay6()
    {
        if (NetworkManager.Inst != null && NetworkManager.Inst.TimeService != null)
        {
            var timeVM = NetworkManager.Inst.TimeService.GetViewModel();
            if (timeVM != null)
            {
                timeVM.CurrentDay = 6;
                Debug.Log($"[Cheat 4] 6일차 이동 완료! (CurrentDay: {timeVM.CurrentDay})");
            }
        }
    }
}