internal class ShipFollow : AIState<Ship>
{
	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		PLog.Log("Folllooow");
	}
}
