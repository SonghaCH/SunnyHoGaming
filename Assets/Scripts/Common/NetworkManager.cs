using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;


public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Inst { get; set; }

    public NetworkInventoryService InventoryService { get; private set; }
    public PlayerService PlayerService { get; private set; }
    public GameStateService GameStateService { get; private set; }
    public TimeService TimeService { get; private set; }


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
    }

    private void Update()
    {
        if (GameStateService != null)
        {
            GameState currentGameState = GameStateService.GetViewModel().CurrentGameState;

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
                    PlayerService.UpdatePlayerState(currentDay, deltaTime);
                }
            }
        }
    }

    private void InitNetworkService()
    {
        // 앞으로 네트워크 매니저에서 사용할 다양한 서비스를 생성
        InventoryService = new NetworkInventoryService();
        TimeService = new TimeService(0.01f);
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
            GameState currentGameState = GameStateService.GetViewModel().CurrentGameState;

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

    // 파일 저장 경로 설정 (C:/Users/이름/.../projectName/save.json)
    private string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "SaveData.json");
    } 

    // 세이브 기능 구현
    public void RequstSaveData(PlayerModel data)
    {
        // prettyPrint = true는 JSON을 보기 좋게 정렬
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(), json); // 파일 쓰기는 상당한 비용이 소모됨!
        Debug.Log($"저장 완료: {GetPath()}");
    }

    // 로드 기능
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
            RequstSaveData(GetDefaultPlayerData());
            return playerData;
        }
    }

    public PlayerModel GetDefaultPlayerData()
    {
        var newPlayerData = new PlayerModel();
        
        return newPlayerData;
    }
}
