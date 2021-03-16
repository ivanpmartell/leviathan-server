using UnityEngine;

internal class ShipCombat_Surround : ShipCombat_Base
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

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		Unit unit = VerifyTarget(owner);
		if (unit == null)
		{
			sm.PopState();
			return;
		}
		Vector3 facing = unit.transform.position - owner.transform.position;
		facing.Normalize();
		owner.ClearMoveOrders();
		Order order = new Order(owner, Order.Type.MoveForward, owner.transform.position);
		order.SetFacing(facing);
		owner.AddOrder(order);
	}
}
