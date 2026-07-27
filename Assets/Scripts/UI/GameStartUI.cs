using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private UIButton Btn_NewGame; 
    [SerializeField] private UIButton Btn_LoadGame; 
    [SerializeField] private UIButton Btn_Setting;
    [SerializeField] private UIButton Btn_Exit;

    [Header("Camera")]
    [SerializeField] private GameObject _skyboxCameraPrefab;

    private GameObject _skyboxCameraInstance;

    private void OnEnable()
    {
        ShowCursor();
        if (NetworkManager.Inst != null && NetworkManager.Inst.PlayerService != null)
        {
            NetworkManager.Inst.PlayerService.SetCanMove(false);
        }

        _skyboxCameraInstance = Instantiate(_skyboxCameraPrefab);

        if (Btn_NewGame != null) Btn_NewGame.BindOnClickButtonEvent(OnClick_NewGame);
        if (Btn_LoadGame != null) Btn_LoadGame.BindOnClickButtonEvent(OnClick_LoadGame);
        if (Btn_Setting != null) Btn_Setting.BindOnClickButtonEvent(OnClick_Setting);
        if (Btn_Exit != null) Btn_Exit.BindOnClickButtonEvent(OnClick_Exit);

        AudioManager.Instance.PlayBGM("Sound/TitleBGM");
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
        {
            ShowCursor();
        }
    }

    private void OnDisable()
    {
        if (Btn_NewGame != null) Btn_NewGame.UnBindAllOnClickButtonEvent();
        if (Btn_LoadGame != null) Btn_LoadGame.UnBindAllOnClickButtonEvent();
        if (Btn_Setting != null) Btn_Setting.UnBindAllOnClickButtonEvent();
        if (Btn_Exit != null) Btn_Exit.UnBindAllOnClickButtonEvent();

        if (_skyboxCameraInstance != null)
        {
            Destroy(_skyboxCameraInstance);
        }

        AudioManager.Instance.StopBGM();
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    private void OnClick_NewGame()
    {
        AudioManager.Instance.StopBGM();
        if (NetworkManager.Inst != null)
        {
            NetworkManager.Inst.RequestNewGame(); 
        }

        TransitionToMainGame(true).Forget();
    }

 
    private void OnClick_LoadGame()
    {
        if (NetworkManager.Inst != null)
        {
            NetworkManager.Inst.RequestLoadGame();
        }

        TransitionToMainGame(false).Forget();
        AudioManager.Instance.PlayBGM("Sound/InGameBGM");
    }


    private async UniTaskVoid TransitionToMainGame(bool playVideo)
    {
        UIManager.Instance.CloseGameStartUI();
       

        if (playVideo)
        {
            UIManager.Instance.OpenOpeningVideoPlayerUI();
        }
        else
        {
            if (NetworkManager.Inst != null && NetworkManager.Inst.GameStateService != null)
            {
                NetworkManager.Inst.GameStateService.GetViewModel().OnRequestingPlay();
            }
        }
        UIManager.Instance.OpenFixerPopupUI();
        await UniTask.Yield(PlayerLoopTiming.Update);
        UIManager.Instance.CloseFixerPopupUI();
        UIManager.Instance.OpenMainUI();
    }

    private void OnClick_Setting()
    {
        UIManager.Instance.OpenSettingPopupUI();
        Debug.Log("세팅 버튼~");
    }

    private void OnClick_Exit()
    {
        Application.Quit();
    }
}