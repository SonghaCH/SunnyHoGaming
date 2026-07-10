using UnityEngine;

public class WorkPopupUI : UIBase
{
    [SerializeField] private UIButton Btn_WorkStart;
    [SerializeField] private UIButton Btn_Cancle;
    [SerializeField] private UIButton Btn_Close;


    private void OnEnable()
    {
        Btn_WorkStart.BindOnClickButtonEvent(OnClick_WorkStart);
        Btn_Cancle.BindOnClickButtonEvent(OnClick_Cancle);
        Btn_Close.BindOnClickButtonEvent(OnClick_Close);
    }

    private void OnClick_WorkStart()
    {
        Debug.Log("작업시작");
    }

    private void OnClick_Cancle()
    {
        Debug.Log("작업 취소");

    }

    private void OnClick_Close()
    {
        UIManager.Instance.CloseWorkPopupUI();
    }

}

