using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
        TimeService = new TimeService(0.05f);
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
                Debug.LogWarning("[SaveGame] 저장 시점에 픽서 컨테이너가 비어있습니다. 빈 데이터가 저장되는 것을 방지합니다.");
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
        Debug.Log($"저장 완료: {GetPath()}");
    }

    public PlayerModel RequstLoadSaveData()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerModel data = JsonUtility.FromJson<PlayerModel>(json);
            Debug.Log("데이터를 불러왔습니다.");
            return data;
        }
        else
        {
            Debug.LogWarning("세이브 파일이 없습니다. 새 데이터를 생성합니다.");
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

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.ResetDailyQuests();
            }

            int today = TimeService.GetViewModel().CurrentDay;
            Debug.Log($"[Auto Save] {today}일 차 아침 8시가 되었습니다. 픽서 스폰 완료 대기...");

            while (WorldManager.Instance == null || WorldManager.Instance.IsSpawnCompleted == false)
            {
                await UniTask.Yield();
            }

            await UniTask.Yield();

            RequestSaveGame();
        }
    }
}