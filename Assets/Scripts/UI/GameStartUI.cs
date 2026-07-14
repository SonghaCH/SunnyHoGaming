using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : UIBase
{
    [SerializeField] private UIButton Btn_Start;
    [SerializeField] private UIButton Btn_Setting;
    [SerializeField] private UIButton Btn_Exit;


    private void OnEnable()
    {
        Btn_Start.BindOnClickButtonEvent(OnClick_Start);
        Btn_Setting.BindOnClickButtonEvent(OnClick_Setting);
        Btn_Exit.BindOnClickButtonEvent(OnClick_Exit);
    }


    private void OnClick_Start()
    {
        UIManager.Instance.CloseGameStartUI();
        UIManager.Instance.OpenMainUI();
    }
    private void OnClick_Setting()
    {
        Debug.Log("세팅 버튼~");
    }
   
    private void OnClick_Exit()
    {
        Application.Quit();
    }

}
