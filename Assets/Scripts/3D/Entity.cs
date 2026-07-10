using UnityEngine;
using Cysharp.Threading.Tasks;

public class Entity : MonoBehaviour
{
    [Header("주소 설정")]
    [SerializeField] private string ResourceMeshObjectPath;
    [SerializeField] private string ResourceAnimControllerPath; // 혹시 애니메이션 컨트롤러를 임의 지정해야하는 경우 사용


    [Header("캐릭터 3D 메쉬 & 애니메이션 전용")]
    [SerializeField] private Transform TranformEntityMeshRoot;
    [SerializeField] private Animator AnimatorEntity;
    [SerializeField] private Vector3 InitialMeshScale;

    private void Start()
    {
        // 어드레서블 주소가 미리 설정되어 있다면, 해당 리소스를 비동기 로딩한다.
        if (string.IsNullOrEmpty(ResourceMeshObjectPath) == false)
        {
            InitEntity3DMeshAsync(ResourceMeshObjectPath, ResourceAnimControllerPath).Forget();
        }
    }

    public async UniTaskVoid InitEntity3DMeshAsync(string meshObjectPath, string specificAnimContollerPath = "")
    {
        TranformEntityMeshRoot.gameObject.SetActive(false);
        var animator = await GameUtil.LoadAndMeshObjectAndBindAnimator(TranformEntityMeshRoot, meshObjectPath, specificAnimContollerPath);
        if(animator == null)
        {
            Debug.LogError($"{meshObjectPath} 오브젝트 비동기 로딩에 실패했습니다!!");
        }

        AnimatorEntity = animator;
        TranformEntityMeshRoot.localScale = InitialMeshScale;
        TranformEntityMeshRoot.gameObject.SetActive(true);
    }

    public Animator GetEntityAnimator()
    {
        return AnimatorEntity;
    }
}
