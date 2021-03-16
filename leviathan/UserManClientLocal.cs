using System.Collections.Generic;
using PTech;

internal class UserManClientLocal : UserManClient
{
	private User m_user;

	private PackMan m_packMan;

	private MapMan m_mapMan;

	private GDPBackend m_gdpBackend;

	public UserManClientLocal(User user, PackMan packMan, MapMan mapMan, GDPBackend gdpBackend)
	{
		m_user = user;
		m_packMan = packMan;
		m_mapMan = mapMan;
		m_gdpBackend = gdpBackend;
	}

	public override List<FleetDef> GetFleetDefs(int campaignID)
	{
		List<FleetDef> list = new List<FleetDef>();
		foreach (FleetDef fleetDef in m_user.GetFleetDefs())
		{
			if (fleetDef.m_campaignID == campaignID)
			{
				list.Add(fleetDef);
			}
		}
		return list;
	}

	public override List<ShipDef> GetShipDefs(int campaignID)
	{
		List<ShipDef> list = new List<ShipDef>();
		foreach (ShipDef shipDef in m_user.GetShipDefs())
		{
			if (shipDef.m_campaignID == campaignID)
			{
				list.Add(shipDef);
			}
		}
		return list;
	}

	public override List<string> GetAvailableMaps()
	{
		return Utils.GetDistinctList(m_user.GetAvailableMaps());
	}

	public override List<int> GetAvailableFlags()
	{
		return Utils.GetDistinctList(m_user.GetAvailableFlags());
	}

	public override List<string> GetUnlockedCampaignMaps(string campaign)
	{
		List<KeyValuePair<string, string>> unlockedCampaignMaps = m_user.GetUnlockedCampaignMaps();
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> item in unlockedCampaignMaps)
		{
			if (item.Key == campaign)
			{
				list.Add(item.Value);
			}
		}
		return list;
	}

	public override List<string> GetAvailableCampaigns()
	{
		return m_user.GetAvailableCampaigns();
	}

	public override List<string> GetAvailableShips(int campaignID)
	{
		return Utils.GetDistinctList(m_user.GetAvailableShips(campaignID));
	}

	public override List<string> GetAvailableSections(int campaignID)
	{
		return Utils.GetDistinctList(m_user.GetAvailableSections(campaignID));
	}

	public override List<string> GetAvailableHPModules(int campaignID)
	{
		return Utils.GetDistinctList(m_user.GetAvailableHPModules(campaignID));
	}

	public override void AddShip(ShipDef ship)
	{
		m_user.AddShipDef(ship);
		if (m_onUpdated != null)
		{
			m_onUpdated();
		}
	}

	public override void AddFleet(FleetDef fleet)
	{
		m_user.AddFleetDef(fleet);
		if (m_onUpdated != null)
		{
			m_onUpdated();
		}
	}

	public override void RemoveShip(string name)
	{
		m_user.RemoveShipDef(name);
		if (m_onUpdated != null)
		{
			m_onUpdated();
		}
	}

	public override void RemoveFleet(string fleet)
	{
		m_user.RemoveFleetDef(fleet);
		if (m_onUpdated != null)
		{
			m_onUpdated();
		}
	}

	public override FleetDef GetFleet(string name, int campaignID)
	{
		return m_user.GetFleetDef(name, campaignID);
	}

	public override void SetFlag(int flag)
	{
		m_user.SetFlag(flag);
	}

	public override void AddShipyardTime(float time)
	{
		m_user.m_stats.m_totalShipyardTime += (long)time;
	}

	public override void UnlockAchievement(int id)
	{
		PLog.Log("UnlockAchievement: " + id);
		if (m_gdpBackend != null)
		{
			m_gdpBackend.UnlockAchievement(id);
		}
		m_user.m_stats.UnlockAchievement(id);
	}

	public override void BuyPackage(string packageName)
	{
	}

	private void UnlockContentPack(string packageName)
	{
		PLog.Log("unlocking content pack " + packageName);
		bool unlockAllMaps = false;
		ContentPack pack = m_packMan.GetPack(packageName);
		if (pack != null)
		{
			m_user.AddContentPack(pack, m_mapMan, unlockAllMaps);
		}
		else
		{
			PLog.LogError("Tried to unlock missing content pack " + packageName);
		}
	}

	public void ResetContentPacks()
	{
		m_user.RemoveAllContentPacks();
		UnlockContentPack("Base");
	}

	public override void SetOwnedPackages(List<string> owned)
	{
		ResetContentPacks();
		foreach (string item in owned)
		{
			UnlockContentPack(item);
		}
		UpdateShipAvailability(m_user.GetShipDefs());
		UpdateFleetAvailability(m_user.GetFleetDefs());
	}
}
