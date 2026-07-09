using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class _PlayableAgent : MonoBehaviour
{
    [Header("스킬")]
    [SerializeField] private Collider Collider_PlayerNormalAttack;
    [SerializeField] private GameObject Prefab_SkillProjectile;
    [SerializeField] private Transform Transform_SkillProjectileRoot;

    [Header("전투 관련 정보")]
    [SerializeField] private int _maxHp;
    [SerializeField] private int _playerHp = 1000;
    [SerializeField] private int _playerBaseAtk = 100;

    [Header("애니메이터")]
    [SerializeField] private _AnimationController AnimatorControllerEntity;

    private bool _isSkillUsing = false;

    private event Action<int, int> _onHpChanged;
    private event Action<int, int> _onMpChanged;

    void Awake()
    {
        _playerHp = 1000;
        _maxHp = _playerHp;
    }

    private void Start()
    {
        // 나 스스로를 등록한다. -> 씬에 있는 그 플레이어가 등록됨
        GameObjectManager.Inst.RegisterLocalPlayer(this);
    }

    private void OnDisable()
    {
        ResetStatChangedEvent();
    }
    void Update()
    {
       
    }

    private void ChangePlayerState(EntityState newState)
    {
        // 이런 곳에 UI나 플레이어의 별도 처리를 넣어줄 수도 있다


        // 우선 애니메이션만 바꿔 봅시다
        AnimatorControllerEntity.SetState(newState);
    }


    

    

    private void CreateProjectileSkillObject()
    {
        // UI에서도 동적생성했듯, 지금 스킬 투사체 오브젝트도 소환!(실체화-동적생성)
        var gObj = Instantiate(Prefab_SkillProjectile, Transform_SkillProjectileRoot);
        if (gObj == null) return;

        var skillProjectileComponent = gObj.GetComponent<_SkillProjectile>();
        if (skillProjectileComponent == null) return;

        var tag = this.gameObject.tag;
        skillProjectileComponent.InitSkillObject2D(0, true, this.transform.position, 500, tag, OnMonsterCollied);
    }

    private void OnMonsterCollied(int monsterInstanceId, int skillDamage)
    {
        // 게임 오브젝트 매니저는 모든 몬스터를 관리한다
        // 그 자료구조는 Dictionary로 key - instanceId다
        // 몬스터를 게임오브젝트 매니저를 통해 받아올 수 있다!


        // 몬스터때도 구현했던 2번 방식) 플레이어한테 스킬이 충돌정보를 알려주기만 하고, 실제 몬스터와의 상호작용은 플레이어가
        // 주도권을 갖고 한다!
        var monsterComponent = GameObjectManager.Inst.GetMonsterObjectByInstanceId(monsterInstanceId);
        if (monsterComponent == null) return;

        Debug.LogWarning($"플레이어가 {monsterInstanceId}에 데미지 {skillDamage} 부여");
    }


    // 시간제어가 필요하므로 코루틴 , 유니테스크를 사용해야한다
    IEnumerator CoStartNormalAttack()
    {
        _isSkillUsing = true;
        yield return new WaitForSeconds(1.0f);
        Collider_PlayerNormalAttack.gameObject.SetActive(false);
        _isSkillUsing = false;
    }

    // AI가 준 코드에서 -> 메서드 단위로 기능을 분리해서 명료하게 나눠줘
    // 파라미터(매개변수), 지역변수, 반환형, 멤버변수
    public void UseOverlapSkill(Vector3 offsetPosition, float radius)
    {
        //_lastOverlapOffset = offsetPosition;
        //_lastOverlapRadius = radius;

        //// 중심점 기준으로 반지름(radius) 안에 들어온 모든 콜리더를 검출
        //Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);

        //foreach (Collider2D col in hitColliders)
        //{
        //    if (col != null && col.gameObject != this.gameObject)
        //    {
        //        Debug.Log($"오버랩 스킬 적중: {col.name}");
        //    }
        //}
    }


    // 플레이어의 전투와 관련된 부분은 사실 나중에 다른 곳으로 빠질 수 있기 때문에
    // 데이터의 와리가리 하는 부분은 -> Rpg 재접하면 풀피 -> 세이브 -> GameManager
    // 결국 인스턴스 데이터가 이 플레이어 코드안에 있는게 아니라 -> 저장이 가능하도록 GameManager에 플레이어 인스턴스 데이터로 저장이 되어야 함
    // PlayerModel

    

    public void PlayerDie()
    {
        // bool _isAlive = false;
    }

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
        _onHpChanged?.Invoke(_playerHp, _maxHp);
        // _onMpChanged?.Invoke(_playerMp);
    }

    public void AddHp(int hp)
    {
        _playerHp += hp;
        InvokeStatChangedEvent();
    }

    public void AddAtk(int atk)
    {
        _playerBaseAtk += atk;
    }
}
