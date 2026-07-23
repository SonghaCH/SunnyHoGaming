using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SubTaskSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subTaskText;

    // 🌟 1. 체크표시(V) 오브젝트를 받기 위한 변수 추가!
    [SerializeField] private GameObject checkMarkObject;

    public void SetData(SubTaskData subTask)
    {
        if (subTask == null) return;

        // 텍스트 출력 ("산소 공급 장치 수리" 등)
        if (subTaskText != null)
        {
            subTaskText.text = subTask.subTaskText;
        }

        // 🌟 2. 퀘스트/서브태스크 완료 여부에 따라 체크표시 V 켜고 끄기
        if (checkMarkObject != null)
        {
            checkMarkObject.SetActive(subTask.isCompleted);
        }
    }
}
