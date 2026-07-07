using System;
using Unity.Behavior;

[BlackboardEnum]
public enum BTState
{
	Idle,
	Patrol,
	Wandering,
	Chase,
	Attack
}
