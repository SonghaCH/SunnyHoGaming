using TMPro;
using UnityEngine;

public class FPopupUI : UIBase
{
    [SerializeField] private TextMeshProUGUI _textName;

    public void SetInteractName(string interactName)
    {
        _textName.text = interactName;
    }
}
