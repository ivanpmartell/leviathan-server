using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ShipAi : UnitAi
{
	public Ship m_ship;

	public override void SetTargetId(Unit target)
	{
		TurnMan instance = TurnMan.instance;
		if (instance.IsHostile(m_ship.GetOwner(), target.GetOwner()))
		{
			m_targetId = target.GetNetID();
		}
	}

	public void FindEnemy(float dt)
	{
		m_nextScan -= dt;
		if (m_nextScan > 0f)
		{
			return;
		}
		m_nextScan = 2f;
		List<NetObj> all = NetObj.GetAll();
		TurnMan instance = TurnMan.instance;
		for (int i = 0; i < all.Count; i++)
		{
			Ship ship = all[i] as Ship;
			if (!(ship == null) && !ship.IsDead() && ship.IsValidTarget() && instance.IsHostile(m_ship.GetOwner(), ship.GetOwner()))
			{
				float num = Vector3.Distance(m_ship.transform.position, ship.transform.position);
				if (!(num > m_ship.GetSightRange()) && m_ship.TestLOS(ship))
				{
					m_targetId = ship.GetNetID();
				}
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
	}

	public override AttackDirection GetAttackDirection(Vector3 position)
	{
		Vector3 to = position - m_ship.transform.position;
		to.y = 0f;
		to.Normalize();
		float num = Vector3.Angle(m_ship.transform.forward, to);
		if (num <= 45f)
		{
			return AttackDirection.Front;
		}
		num = Vector3.Angle(-m_ship.transform.forward, to);
		if (num <= 45f)
		{
			return AttackDirection.Back;
		}
		num = Vector3.Angle(m_ship.transform.right, to);
		if (num <= 45f)
		{
			return AttackDirection.Right;
		}
		num = Vector3.Angle(-m_ship.transform.right, to);
		if (num <= 45f)
		{
			return AttackDirection.Left;
		}
		return AttackDirection.None;
	}

	public AttackDirection GetModuleDirection(HPModule module)
	{
		Vector3 forward = module.transform.forward;
		float num = Vector3.Angle(m_ship.transform.forward, forward);
		if (num <= 45f)
		{
			return AttackDirection.Front;
		}
		num = Vector3.Angle(-m_ship.transform.forward, forward);
		if (num <= 45f)
		{
			return AttackDirection.Back;
		}
		num = Vector3.Angle(m_ship.transform.right, forward);
		if (num <= 45f)
		{
			return AttackDirection.Right;
		}
		num = Vector3.Angle(-m_ship.transform.right, forward);
		if (num <= 45f)
		{
			return AttackDirection.Left;
		}
		return AttackDirection.Front;
	}

	public AttackDirection GetOptimalSide()
	{
		int[] array = new int[4];
		HPModule[] componentsInChildren = m_ship.GetComponentsInChildren<HPModule>();
		HPModule[] array2 = componentsInChildren;
		foreach (HPModule hPModule in array2)
		{
			if (hPModule.m_type == HPModule.HPModuleType.Offensive)
			{
				AttackDirection moduleDirection = GetModuleDirection(hPModule);
				array[(int)moduleDirection]++;
			}
		}
		AttackDirection result = AttackDirection.Front;
		int num = 0;
		for (int j = 0; j < 4; j++)
		{
			if (array[j] > num)
			{
				result = (AttackDirection)j;
				num = array[j];
			}
		}
		return result;
	}

	public List<HPModule> GetModulesOnSide(AttackDirection side)
	{
		List<HPModule> list = new List<HPModule>();
		HPModule[] componentsInChildren = m_ship.GetComponentsInChildren<HPModule>();
		HPModule[] array = componentsInChildren;
		foreach (HPModule hPModule in array)
		{
			if (GetModuleDirection(hPModule) == side && hPModule.m_type == HPModule.HPModuleType.Offensive)
			{
				list.Add(hPModule);
			}
		}
		return list;
	}

	public float GetOptimalAttackRange(AttackDirection side)
	{
		List<HPModule> modulesOnSide = GetModulesOnSide(side);
		foreach (HPModule item in modulesOnSide)
		{
		}
		return 100f;
	}
}
