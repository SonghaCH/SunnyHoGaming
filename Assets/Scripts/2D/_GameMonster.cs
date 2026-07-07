using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[Obsolete("2D용 레거시", false)]
public class _GameMonster : BattleAgent
{
    [SerializeField] private SpriteRenderer SpriteRenderer_Monster;

    private Vector3 _moveDirection;

    protected override void ChangeMonsterDirection()
    {
        _lookRight = !_lookRight;
        _moveDirection = new Vector3(_lookRight ? 1 : -1, 0, 0);
        SetMeshDirectionByMoveDirection((int)_moveDirection.x);
    }


    void SetMeshDirectionByMoveDirection(int x)
    {
        SpriteRenderer_Monster.flipX = (x < 0);
    }
}
