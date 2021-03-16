internal class ShipCombat_ChickenRace : ShipCombat_Base
{
	public override string DebugString(Ship owner)
	{
		return string.Empty;
	}

	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
		Unit unit = VerifyTarget(owner);
		if (unit == null)
		{
			sm.ChangeState("combat");
		}
		else
		{
			owner.SetOrdersTo(unit.transform.position);
		}
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		Unit unit = VerifyTarget(owner);
		if (unit == null)
		{
			sm.ChangeState("combat");
		}
		else if (RangeToTarget(owner) < 60f)
		{
			owner.ClearMoveOrders();
			sm.ChangeState("c_driveby");
		}
		else
		{
			owner.ClearMoveOrders();
			Order order = new Order(owner, Order.Type.MoveForward, unit.transform.position);
			owner.AddOrder(order);
		}
	}
}
