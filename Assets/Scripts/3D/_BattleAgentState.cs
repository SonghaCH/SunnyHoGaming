using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
public enum BattleState
{
    Idle,
    Move,
    Attack,
    Die,
    Walk,
    Run
}

public interface IBattleState
{
    void EnterState(BattleAgent agent, _Entity entity);
    void UpdateState(BattleAgent agent, _Entity entity);
    void ExitState(BattleAgent agent, _Entity entity);
}

// 1. Idle (대기) 상태
public class BattleState_Idle : IBattleState
{
    public void EnterState(BattleAgent agent, _Entity entity)
    {
        var animator = entity.GetEntityAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetBool("IsRun", false);
        animator.SetBool("IsWalk", false);
        animator.SetBool("IsInteract", false);
        animator.SetBool("IsMining", false);
    }

    public void UpdateState(BattleAgent agent, _Entity entity)
    {
        // 플레이어가 사정거리 안에 들어오거나 추적해야 한다면 Move나 Attack 상태로 전환하는 로직 가능
    }

    public void ExitState(BattleAgent agent, _Entity entity) { }
}

// 2. Attack (공격) 상태
public class BattleState_Attack : IBattleState
{
    public void EnterState(BattleAgent agent, _Entity entity)
    {
        var animator = entity.GetEntityAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetTrigger("IsAttack");
    }

    public void UpdateState(BattleAgent agent, _Entity entity) { }

    public void ExitState(BattleAgent agent, _Entity entity) { }
}

// 3. Die (사망) 상태
public class BattleState_Die : IBattleState
{
    public void EnterState(BattleAgent agent, _Entity entity)
    {
        var animator = entity.GetEntityAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetBool("IsDead", true);
        // 사망 애니메이션 연출 시간을 벌기 위해 즉시 Destroy하지 않고 지연 처리하거나 코루틴 연동 가능
    }

    public void UpdateState(BattleAgent agent, _Entity entity) { }
    public void ExitState(BattleAgent agent, _Entity entity) { }
}

public class BattleState_Run : IBattleState
{
    public void EnterState(BattleAgent agent, _Entity entity)
    {
        ToggleRunAnimation(entity, true);
    }

    public void UpdateState(BattleAgent agent, _Entity entity) { }
    public void ExitState(BattleAgent agent, _Entity entity)
    {
        ToggleRunAnimation(entity, false);
    }

    private void ToggleRunAnimation(_Entity entity, bool isActive)
    {
        var animator = entity.GetEntityAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetBool("IsRun", isActive);
    }
}

public class BattleState_Walk : IBattleState
{
    public void EnterState(BattleAgent agent, _Entity entity)
    {
        ToggleWalkAnimation(entity, true);
    }

    public void UpdateState(BattleAgent agent, _Entity entity) { }
    public void ExitState(BattleAgent agent, _Entity entity)
    {
        ToggleWalkAnimation(entity, false);
    }

    private void ToggleWalkAnimation(_Entity entity, bool isActive)
    {
        var animator = entity.GetEntityAnimator();
        if (animator == null)
        {
            return;
        }

        animator.SetBool("IsWalk", isActive);
    }
}