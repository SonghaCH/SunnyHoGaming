using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleFaction
{
    None,
    Hero,
    Neutral, // 중립
    Enemy
}

public class BattleAgent : MonoBehaviour
{
    [Header("프리팹에서 미리 세팅할 데이터")]
    public float SkillCoolTime;
    public GameObject Prefab_ThisSkillObjct;
    public bool IsDebugSkillOff;

    [Header("데이터를 확인할 수 있도록 임시로 열어줌")]
    public int _instanceId; // 게임 오브젝트 매니저에서 찾기용(동사무소용) - 게임에서 태어날때 부여된 나의 주민등록번호
    public string _dataId; // 데이터 드리븐용 - 나의 부가정보를 나중에 찾을 수 있는 Id 
    public BattleFaction _agentFaction;

    [Header("연동 컴포넌트")]
    [SerializeField] private Entity _myEntity; // 3D 외형 제어용 엔티티 추가

    // TODO : 우선 몬스터 유지
    [Header("받아왔는데 전투에서 필요한 데이터")]
    private MonsterData _thisMonsterData;
    public int _baseHp;
    public int _baseAtk;
    public bool _isAlive = true;
    protected bool _lookRight = true;
    protected int _maxHp;

    // --- 상태 패턴 관련 변수 ---
    private BattleState _currentStateEnum;
    private IBattleState _currentState;
    private Dictionary<BattleState, IBattleState> _states;

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    private void Awake()
    {
        // 상태 객체들 미리 생성 및 캐싱
        _states = new Dictionary<BattleState, IBattleState>
        {
            { BattleState.Idle, new BattleState_Idle() },
            { BattleState.Attack, new BattleState_Attack() },
            { BattleState.Die, new BattleState_Die() },
            { BattleState.Run, new BattleState_Run() },
            { BattleState.Walk, new BattleState_Walk() }
        };

        if (_myEntity == null)
        {
            var entity = this.GetComponent<Entity>();
            if (entity == null)
            {
                Debug.LogError("대상 오브젝트에 Entity 컴포넌트가 존재하지 않습니다!");
                return;
            }
            _myEntity = entity;
        }
    }

    private void OnDisable()
    {
        _isAlive = false;
        ResetStatChangedEvent();
    }

    private void Update()
    {
        if (_isAlive == false)
        {
            return;
        }

        // 현재 상태의 Update 로직을 지속적으로 실행
        if (_currentState != null)
        {
            _currentState.UpdateState(this, _myEntity);
        }
    }

    public bool IsStateChangeable(BattleState newState)
    {
        // 예외처리 전용 (특정 상태일때는 현재 상태가 어떤지에 따라 전환 못하게 미리 막음)
        if (newState == BattleState.Attack)
        {
            if (_currentStateEnum == BattleState.Walk)
            {
                return false;
            }
        }

        return true;
    }

    public void ChangeState(BattleState newState)
    {
        if (_states.ContainsKey(newState) == false)
        {
            Debug.LogWarning($"{newState}에 해당하는 상태 클래스를 찾을 수 없습니다! Awake에서 해당 상태 클래스를 미리 new 해두었는지 확인해주세요!");
            return;
        }

        if (IsStateChangeable(newState) == false)
        {
            return;
        }

        if (_currentState != null)
        {
            _currentState.ExitState(this, _myEntity);
        }

        _currentState = _states[newState];
        _currentState.EnterState(this, _myEntity);
        _currentStateEnum = newState;
    }

    // 태어난 시점에서 어떤 정보를 저장해주자
    public void InitMonster(int instanceId, string dataId)
    {
        _instanceId = instanceId;
        _dataId = dataId;

        // 초기화 한다음에 그 객체가 가지고 있는 데이터를 이렇게 찾아와서 필요한 세팅을 해줍니다.
        var monsterData = GameDataManager.Instance.GetMonsterData(dataId);
        if (monsterData != null)
        {
            // 이 몬스터가 생성된 시점에서 자신의 엑셀 -> JSON을 거친 데이터를 캐싱(보관)해둔다
            _thisMonsterData = monsterData;
            _baseHp = _thisMonsterData.BaseHp;
            _maxHp = _baseHp;
            _baseAtk = _thisMonsterData.BaseAtk;
            _agentFaction = BattleFaction.Enemy;
        }

    }

    public int GetMonsterInstanceId() // 유니티에 GetInstanceID랑 헷갈리지 않도록 함수명을 복잡하게 쓴다
    {
        // 객체 - 데이터 부에 있는것을 반환
        return _instanceId;
    }

    public BattleFaction GetFaction()
    {
        return _agentFaction;
    }

    private int GetFinalNormalAtkDamage(int baseAtk, float normalAtkMultiple)
    {
        return GetFinalSkillDamage(baseAtk, normalAtkMultiple);
    }

    private int GetFinalSkillDamage(int baseAtk, float skillMultiple)
    {
        return (int)(baseAtk * skillMultiple);
    }

    // 코루틴이 등장한다는건 => 유니테스크로 호환이 가능하다
    // 일정 시간마다 스킬을 사용할 예정
    // 스타트 코루틴은 이 몬스터가 생성된 시점에서 돌아도 됨!
    

    protected virtual void ChangeMonsterDirection()
    {
    }

    

    // 몬스터가 소환한 투사체의 충돌이 발생 했을 때 응답이 온다
   


    // 플레이어가 -> 몬스터한테 데미지를 줄때 호출하는 함수
    

    

    public void BindOnStatChangedEvent(Action<int, int> hpChangeCallback, Action<int, int> mpChangeCallback)
    {
        _onHpChanged += hpChangeCallback;
        _onMpChanged += mpChangeCallback;
    }

    public void ResetStatChangedEvent()
    {
        _onHpChanged = null;
        _onMpChanged = null;
    }

    private void InvokeStatChangedEvent()
    {
        // 우선 HP든 MP든 하나라도 바뀌면 다 호출해준다
        _onHpChanged?.Invoke(_baseHp, _maxHp);
        // _onMpChanged?.Invoke(_playerMp);
    }


}
