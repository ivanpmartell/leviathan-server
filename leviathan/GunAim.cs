internal class GunAim : AIState<Gun>
{
	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
	}

	public override string GetStatusText()
	{
		return "Aiming ";
	}

	public override void Update(Gun owner, AIStateMachine<Gun> sm, float dt)
	{
		GunTarget target = owner.GetTarget();
		if (target != null && owner.GetOptimalTargetPosition(target, out var targetPos))
		{
			if (!owner.InFiringCone(targetPos))
			{
				sm.PopState();
				return;
			}
			Unit unit = target.GetTargetObject() as Unit;
			if (unit != null)
			{
				Ship ship = owner.GetUnit() as Ship;
				if (ship != null)
				{
					ship.GetAi().m_targetId = unit.GetNetID();
				}
			}
			if (owner.AimAt(targetPos))
			{
				if (owner.IsContinuous())
				{
					sm.ChangeState("firebeam");
				}
				else
				{
					sm.ChangeState("fire");
				}
			}
		}
		else
		{
			PLog.Log("target is gone, returning to previus state");
			sm.PopState();
		}
	}
}
