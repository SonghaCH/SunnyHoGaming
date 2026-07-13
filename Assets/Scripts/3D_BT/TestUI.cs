using UnityEngine;
using TMPro; 

public class FixerStateUI : MonoBehaviour
{
    [Header("연결할 픽서와 텍스트 UI")]
    [SerializeField] private FixerViewModel targetFixer;
    [SerializeField] private TextMeshProUGUI stateText;

    private void OnEnable()
    {
        if (targetFixer != null)
        {
            targetFixer.OnStateChanged += UpdateStateUI;

            UpdateStateUI(targetFixer.CurrentState);
        }
    }

    private void OnDisable()
    {
        if (targetFixer != null)
        {
            targetFixer.OnStateChanged -= UpdateStateUI;
        }
    }

    private void UpdateStateUI(FixerState newState)
    {
        if (stateText != null)
        {
            stateText.text = $"픽서 상태: {newState}";
        }
    }
}