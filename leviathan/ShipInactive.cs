internal class ShipInactive : AIState<Ship>
{
	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
		owner.GetAi().m_inactive = true;
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		owner.GetAi().m_inactive = true;
	}
}
