using UnityEngine;
using UnityEngine.UI;

public class NotePopupUI : UIBase
{
    [SerializeField] private UIButton Btn_Back;
    [SerializeField] private UIButton Btn_Reverse;
    [SerializeField] private UIButton Btn_TurnBack;
    [SerializeField] private GameObject Image_Defolt;
    [SerializeField] private GameObject Image_Change;


    private void OnEnable()
    {
        Btn_Back.BindOnClickButtonEvent(OnClick_Back);
        Btn_Reverse.BindOnClickButtonEvent(OnClick_Reverse);
        Btn_TurnBack.BindOnClickButtonEvent(OnClick_TurnBack);
    }

    private void OnDisable()
    {
        Btn_Back.UnBindAllOnClickButtonEvent();
        Btn_Reverse.UnBindAllOnClickButtonEvent();
        Btn_TurnBack.UnBindAllOnClickButtonEvent();
    }

    private void OnClick_Back()
    {
        UIManager.Instance.CloseNotePopupUI();
    }

    private void OnClick_Reverse()
    {
        Image_Defolt.SetActive(false);
        Image_Change.SetActive(true);
        Btn_TurnBack.BindOnClickButtonEvent(OnClick_TurnBack);
    }

    private void OnClick_TurnBack()
    {
        Image_Defolt.SetActive(true);
        Image_Change.SetActive(false);
        Btn_Reverse.BindOnClickButtonEvent(OnClick_Reverse);
    }
}
