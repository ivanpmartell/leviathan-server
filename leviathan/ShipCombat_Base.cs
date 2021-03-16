internal class ShipCombat_Base : AIState<Ship>
{
	public override string DebugString(Ship owner)
	{
		Unit unit = NetObj.GetByID(owner.GetAi().m_targetId) as Unit;
		string empty = string.Empty;
		empty = empty + owner.GetAi().m_targetId + "\n";
		empty = empty + "-   Dist: " + RangeToTarget(owner) + "\n";
		empty = empty + "-   Target is Forward = " + owner.IsPositionForward(unit.transform.position);
		return empty + " / Right = " + owner.IsPositionRight(unit.transform.position);
	}

	public bool SwitchState(Ship owner, AIStateMachine<Ship> sm)
	{
		Unit unit = VerifyTarget(owner);
		if (unit == null)
		{
			sm.ChangeState("combat");
			return false;
		}
		return false;
	}

	public Unit VerifyTarget(Ship owner)
	{
		Unit unit = NetObj.GetByID(owner.GetAi().m_targetId) as Unit;
		if (unit == null)
		{
			owner.GetAi().m_targetId = 0;
			return null;
		}
		return unit;
	}

	public float RangeToTarget(Ship owner)
	{
		Unit unit = NetObj.GetByID(owner.GetAi().m_targetId) as Unit;
		return (unit.transform.position - owner.transform.position).magnitude;
	}
}
