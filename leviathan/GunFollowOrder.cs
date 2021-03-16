internal class GunFollowOrder : AIState<Gun>
{
	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
	}

	public override void Exit(Gun owner)
	{
	}

	public override void Update(Gun owner, AIStateMachine<Gun> sm, float dt)
	{
		Order firstOrder = owner.GetFirstOrder();
		if (firstOrder == null)
		{
			sm.PopState();
		}
		else
		{
			if (firstOrder.m_type != Order.Type.Fire)
			{
				return;
			}
			GunTarget gunTarget = ((!firstOrder.IsStaticTarget()) ? new GunTarget(firstOrder.GetTargetID(), firstOrder.GetLocalTargetPos()) : new GunTarget(firstOrder.GetLocalTargetPos()));
			if (gunTarget.GetTargetWorldPos(out var worldPos, owner.GetOwnerTeam()))
			{
				if (owner.InFiringCone(worldPos))
				{
					owner.SetTarget(gunTarget);
					sm.PushState("aim");
					if (!owner.IsLastOrder(firstOrder) || !owner.GetBarrage())
					{
						owner.RemoveFirstOrder();
					}
				}
				else if (owner.GetRemoveInvalidTarget())
				{
					owner.RemoveFirstOrder();
				}
			}
			else
			{
				owner.RemoveFirstOrder();
			}
		}
	}
}
