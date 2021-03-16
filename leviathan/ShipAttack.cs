using System.Collections.Generic;
using System.IO;
using UnityEngine;

internal class ShipAttack : Ship_Base
{
	public override string DebugString(Ship owner)
	{
		string empty = string.Empty;
		return base.DebugString(owner) + empty;
	}

	public override void Enter(Ship owner, AIStateMachine<Ship> sm)
	{
	}

	public override void Exit(Ship owner)
	{
	}

	public override void Save(BinaryWriter writer)
	{
		base.Save(writer);
	}

	public override void Load(BinaryReader reader)
	{
		base.Load(reader);
	}

	public List<GameObject> GetHumanShips()
	{
		List<GameObject> list = new List<GameObject>();
		List<NetObj> all = NetObj.GetAll();
		foreach (NetObj item in all)
		{
			Ship component = item.GetComponent<Ship>();
			if (component != null && !component.IsDead() && TurnMan.instance.IsHuman(component.GetOwner()))
			{
				list.Add(component.gameObject);
			}
		}
		return list;
	}

	public void PickAttackTarget(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		if (owner.GetAi().VerifyTarget() != null)
		{
			return;
		}
		GameObject gameObject = null;
		GameObject target = owner.GetAiSettings().GetTarget();
		List<GameObject> list = null;
		if ((bool)target)
		{
			list = MNode.GetTargets(target);
			if (list.Count == 0)
			{
				return;
			}
		}
		else
		{
			list = GetHumanShips();
			if (list.Count == 0)
			{
				return;
			}
		}
		int index = PRand.Range(0, list.Count - 1);
		gameObject = list[index];
		if (gameObject.GetComponent<MNSpawn>() != null)
		{
			gameObject = gameObject.GetComponent<MNSpawn>().GetSpawnedShip();
		}
		if (gameObject == null)
		{
			owner.GetAi().m_targetId = -1;
		}
		else
		{
			owner.GetAi().m_targetId = gameObject.GetComponent<NetObj>().GetNetID();
		}
	}

	public override void Update(Ship owner, AIStateMachine<Ship> sm, float dt)
	{
		PickAttackTarget(owner, sm, dt);
		Unit unit = owner.GetAi().VerifyTarget();
		if ((bool)unit)
		{
			owner.GetAi().m_goalPosition = unit.transform.position;
		}
		UpdateMovement(owner, sm, dt);
	}
}
