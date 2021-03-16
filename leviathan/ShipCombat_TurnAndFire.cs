using UnityEngine;

internal class ShipCombat_TurnAndFire : ShipCombat_Base
{
	public override string DebugString(Ship owner)
	{
		return base.DebugString(owner);
	}

	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
		Unit unit = NetObj.GetByID(owner.GetAi().m_targetId) as Unit;
		Vector3 facing = unit.transform.position - owner.transform.position;
		facing.Normalize();
		owner.ClearMoveOrders();
		Order order = new Order(owner, Order.Type.MoveForward, owner.transform.position);
		order.SetFacing(facing);
		owner.AddOrder(order);
	}

	private bool CheckChickenRun(Ship owner, Unit Target)
	{
		return true;
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		if (SwitchState(owner, sm))
		{
			return;
		}
		Unit unit = VerifyTarget(owner);
		float optimalAttackRange = owner.GetShipAi().GetOptimalAttackRange(UnitAi.AttackDirection.None);
		float num = RangeToTarget(owner);
		if (Mathf.Abs(num - optimalAttackRange) > 50f)
		{
			if (CheckChickenRun(owner, unit))
			{
				sm.ChangeState("c_chicken");
			}
			else
			{
				sm.ChangeState("c_driveby");
			}
		}
		else if (owner.GetShipAi().GetAttackDirection(unit.transform.position) != 0)
		{
			Vector3 facing = unit.transform.position - owner.transform.position;
			facing.Normalize();
			owner.ClearMoveOrders();
			Order order = new Order(owner, Order.Type.MoveForward, owner.transform.position);
			order.SetFacing(facing);
			owner.AddOrder(order);
		}
	}
}
