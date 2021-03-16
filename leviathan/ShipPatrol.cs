using System.IO;
using UnityEngine;

internal class ShipPatrol : AIState<Ship>
{
	private Vector3[] points;

	private int index;

	public override string DebugString(Ship owner)
	{
		return index + "/" + points.Length;
	}

	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
		if (owner.GetAiSettings().GetTarget() != null)
		{
			points = owner.GetAiSettings().GetTarget().GetComponent<Path>()
				.GetPoints();
			return;
		}
		PLog.Log("ShipPatrol:Enter: No path, exit patrol." + owner.GetNetID());
		owner.GetAiSettings().m_mission = ShipAISettings.AiMission.Defend;
		sm.PopState();
	}

	public override void Exit(Ship owner)
	{
		PLog.Log("ShipPatrol:Exit");
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		owner.GetShipAi().FindEnemy(dt);
		if (owner.GetShipAi().HasEnemy())
		{
			sm.PushState("combat");
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
		}
		else
		{
			if (!owner.IsOrdersEmpty())
			{
				return;
			}
			index++;
			if (index == points.Length)
			{
				if (owner.GetAiSettings().m_mission == ShipAISettings.AiMission.Goto)
				{
					owner.GetAiSettings().m_mission = ShipAISettings.AiMission.Defend;
					sm.PopState();
					return;
				}
				index = 0;
			}
			owner.SetOrdersTo(points[index]);
		}
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
