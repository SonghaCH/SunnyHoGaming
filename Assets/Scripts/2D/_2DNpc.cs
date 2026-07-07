using UnityEngine;

public class _2pc : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField] private _2DAnimatorController AnimatorController_Entity;

    [Header("NPC의 정보")]
    [SerializeField] private int _instanceId;
    [SerializeField] private string _npcDataId;

    private bool _isFirstCollied;

    public void StartInteract()
    {
        ChangeNpcState(_EntityAnimState.InteractionStart);
    }

    public void ChangeNpcState(_EntityAnimState newState)
    {
        // 우선 애니메이션만 바꿔 봅시다
        AnimatorController_Entity.SetState(newState);
    }

    

    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            OnInteractionButtonClicked("G");
        }
    }

    private void OnInteractionButtonClicked(string interectionKey)
    {
        if(interectionKey == "G")
        {
            UIManager.Instance.OpenInventoryPopup();
        }
    }


   

}
