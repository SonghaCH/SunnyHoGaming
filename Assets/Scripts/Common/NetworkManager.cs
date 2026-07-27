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

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Inst { get; set; }

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
        TimeService = new TimeService(0.1f);
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
            PlayerModel loadedData = RequstLoadSaveData();

            TimeService.SetTimeByDay(loadedData.CurrentDay);

            if (GameObjectManager.Instance != null)
            {
                await GameObjectManager.Instance.ClearAllFixersAsync();
            }

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
        }
    }

    public void RequestLoadGame()
    {
        RequestLoadGameAsync().Forget();
    }

    public void RequestNewGame()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        PlayerModel defaultData = GetDefaultPlayerData();
        RequstSaveData(defaultData);
        RequestLoadGame(); 
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
                await SetGameClear();
                return; 
            }

            int failedDay = currentDay - 1;
            GameOverReason reason = CheckGameOver(failedDay);

            if (reason != GameOverReason.None)
            {
                SetGameOver(failedDay, reason); 
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

    private async UniTask SetGameClear()
    {
        if (GameStateService != null)
        {
            GameStateService.GetViewModel().OnRequestingPause();
        }
        UIManager.Instance.CloseAllPopups();

        UIManager.Instance.OpenEndingVideoPlayerUI();

        UIBase endingUI = UIManager.Instance.GetOpenedUI(UIRootType.VeryFrontUI, UIType.EndingVideoPlayerUI);

        if (endingUI != null)
        {
            VideoPlayer vp = endingUI.GetComponentInChildren<VideoPlayer>();

            if (vp != null)
            {
                vp.Prepare();
                await UniTask.WaitUntil(() => vp.isPrepared);

                vp.Play();
                await UniTask.WaitUntil(() => vp.isPlaying == false);
            }
            else
            {
                Debug.LogWarning("[SetGameClear] EndingVideoPlayerUI에 VideoPlayer 컴포넌트가 없습니다!");
            }
        }

        UIManager.Instance.CloseEndingVideoPlayerUI();

        PlayerModel defaultData = GetDefaultPlayerData();
        RequstSaveData(defaultData);

        if (GameStateService != null)
        {
            GameStateService.GetViewModel().OnRequestingTitle();
        }
    }

    private void SetGameOver(int failedDay, GameOverReason reason)
    {
        if (GameStateService != null)
        {
            GameStateService.GetViewModel().OnRequestingPause();
        }
        UIManager.Instance.CloseAllPopups(); 

        if (reason == GameOverReason.QuestFailed)
        {
            UIManager.Instance.OpenFailVideoPlayerUI();

            
        }
        else if (reason == GameOverReason.NoSleep)
        {
            UIManager.Instance.OpenSleepFailVideoPlayerUI();

            
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenGameOverPopupUI();
        }
    }
}