using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.Video;


public enum GameOverReason
{
    None,
    NoSleep,
    QuestFailed
}

[Serializable]
public struct ItemReward
{
    [Header("RewardItem Setting")]
    public string rewardItemId;
    public int rewardAmount;
}

[Serializable]
public struct DailyEventData
{
    [Header("이날 지급할 아이템 목록")]
    public ItemReward[] rewards;
}


public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Inst { get; set; }

    [SerializeField] private DailyEventData[] dailyEventDataList;

    public NetworkInventoryService InventoryService { get; private set; }
    public PlayerService PlayerService { get; private set; }
    public GameStateService GameStateService { get; private set; }
    public TimeService TimeService { get; private set; }

    private bool _isLoading = false;


    private void Awake()
    {
        Inst = this;
        InitNetworkService();
    }

    private void Start()
    {
        if (GameStateService != null)
        {
            GameStateViewModel gameStateViewModel = GameStateService.GetViewModel();

            if (gameStateViewModel != null)
            {
                gameStateViewModel.PropertyChanged += OnGameStateChanged;
            }

            GameStateService.GetViewModel().OnRequestingTitle();
        }

        if (TimeService != null)
        {
            var timeVM = TimeService.GetViewModel();
            if (timeVM != null)
            {
                timeVM.PropertyChanged += OnTimePropertyChanged_Network;
            }
        }
    }

    private void OnDestroy()
    {
        if (TimeService != null)
        {
            var timeVM = TimeService.GetViewModel();
            if (timeVM != null)
            {
                timeVM.PropertyChanged -= OnTimePropertyChanged_Network;
            }
        }
    }

    private void Update()
    {
        if (GameStateService != null)
        {
            GameState currentGameState = GameStateService.GetCurrentState();

            if (currentGameState == GameState.Playing)
            {
                float deltaTime = Time.deltaTime;

                if (TimeService != null)
                {
                    TimeService.UpdateTime(deltaTime);
                }

                if (PlayerService != null && TimeService != null)
                {
                    int currentDay = TimeService.GetViewModel().CurrentDay;
                }
            }
        }
    }

    private void InitNetworkService()
    {
        InventoryService = new NetworkInventoryService();
        TimeService = new TimeService(0.16f);
        PlayerService = new PlayerService();
        GameStateService = new GameStateService();
    }

    private void OnGameStateChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameStateViewModel.CurrentGameState))
        {
            UpdatePlayerCanMove();
        }
    }

    private void UpdatePlayerCanMove()
    {
        if (GameStateService != null && PlayerService != null)
        {
            GameState currentGameState = GameStateService.GetCurrentState();

            if (currentGameState == GameState.Playing)
            {
                PlayerService.SetCanMove(true);
            }
            else
            {
                PlayerService.SetCanMove(false);
            }
        }
    }

    private string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "SaveData.json");
    }


    public void RequestSaveGame()
    {
        PlayerModel saveData = new PlayerModel();

        saveData.CurrentDay = TimeService.GetViewModel().CurrentDay;
        saveData.Hunger = PlayerService.GetStatusViewModel().Hunger;

        var inventory = InventoryService.GetPlayerItemList();
        foreach (var keyValue in inventory)
        {
            saveData.ItemList.Add(new ItemModel
            {
                ItemUniqueId = keyValue.Value.ItemUniqueId,
                ItemDataId = keyValue.Value.ItemDataId,
                ItemStackCount = keyValue.Value.ItemStackCount
            });
        }

        if (GameObjectManager.Instance != null)
        {
            var fixers = GameObjectManager.Instance.FixerObjectContainer;
            if (fixers.Count > 0)
            {
                foreach (var keyValue in fixers)
                {
                    saveData.FixerList.Add(new FixerSaveData
                    {
                        instanceId = keyValue.Value.InstanceId,
                        fixerDataId = keyValue.Value.DataId,
                        lastPosition = keyValue.Value.transform.position,
                        lastState = keyValue.Value.CurrentState
                    });
                }
            }
            else
            {
                Debug.LogWarning("[SaveGame] 저장 시점에 픽서 컨테이너가 비어있습니다.");
            }
        }

        if (ActiveManager.Instance != null)
        {
            for (int i = 0; i <= (int)ActiveTaskType.RouteControl; i++)
            {
                ActiveTaskType type = (ActiveTaskType)i;
                saveData.ActiveProgressList.Add(new ActiveProgressData
                {
                    TaskType = type,
                    Progress = ActiveManager.Instance.GetSystemProgress(type)
                });
            }
        }

        RequstSaveData(saveData);
    }


    public async UniTask RequestLoadGameAsync()
    {
        _isLoading = true;

        try
        {
            if (WorldManager.Instance != null)
            {
                WorldManager.Instance.ClearMap();
            }

            if (GameObjectManager.Instance != null)
            {
                await GameObjectManager.Instance.ClearAllFixersAsync();
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseAllPopups();
            }

            await UniTask.Yield(); 

            PlayerModel loadedData = RequstLoadSaveData();

            TimeService.SetTimeByDay(loadedData.CurrentDay);

            PlayerService.GetStatusViewModel().Hunger = loadedData.Hunger;

            var invenVM = InventoryService.GetLocalInventoryViewModel();
            invenVM.ItemList.Clear();
            foreach (var item in loadedData.ItemList)
            {
                invenVM.ItemList.Add(item.ItemUniqueId, new ItemSlotViewModel
                {
                    ItemUniqueId = item.ItemUniqueId,
                    ItemDataId = item.ItemDataId,
                    ItemStackCount = item.ItemStackCount
                });
            }
            invenVM.RefreshItemList();

            if (ActiveManager.Instance != null)
            {
                foreach (var progressData in loadedData.ActiveProgressList)
                {
                    ActiveManager.Instance.ForceSetSystemProgress(progressData.TaskType, progressData.Progress);
                }
                ActiveManager.Instance.RefreshAllActiveObjects();
            }

            if (WorldManager.Instance != null && loadedData.FixerList.Count > 0)
            {
                WorldManager.Instance.SetPendingFixerData(loadedData.FixerList);
            }
        }
        finally
        {
            _isLoading = false;

            if (TimeService != null && TimeService.GetViewModel() != null)
            {
                ProcessDailyStartEvent(TimeService.GetViewModel().CurrentDay).Forget();
            }
        }
    }

    public void RequestLoadGame()
    {
        RequestLoadGameAsync().Forget();
    }

    public async void RequestNewGame()
    {
        try
        {
            string path = GetPath();
            if (File.Exists(path))
            {
                File.Delete(path);
                await UniTask.Delay(50);
            }

            PlayerModel defaultData = GetDefaultPlayerData();
            RequstSaveData(defaultData);
            RequestLoadGame();

            await RequestLoadGameAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[RequestNewGame] 새 게임 생성 중 에러 발생: {e.Message}");
        }
    }

    public void RequstSaveData(PlayerModel data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(), json);
    }

    public PlayerModel RequstLoadSaveData()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerModel data = JsonUtility.FromJson<PlayerModel>(json);
            return data;
        }
        else
        {
            var playerData = GetDefaultPlayerData();
            RequstSaveData(playerData);
            return playerData;
        }
    }

    public PlayerModel GetDefaultPlayerData()
    {
        var newPlayerData = new PlayerModel();
        newPlayerData.CurrentDay = 1;
        newPlayerData.Hunger = 100f;

        for (int i = 0; i <= (int)ActiveTaskType.RouteControl; i++)
        {
            newPlayerData.ActiveProgressList.Add(new ActiveProgressData { TaskType = (ActiveTaskType)i, Progress = 100f });
        }
        return newPlayerData;
    }

    private async void OnTimePropertyChanged_Network(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimeViewModel.CurrentDay))
        {
            if (_isLoading) return;

            await UniTask.NextFrame();

            int currentDay = TimeService.GetViewModel().CurrentDay;

            if (CheckGameClear(currentDay))
            {
                SetGameClear();
                return;
            }

            int failedDay = currentDay - 1;
            GameOverReason reason = CheckGameOver(failedDay);

            if (reason != GameOverReason.None)
            {
                await SetGameOver(failedDay, reason);
                return;
            }

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.ResetDailyQuests();
            }

            while (WorldManager.Instance == null || WorldManager.Instance.IsSpawnCompleted == false)
            {
                await UniTask.Yield();
            }

            await UniTask.Yield();

            RequestSaveGame();

            ProcessDailyStartEvent(currentDay).Forget();
        }
    }

    private bool CheckGameClear(int currentDay)
    {
        int clearDay = 7;
        return currentDay >= clearDay;
    }

    private GameOverReason CheckGameOver(int failedDay)
    {
        if (QuestManager.Instance != null)
        {
            foreach (var quest in QuestManager.Instance.activeQuests)
            {
                if (quest.UnlockDay <= failedDay && (quest.Type == "Main" || quest.Type == "Daily"))
                {
                    foreach (var subTask in quest.subTaskList)
                    {
                        if (subTask.isCompleted == false)
                        {
                            return GameOverReason.QuestFailed;
                        }
                    }
                }
            }
        }

        if (PlayerService != null && PlayerService.GetStatusViewModel() != null)
        {
            if (PlayerService.GetStatusViewModel().IsSleeping == false)
            {
                return GameOverReason.NoSleep;
            }
        }

        return GameOverReason.None;
    }

    private void SetGameClear()
    {
        if (GameStateService != null)
        {
            GameStateService.GetViewModel().OnRequestingPause();
        }
        UIManager.Instance.CloseAllPopups();

        UIManager.Instance.OpenEndingVideoPlayerUI();
    }

    private async UniTask SetGameOver(int failedDay, GameOverReason reason)
    {
        if (GameStateService != null)
        {
            GameStateService.GetViewModel().OnRequestingPause();
        }
        UIManager.Instance.CloseAllPopups();

        UIBase videoUI = null;

        if (reason == GameOverReason.QuestFailed)
        {
            UIManager.Instance.OpenFailVideoPlayerUI();
           
            videoUI = UIManager.Instance.GetOpenedUI(UIRootType.VeryFrontUI, UIType.FailVideoPlayerUI);
        }
        else if (reason == GameOverReason.NoSleep)
        {
            UIManager.Instance.OpenSleepFailVideoPlayerUI();
          
            videoUI = UIManager.Instance.GetOpenedUI(UIRootType.VeryFrontUI, UIType.SleepFailVideoPlayerUI);
        }

        if (videoUI != null)
        {
            while (videoUI.gameObject.activeSelf == true)
            {
                await UniTask.Yield();
            }
        }

        UIManager.Instance.CloseAllPopups();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenUI(UIRootType.VeryFrontUI, UIType.GameOverPopupUI);
        }

        ReturnToTitleSequence();
    }

    
    public void ReturnToTitleSequence()
    {
        UIManager.Instance?.CloseAllPopups();
        UIManager.Instance?.CloseMainUI();

        if (GameStateService != null)
        {
            GameStateService.GetViewModel().OnRequestingTitle();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenGameStartUI();
        }
    }

    public async UniTaskVoid ProcessDailyStartEvent(int currentDay)
    {
        int index = currentDay - 1;
        bool hasReceivedItem = false;


        if (dailyEventDataList != null && index >= 0 && index < dailyEventDataList.Length)
        {
            DailyEventData dailyData = dailyEventDataList[index];

            if (dailyData.rewards != null && dailyData.rewards.Length > 0)
            {
                foreach (var reward in dailyData.rewards)
                {
                    if (!string.IsNullOrEmpty(reward.rewardItemId) && reward.rewardAmount > 0)
                    {
                        if (this.InventoryService != null)
                        {
                            this.InventoryService.AddItem(reward.rewardItemId, reward.rewardAmount);

                            hasReceivedItem = true;
                        }
                    }
                }
            }
        }


        if (currentDay >= 2)
        {
            switch (currentDay)
            {
                case 2:
                    UIManager.Instance.OpenDay1VideoPlayerUI();
                    break;
                case 3:
                    UIManager.Instance.OpenDay2VideoPlayerUI();
                    break;
                case 4:
                    UIManager.Instance.OpenDay3VideoPlayerUI();
                    break;
                case 5:
                    UIManager.Instance.OpenDay4VideoPlayerUI();
                    break;
                default:
                    Debug.Log($"[ProcessDailyStartEvent] {currentDay}일차 컷신 영상 없음.");
                    break;
            }
        }


        if (hasReceivedItem)
        {
            await UniTask.DelayFrame(5);

            if (GameStateService.GetCurrentState() == GameState.Paused)
            {
                await UniTask.WaitUntil(() => GameStateService.GetCurrentState() == GameState.Playing);

                await UniTask.Delay(System.TimeSpan.FromSeconds(0.5));
            }

            UIManager.Instance.OpenSimplePopup("아이템이 지급되었습니다.");
        }
    }
}