using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private string targetDialogueId;
    [SerializeField] private bool isOneTimeTrigger = true;

    private bool _hasTriggered = false;

    private void Awake()
    {
        // 실수를 방지하기 위해 BoxCollider의 IsTrigger 설정을 강제로 켜줍니다.
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOneTimeTrigger && _hasTriggered) return;

        // (프로젝트 설정에서 레이어 충돌 매트릭스를 적용했다면 더 안전합니다!)
        if (other.CompareTag("Player"))
        {
            _hasTriggered = true;

            // 1. UIManager를 통해 DialogueUI를 가져오거나 새로 활성화합니다.
            var uiBase = UIManager.Instance.OpenUI(UIRootType.VeryFrontUI, UIType.DialogueUI);

            if (uiBase is DialogueUI dialogueUi)
            {
                // 2. 인스펙터에 적어둔 ID를 넘겨서 비동기 타이핑 연출 시작!
                dialogueUi.StartDialogue(targetDialogueId);
            }

            // 한 번만 쓰고 버릴 트리거라면 씬에서 바로 없애서 물리 연산 부하를 0으로 만듭니다.
            if (isOneTimeTrigger)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}