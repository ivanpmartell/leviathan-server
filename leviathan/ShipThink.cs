internal class ShipThink : AIState<Ship>
{
	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		if (owner.GetOwner() <= 3)
		{
			sm.PushState("human");
			return;
		}
		if (owner.GetAiSettings().m_mission == ShipAISettings.AiMission.Inactive)
		{
			sm.PushState("inactive");
		}
		if (owner.GetAiSettings().m_mission == ShipAISettings.AiMission.Defend)
		{
			sm.PushState("guard");
		}
		if (owner.GetAiSettings().m_mission == ShipAISettings.AiMission.Patrol)
		{
			sm.PushState("patrol");
		}
		if (owner.GetAiSettings().m_mission == ShipAISettings.AiMission.Goto)
		{
			sm.PushState("patrol");
		}
		if (owner.GetAiSettings().m_mission == ShipAISettings.AiMission.Attack)
		{
			sm.PushState("attack");
		}
		if (owner.GetAiSettings().m_mission == ShipAISettings.AiMission.BossC1M3)
		{
			sm.PushState("BossC1M3");
		}
	}
}
