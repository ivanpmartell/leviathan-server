using System.IO;
using UnityEngine;

internal class ShipBossC1m3 : AIState<Ship>
{
	private Vector3[] points;

	private int index;

	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
		if (owner.GetAiSettings().GetTarget() != null)
		{
			points = owner.GetAiSettings().GetTarget().GetComponent<Path>()
				.GetPoints();
		}
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		float num = owner.GetHealth();
		float num2 = owner.GetMaxHealth();
		float num3 = num / num2;
		if ((double)num3 > 0.96)
		{
			return;
		}
		if (owner.GetPath() != null)
		{
			points = owner.GetPath().GetComponent<Path>().GetPoints();
		}
		if (points == null)
		{
			return;
		}
		if (index >= points.Length || index < 0)
		{
			PLog.Log("ShipPatrol::Update Out of range index " + index + " Count " + points.Length);
			index = 0;
			return;
		}
		Vector3 vector = points[index];
		float num4 = Utils.DistanceXZ(owner.transform.position, vector);
		if (!(num4 < owner.GetLength() / 2f + 1f))
		{
			Order order = new Order(owner, Order.Type.MoveForward, vector);
			owner.ClearMoveOrders();
			owner.AddOrder(order);
			return;
		}
		index++;
		if (index == points.Length)
		{
			index = 0;
		}
		Order order2 = new Order(owner, Order.Type.MoveForward, points[index]);
		owner.ClearMoveOrders();
		owner.AddOrder(order2);
	}

	public override void Save(BinaryWriter writer)
	{
		writer.Write(index);
		writer.Write(points.Length);
		Vector3[] array = points;
		for (int i = 0; i < array.Length; i++)
		{
			Vector3 vector = array[i];
			writer.Write(vector.x);
			writer.Write(vector.y);
			writer.Write(vector.z);
		}
	}

	public override void Load(BinaryReader reader)
	{
		index = reader.ReadInt32();
		int num = reader.ReadInt32();
		points = new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			points[i].x = reader.ReadSingle();
			points[i].y = reader.ReadSingle();
			points[i].z = reader.ReadSingle();
		}
	}
}
