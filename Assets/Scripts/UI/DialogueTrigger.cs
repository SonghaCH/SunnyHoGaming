using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] protected string targetDialogueId;
    [SerializeField] protected bool isOneTimeTrigger = true;

    protected bool _hasTriggered = false;

    private void Awake()
    {
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isOneTimeTrigger && _hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            ExecuteDialogue();
        }
    }

    protected void ExecuteDialogue()
    {
        _hasTriggered = true;

        var uiBase = UIManager.Instance.OpenUI(UIRootType.VeryFrontUI, UIType.DialogueUI);

        if (uiBase is DialogueUI dialogueUi)
        {
            dialogueUi.StartDialogue(targetDialogueId);
        }

        if (isOneTimeTrigger)
        {
            Destroy(gameObject);
        }
    }

    protected void OnDrawGizmos()
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