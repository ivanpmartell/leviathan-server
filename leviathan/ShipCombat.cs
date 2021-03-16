internal class ShipCombat : ShipCombat_Base
{
	public override string DebugString(Ship owner)
	{
		return base.DebugString(owner);
	}

	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
		owner.GetAiSettings().RunOnCombatEvent();
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		sm.ChangeState("c_driveby");
	}
}
