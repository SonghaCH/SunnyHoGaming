using UnityEngine;

public enum GatewayType
{
    None,
    SimplePositionMove,
    ZoneLoad
}

public class _GatewaySpot : MonoBehaviour
{
    [Header("게이트웨이 지점 정보 - 에디터에서 등록")]
    [SerializeField] private Transform Transform_GatewayArrivalPoint;
    [SerializeField] private Vector3 Position_SpecificArrivalPoint;
    [SerializeField] private Vector3 Rotation_SpecificArrivalRotation;

    [Header("게이트웨이 지점 데이터 ID - 데이터 드리븐 사용시 등록")]
    [SerializeField] private string ArrivalZoneDataId; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var arrivalPoint = Transform_GatewayArrivalPoint == null ? Position_SpecificArrivalPoint : Transform_GatewayArrivalPoint.position;
            var arrivalRotation = Rotation_SpecificArrivalRotation;
            MovePlayerToOtherPosition(other.transform, arrivalPoint, arrivalRotation);
        }
    }

    private void MovePlayerToOtherPosition(Transform playerTransform, Vector3 targetPosition, Vector3 targetRotation)
    {
        // + 월드매니저를 통해서 순간이동 처리해주는게 더 좋다!
        // 게이트웨이는 어디까지나 충돌을 감지하고, 갈 위치에 대한 데이터만 보관하고 전달하는 역할만 하는게 좋음

        playerTransform.transform.position = targetPosition;
        playerTransform.Rotate(targetRotation);
    }

    private void MovePlayerToNewZone(string arrivalZoneDataId)
    {
        // 1) UIManager를 통해서 로딩 UI를 띄운다
        // 2) Zone테이블에 로드할 Zone의 데이터를 가져온다
        // 3) 데이터에 미리 기입된 맵의 에셋 경로(어드레서블)을 가지고, 월드 매니저에게 새 맵을 불러오도록 요청한다 (비동기 추천)
        // 4) 맵이 다 로드된 다음에 콜백에서 해당 맵의 시작위치 Vector3를 받아와서 플레이어를 이동시켜준다 (참고로 StartPoint 같은게 데이터에 있으면 된다)
    }
}
