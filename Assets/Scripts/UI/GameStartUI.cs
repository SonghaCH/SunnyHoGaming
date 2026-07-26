using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : UIBase
{
    [SerializeField] private UIButton Btn_Start;
    [SerializeField] private UIButton Btn_Setting;
    [SerializeField] private UIButton Btn_Exit;
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

        Btn_Start.BindOnClickButtonEvent(OnClick_Start);
        Btn_Setting.BindOnClickButtonEvent(OnClick_Setting);
        Btn_Exit.BindOnClickButtonEvent(OnClick_Exit);

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
        Btn_Start.UnBindAllOnClickButtonEvent();
        Btn_Setting.UnBindAllOnClickButtonEvent();
        Btn_Exit.UnBindAllOnClickButtonEvent();

        if (_skyboxCameraInstance != null)
        {
            Destroy(_skyboxCameraInstance);
        }
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnClick_Start()
    {
        AudioManager.Instance.StopBGM();
        UIManager.Instance.CloseGameStartUI();
        UIManager.Instance.OpenVideoPlayerUI();
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