using System.Collections.Generic;
using UnityEngine;

internal class GameUtils
{
	public static bool FindAveragePlayerPos(int playerID, out Vector3 pos)
	{
		List<NetObj> all = NetObj.GetAll();
		pos = new Vector3(0f, 0f, 0f);
		int num = 0;
		foreach (NetObj item in all)
		{
			if (item.GetOwner() == playerID)
			{
				num++;
				pos += item.transform.position;
			}
		}
		pos /= (float)num;
		return num > 0;
	}

	public static bool FindCameraStartPos(int playerID, out Vector3 pos)
	{
		List<NetObj> all = NetObj.GetAll();
		pos = new Vector3(0f, 0f, 0f);
		int num = -1;
		foreach (NetObj item in all)
		{
			if (item.GetOwner() != playerID)
			{
				continue;
			}
			Ship ship = item as Ship;
			if (ship != null)
			{
				int totalValue = ship.GetTotalValue();
				if (!ship.IsDead() && totalValue > num)
				{
					num = totalValue;
					pos = item.transform.position;
				}
			}
		}
		if (num < 0)
		{
			return false;
		}
		return true;
	}
}
