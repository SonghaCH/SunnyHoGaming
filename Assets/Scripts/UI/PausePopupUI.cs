using Unity.AppUI.UI;
using UnityEngine;

public class PausePopupUI : UIBase
{
    [SerializeField] private UIButton Button_Resume;
    [SerializeField] private UIButton Button_Setting;
    [SerializeField] private UIButton Button_Exit;


    private void OnEnable()
    {
        Button_Resume.BindOnClickButtonEvent(Onclick_Resume);
        Button_Setting.BindOnClickButtonEvent(Onclick_Setting);
        Button_Exit.BindOnClickButtonEvent(Onclick_Exit);
    }

    private void Onclick_Resume()
    {
        UIManager.Instance.ClosePausePopupUI();
    }
    private void Onclick_Setting()
    {
        UIManager.Instance.ClosePausePopupUI();
        //UIManager.Instance.OpenSettingPopupUI();
    }
    private void Onclick_Exit()
    {
        UIManager.Instance.ClosePausePopupUI();
        //UIManager.Instance.OpenExitPopupUI();

    }
}
