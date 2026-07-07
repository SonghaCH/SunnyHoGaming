using UnityEngine;

public class _MapChunkAnchor : MonoBehaviour
{
    [Tooltip("이 청크 프리팹의 어드레서블 주소 이름을 적어주세요.")]
    [SerializeField] private string _addressableKey;

    public string GetRegisteredAddressableKey()
    {
        return _addressableKey;
    }

    public Vector3 GetRegisteredPosition()
    {
        return this.transform.position;
    }
}
