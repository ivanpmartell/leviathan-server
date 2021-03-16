internal class ShipGuard : AIState<Ship>
{
	public override string DebugString(Ship owner)
	{
		return string.Empty;
	}

	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
	}

	public override void Exit(Ship owner)
	{
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		owner.GetShipAi().FindEnemy(dt);
		if (owner.GetShipAi().HasEnemy())
		{
			sm.PushState("combat");
		}
	}
}
