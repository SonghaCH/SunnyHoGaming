using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject GObj_StartObject;

    public static GameManager Inst { get; set; }

    // 플레이어를 캐싱 -> 게임을 할때 1개라서
    // LocalPlayer (나) / RemotePlayer (너)
    // public _2DPlayer LocalPlayer; -> GameObjectManager또는 GameManager의 GetLocalPlayer로 역할 이전됨

    // 플레이 중에 저장되어야 하는 정보들이 있는 위치
    private PlayerModel _playerModel = new PlayerModel();
    public PlayerService PlayerService { get; private set; }
    public GameStateService GameStateService { get; private set; }
    public TimeService TimeService { get; private set; }

    private void Awake()
    {
        Inst = this;
        InitServices();
    }
    private void InitServices()
    {
        TimeService = new TimeService(1.0f);
        PlayerService = new PlayerService();
        GameStateService = new GameStateService();
    }


    private void Start()
    {
        LoadSaveData();
    }

    public void StartGame()
    {
        UIManager.Instance.OpenUI(UIRootType.MainUI, UIType.LocalPlayerProfileUI);
        UIManager.Instance.OpenUI(UIRootType.MainUI, UIType.MVVMTestUI);
    }

    public void SaveData()
    {
        NetworkManager.Inst.RequstSaveData(_playerModel);
    }

    public void SaveAndEndGame()
    {
        SaveData();
        Application.Quit();
    }

    private void LoadSaveData()
    {
        _playerModel = NetworkManager.Inst.RequstLoadSaveData();
    }


}
