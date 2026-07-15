using UnityEngine;

public class NotePopupUI : UIBase
{
    [SerializeField] private UIButton Btn_Back;

    private void OnEnable()
    {
        Btn_Back.BindOnClickButtonEvent(OnClick_Back);
    }

    private void OnClick_Back()
    {
        UIManager.Instance.CloseNotePopupUI();
    }
}
