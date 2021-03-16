internal class GunThink : AIState<Gun>
{
	private bool m_noAmmo;

	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
		owner.SetTarget(null);
	}

	public override string GetStatusText()
	{
		if (m_noAmmo)
		{
			return "Out of ammo";
		}
		return "Playing cards";
	}

	public override void Update(Gun owner, AIStateMachine<Gun> sm, float dt)
	{
		Unit unit = owner.GetUnit();
		if (unit.GetAi().m_inactive)
		{
			sm.ChangeState("off");
			return;
		}
		if (owner.GetLoadedSalvo() == 0f && owner.GetAmmo() == 0)
		{
			m_noAmmo = true;
			return;
		}
		m_noAmmo = false;
		if (owner.m_canDeploy)
		{
			if (owner.GetDeploy())
			{
				sm.PushState("deploy");
			}
		}
		else if (owner.GetFirstOrder() != null)
		{
			sm.PushState("followorder");
		}
		else
		{
			sm.PushState("guard");
		}
	}
}
