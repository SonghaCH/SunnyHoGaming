using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform _doorTransform;
    [SerializeField] private float _openHeight = 3f;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private int _unlockDay = 2;

    private Vector3 _closedPosition;
    private Vector3 _openPosition;
    private bool _isOpen;

    private void Awake()
    {
        _closedPosition = _doorTransform.localPosition;
        _openPosition = _closedPosition + Vector3.up * _openHeight;
    }

    private void Update()
    {
        Vector3 targetPosition = _isOpen ? _openPosition : _closedPosition;
        _doorTransform.localPosition = Vector3.MoveTowards(_doorTransform.localPosition, targetPosition, _moveSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        DoorPopupUI.OnPasswordSuccess += OpenDoor;
    }

    private void OnDisable()
    {
        DoorPopupUI.OnPasswordSuccess -= OpenDoor;
    }

    private void OnTriggerEnter(Collider other)
    {
        int currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;
        if (currentDay < _unlockDay)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            UIManager.Instance.OpenFPopupUI();
        }

        if (other.CompareTag("Fixer"))
        {
            _isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"OnTriggerExit 호출됨: {other.name}, tag: {other.tag}");
        if (other.CompareTag("Player") || other.CompareTag("Fixer"))
        {
            _isOpen = false;
        }
    }

    private void OpenDoor()
    {
        Debug.Log("[Door] OpenDoor() 호출됨!");
        _isOpen = true;
    }

    public bool Interact()
    {
        int currentDay = NetworkManager.Inst.TimeService.GetViewModel().CurrentDay;
        return currentDay >= _unlockDay;
    }
}