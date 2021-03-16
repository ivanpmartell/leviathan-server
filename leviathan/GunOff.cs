internal class GunOff : AIState<Gun>
{
	public override void Enter(Gun owner, AIStateMachine<Gun> sm)
	{
		owner.SetTarget(null);
	}

	public override string GetStatusText()
	{
		return "Offline";
	}

	public override void Update(Gun owner, AIStateMachine<Gun> sm, float dt)
	{
	}
}
