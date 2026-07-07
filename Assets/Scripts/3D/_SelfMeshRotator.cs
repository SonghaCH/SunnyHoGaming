using UnityEngine;

public class _SelfRotator : MonoBehaviour
{
    [SerializeField] private Transform _targetMeshRoot;

    [Tooltip("초당 회전할 각도")]
    [SerializeField] private Vector3 _rotationSpeed = new Vector3(0f, 30f, 0f); // 기본값: Y축으로 초당 30도

    void Update()
    {
        _targetMeshRoot.Rotate(_rotationSpeed * Time.deltaTime);
    }
}
