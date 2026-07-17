using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform _doorTransform;
    [SerializeField] private float _openHeight = 3f;
    [SerializeField] private float _moveSpeed = 2f;

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
        // MoveTowards - 일정한 속도로 이도시켜주는 함수
        Vector3 targetPosition = _isOpen ? _openPosition : _closedPosition;
        _doorTransform.localPosition = Vector3.MoveTowards(_doorTransform.localPosition, targetPosition, _moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isOpen = false;
        }
    }
}
