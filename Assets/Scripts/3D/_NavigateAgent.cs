using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class _NavigateAgent : MonoBehaviour
{
    [Header("추적 대상")]
    [SerializeField] private Transform Transform_TargetObject;

    [Header("설정")]
    [SerializeField] private bool IsAlwaysTargetPlayer;
    [SerializeField] private bool IsReturnToOrigin;
    [SerializeField] private float PathUpdateInterval = 0.2f; // 경로 갱신 주기
    [SerializeField] private float TargetCheckInterval = 1.0f;  // 타겟 감시 주기 (1초)

    [Header("애니메이션을 제어하는 컴포넌트 등록")]
    [SerializeField] private BattleAgent BattleAgent_StateChanger; // 전투 컴포넌트에 상태를 넣어놨다보니 그냥 사용하는데, 따로 애니메이션만 제어하는 상태패턴 사용 컴포넌트를 따로 둬도 됩니다
    private NavMeshAgent _agent;
    private Vector3 _originPosition; // 처음 스폰된 위치 저장용
    private Coroutine _monitorCoroutine;
    private Coroutine _followCoroutine;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // 1초마다 타겟 상태를 체크하는 메인 루틴 시작
        _monitorCoroutine = StartCoroutine(MonitorTargetRoutine());
        _originPosition = transform.position;
    }

   
    private void OnDisable()
    {
        StopAllCoroutines();
        _monitorCoroutine = null;
        _followCoroutine = null;
    }

    /// <summary>
    /// 외부에서 스크립트로 타겟을 직접 주입할 때도 안전하게 대응
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        Transform_TargetObject = newTarget;
    }

    /// <summary>
    /// 1초마다 타겟의 존재 여부를 체크하는 코루틴
    /// </summary>
    private IEnumerator MonitorTargetRoutine()
    {
        WaitForSeconds checkWait = new WaitForSeconds(TargetCheckInterval);

        while (true)
        {
            if (Transform_TargetObject != null)
            {
                // 타겟은 있는데 추적 루틴이 돌고 있지 않다면 새로 시작
                if (_followCoroutine == null)
                {
                    _followCoroutine = StartCoroutine(FollowTargetRoutine());
                }
            }
            else
            {
                // 타겟이 없어졌다면 추적 루틴을 멈추고 에이전트 정지
                if (_followCoroutine != null)
                {
                    StopCoroutine(_followCoroutine);
                    _followCoroutine = null;

                    if (_agent.isOnNavMesh)
                    {
                        _agent.ResetPath(); // 목적지 초기화 (제자리 정지)
                    }
                }

                if (IsAlwaysTargetPlayer == true) 
                {
                    // 방식 1) 일정 간격마다 플레이어를 찾아본다
                    TryFindLocalPlayer();
                }
            }

            yield return checkWait;
        }
    }

    /// <summary>
    /// 타겟을 향해 경로를 주기적으로 갱신하는 코루틴
    /// </summary>
    private IEnumerator FollowTargetRoutine()
    {
        WaitForSeconds updateWait = new WaitForSeconds(PathUpdateInterval);

        while (Transform_TargetObject != null)
        {
            if (_agent.isOnNavMesh)
            {
                _agent.SetDestination(Transform_TargetObject.position);
                ChangeAnimation(BattleState.Run);
            }

            yield return updateWait;
        }

        _followCoroutine = null;
    }


    private void TryFindLocalPlayer()
    {
        // 방식 1) 아무리 멀리 있어도 몬스터가 무조건 플레이어를 알아야하는 경우 - 주기적으로 플레이어를 찾아본다
        var player = GameManager.Inst.GetLocalPlayer();
        if (player != null)
        {
            SetTarget(player.transform);
        }
    }

    /// <summary>
    /// 처음 위치로 돌아가도록 명령하는 메서드
    /// </summary>
    private void ReturnToOrigin()
    {
        ChangeAnimation(BattleState.Walk);
        if (_agent.isOnNavMesh)
        {
            _agent.ResetPath();
            SetDestinationWithCallback(_originPosition, OnDestinationArrived);
        }
    }

    private void OnDestinationArrived()
    {
        ChangeAnimation(BattleState.Idle);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 방식 2) 플레이어가 반경 안에 들어온 경우 감지
        if (other.CompareTag("Player") == true)
        {
            SetTarget(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") == true)
        {
            if(IsReturnToOrigin == true)
            {
                ReturnToOrigin();
            }
            else
            {
                if (_agent.isOnNavMesh == true)
                {
                    _agent.ResetPath(); // 목적지 초기화 (제자리 정지)
                    ChangeAnimation(BattleState.Idle);
                }
            }

            Transform_TargetObject = null; // 타겟 오브젝트 초기화
        }
    }

    private void ChangeAnimation(BattleState state)
    {
        // 추후 BattleAgent가 아니라 AnimatedAgent 같은걸 만들수도 있으니 따로 빼둠
        if(BattleAgent_StateChanger != null)
        {
            BattleAgent_StateChanger.ChangeState(state);
        }
    }


    // [목적지 도착 체크] =============================================================================
    /// <summary>
    /// 목적지를 설정하고, 도착 시 실행할 콜백을 등록합니다.
    /// </summary>
    public void SetDestinationWithCallback(Vector3 destination, Action onArrived, float tolerance = 0.2f)
    {
        if (_agent.isOnNavMesh == false) 
        {
            return;
        }

        _agent.SetDestination(destination);

        // 안전하게 기존에 돌고 있을지도 모르는 똑같은 감지 루틴을 하나만 돌도록 제어
        StartCoroutine(MonitorArrivalRoutine(onArrived, tolerance));
    }

    private IEnumerator MonitorArrivalRoutine(Action onArrived, float tolerance)
    {
        // 경로 계산이 완료될 때까지(pathPending이 false가 될 때까지) 매 프레임 대기
        while (_agent.pathPending)
        {
            yield return null;
        }

        while (_agent != null && _agent.isOnNavMesh == true)
        {
            // 남은 거리가 오차 범위 내이고, 속도가 거의 0에 수렴할 때 (도착 판정)
            if (_agent.remainingDistance <= tolerance && _agent.velocity.sqrMagnitude < 0.1f)
            {
                onArrived?.Invoke();
                yield break;
            }
            yield return null;
        }
    }
}
