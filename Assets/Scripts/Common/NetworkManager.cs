using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Inst { get; set; }

    public NetworkPlayerService LocalPlayerService { get; private set; }
    public NetworkInventoryService InventoryService { get; private set; }

    private void Awake()
    {
        Inst = this;
        InitNetworkService();
    }

    private void InitNetworkService()
    {
        // 앞으로 네트워크 매니저에서 사용할 다양한 서비스를 생성
        LocalPlayerService = new NetworkPlayerService();
        InventoryService = new NetworkInventoryService();

    }

    public void RequestCreateLocalPlayer()
    {
        // 게임 시작이나, 맵 진입 시 로컬 플레이어를 서버에 생성하는 요청
        var localPlayerVm = LocalPlayerService.CreateLocalPlayerViewModel();

        // 응답 받았다고 가정한다 = 추후 실제 서버 통신시에는 람다나 비동기 로직으로 받아온다
        OnRecvCreateLocalPlayer(localPlayerVm);
    }

    public void OnRecvCreateLocalPlayer(LocalPlayerViewModel localPlayerVm)
    {
        GameObjectManager.Inst.CreateLocalPlayer(localPlayerVm);
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
        newPlayerData.PlayerName = "NoName";
        newPlayerData.PlayerTotalExp = 0;
        return newPlayerData;
    }
}
