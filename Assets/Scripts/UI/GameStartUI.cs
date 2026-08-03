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
    private bool _isProcessing = false;

    private void OnEnable()
    {
        _isProcessing = false;

        ShowCursor();
        if (NetworkManager.Inst != null && NetworkManager.Inst.PlayerService != null)
        {
            NetworkManager.Inst.PlayerService.SetCanMove(false);
        }

        if (_skyboxCameraPrefab != null && _skyboxCameraInstance == null)
        {
            _skyboxCameraInstance = Instantiate(_skyboxCameraPrefab);
        }

        if (Btn_NewGame != null) Btn_NewGame.BindOnClickButtonEvent(OnClick_NewGame);
        if (Btn_LoadGame != null) Btn_LoadGame.BindOnClickButtonEvent(OnClick_LoadGame);
        if (Btn_Setting != null) Btn_Setting.BindOnClickButtonEvent(OnClick_Setting);
        if (Btn_Exit != null) Btn_Exit.BindOnClickButtonEvent(OnClick_Exit);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM("Sound/TitleBGM");
        }
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

        ClearSkyboxCamera();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }
    }

    private void ClearSkyboxCamera()
    {
        if (_skyboxCameraInstance != null)
        {
            Destroy(_skyboxCameraInstance);
            _skyboxCameraInstance = null;
        }
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnClick_NewGame()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        StartNewGameAsync().Forget();
    }

    private async UniTaskVoid StartNewGameAsync()
    {
        if (NetworkManager.Inst != null)
        {
            NetworkManager.Inst.RequestNewGame();
        }

        await TransitionToMainGameAsync(true);
    }

    private void OnClick_LoadGame()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        StartLoadGameAsync().Forget();
    }

    private async UniTaskVoid StartLoadGameAsync()
    {
        if (NetworkManager.Inst != null)
        {
            await NetworkManager.Inst.RequestLoadGameAsync();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM("Sound/InGameBGM");
        }

        await TransitionToMainGameAsync(false);
    }

    private async UniTask TransitionToMainGameAsync(bool playVideo)
    {
        ClearSkyboxCamera();
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

        UIManager.Instance.OpenMainUI();
        await UniTask.CompletedTask;
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