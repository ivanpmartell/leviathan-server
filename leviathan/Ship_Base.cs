using System.IO;
using UnityEngine;

internal class Ship_Base : AIState<Ship>
{
	public override string DebugString(Ship owner)
	{
		return string.Empty;
	}

	public override void Save(BinaryWriter writer)
	{
	}

	public override void Load(BinaryReader reader)
	{
	}

	public void UpdateMovement(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		Vector3? goalPosition = owner.GetAi().m_goalPosition;
		if (!goalPosition.HasValue)
		{
			owner.ClearMoveOrders();
			Vector3? goalFacing = owner.GetAi().m_goalFacing;
			if (goalFacing.HasValue)
			{
				Order order = new Order(owner, Order.Type.MoveRotate, owner.transform.position);
				order.SetFacing(owner.GetAi().m_goalFacing.Value);
				owner.AddOrder(order);
			}
		}
		else
		{
			if (owner.IsOrdersEmpty())
			{
				owner.SetOrdersTo(owner.GetAi().m_goalPosition.Value);
			}
			if (owner.IsBlocked())
			{
				owner.ClearMoveOrders();
			}
		}
	}
}
