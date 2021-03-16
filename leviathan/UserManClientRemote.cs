using System.Collections.Generic;
using PTech;

internal class UserManClientRemote : UserManClient
{
	private RPC m_rpc;

	private GDPBackend m_gdpBackend;

	private List<ShipDef> m_shipDefs = new List<ShipDef>();

	private List<FleetDef> m_fleetDefs = new List<FleetDef>();

	private List<ContentPack> m_contentPacks = new List<ContentPack>();

	private Dictionary<int, ContentPack> m_campaignContentPacks = new Dictionary<int, ContentPack>();

	private List<KeyValuePair<string, string>> m_unlockedCampaignMaps = new List<KeyValuePair<string, string>>();

	public UserManClientRemote(RPC rpc, GDPBackend gdpBackend)
	{
		m_rpc = rpc;
		m_gdpBackend = gdpBackend;
		m_rpc.Register("FleetList", RPC_FleetList);
		m_rpc.Register("ShipList", RPC_ShipList);
		m_rpc.Register("ContentPack", RPC_ContentPack);
		RequestUpdate();
	}

	private void RequestUpdate()
	{
		m_rpc.Invoke("RequestShips");
		m_rpc.Invoke("RequestFleets");
		m_rpc.Invoke("RequestContentPack");
	}

	public override void SetFlag(int flag)
	{
		m_rpc.Invoke("SetFlag", flag);
	}

	public override List<FleetDef> GetFleetDefs(int campaignID)
	{
		List<FleetDef> list = new List<FleetDef>();
		foreach (FleetDef fleetDef in m_fleetDefs)
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
		foreach (ShipDef shipDef in m_shipDefs)
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
		List<string> list = new List<string>();
		foreach (ContentPack contentPack in m_contentPacks)
		{
			list.AddRange(contentPack.m_maps);
		}
		return Utils.GetDistinctList(list);
	}

	public override List<string> GetAvailableCampaigns()
	{
		List<string> list = new List<string>();
		foreach (ContentPack contentPack in m_contentPacks)
		{
			list.AddRange(contentPack.m_campaigns);
		}
		return list;
	}

	public override List<int> GetAvailableFlags()
	{
		List<int> list = new List<int>();
		foreach (ContentPack contentPack in m_contentPacks)
		{
			list.AddRange(contentPack.m_flags);
		}
		return Utils.GetDistinctList(list);
	}

	public override List<string> GetUnlockedCampaignMaps(string campaign)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> unlockedCampaignMap in m_unlockedCampaignMaps)
		{
			if (unlockedCampaignMap.Key == campaign)
			{
				list.Add(unlockedCampaignMap.Value);
			}
		}
		return list;
	}

	public override List<string> GetAvailableShips(int campaignID)
	{
		if (campaignID <= 0)
		{
			List<string> list = new List<string>();
			foreach (ContentPack contentPack in m_contentPacks)
			{
				list.AddRange(contentPack.m_ships);
			}
			return Utils.GetDistinctList(list);
		}
		if (m_campaignContentPacks.TryGetValue(campaignID, out var value))
		{
			return value.m_ships;
		}
		return null;
	}

	public override List<string> GetAvailableSections(int campaignID)
	{
		if (campaignID <= 0)
		{
			List<string> list = new List<string>();
			foreach (ContentPack contentPack in m_contentPacks)
			{
				list.AddRange(contentPack.m_sections);
			}
			return Utils.GetDistinctList(list);
		}
		if (m_campaignContentPacks.TryGetValue(campaignID, out var value))
		{
			return value.m_sections;
		}
		return null;
	}

	public override List<string> GetAvailableHPModules(int campaignID)
	{
		if (campaignID <= 0)
		{
			List<string> list = new List<string>();
			foreach (ContentPack contentPack in m_contentPacks)
			{
				list.AddRange(contentPack.m_hpmodulse);
			}
			return Utils.GetDistinctList(list);
		}
		if (m_campaignContentPacks.TryGetValue(campaignID, out var value))
		{
			return value.m_hpmodulse;
		}
		return null;
	}

	public override void AddShip(ShipDef ship)
	{
		m_rpc.Invoke("AddShip", ship.ToArray());
	}

	public override void AddFleet(FleetDef fleet)
	{
		m_rpc.Invoke("AddFleet", fleet.ToArray());
	}

	public override void RemoveShip(string shipName)
	{
		m_rpc.Invoke("RemoveShip", shipName);
	}

	public override void RemoveFleet(string fleetName)
	{
		m_rpc.Invoke("RemoveFleet", fleetName);
	}

	public override FleetDef GetFleet(string name, int campaignID)
	{
		foreach (FleetDef fleetDef in m_fleetDefs)
		{
			if (fleetDef.m_name == name && fleetDef.m_campaignID == campaignID)
			{
				return fleetDef;
			}
		}
		return null;
	}

	private void RPC_FleetList(RPC rpc, List<object> args)
	{
		int num = 0;
		int num2 = (int)args[num++];
		m_fleetDefs.Clear();
		for (int i = 0; i < num2; i++)
		{
			byte[] data = args[num++] as byte[];
			FleetDef item = new FleetDef(data);
			m_fleetDefs.Add(item);
		}
		UpdateFleetAvailability(m_fleetDefs);
		if (m_onUpdated != null)
		{
			m_onUpdated();
		}
	}

	private void RPC_ShipList(RPC rpc, List<object> args)
	{
		int num = 0;
		int num2 = (int)args[num++];
		m_shipDefs.Clear();
		for (int i = 0; i < num2; i++)
		{
			byte[] data = args[num++] as byte[];
			ShipDef item = new ShipDef(data);
			m_shipDefs.Add(item);
		}
		UpdateShipAvailability(m_shipDefs);
		if (m_onUpdated != null)
		{
			m_onUpdated();
		}
	}

	private void RPC_ContentPack(RPC rpc, List<object> args)
	{
		m_contentPacks.Clear();
		m_campaignContentPacks.Clear();
		m_unlockedCampaignMaps.Clear();
		int num = 0;
		int num2 = (int)args[num++];
		for (int i = 0; i < num2; i++)
		{
			byte[] data = (byte[])args[num++];
			ContentPack contentPack = new ContentPack();
			contentPack.FromArray(data);
			m_contentPacks.Add(contentPack);
		}
		int num3 = (int)args[num++];
		for (int j = 0; j < num3; j++)
		{
			int key = (int)args[num++];
			byte[] data2 = (byte[])args[num++];
			ContentPack contentPack2 = new ContentPack();
			contentPack2.FromArray(data2);
			m_campaignContentPacks.Add(key, contentPack2);
		}
		int num4 = (int)args[num++];
		for (int k = 0; k < num4; k++)
		{
			KeyValuePair<string, string> item = new KeyValuePair<string, string>((string)args[num++], (string)args[num++]);
			m_unlockedCampaignMaps.Add(item);
		}
		UpdateShipAvailability(m_shipDefs);
		UpdateFleetAvailability(m_fleetDefs);
		if (m_onUpdated != null)
		{
			m_onUpdated();
		}
	}

	public override void AddShipyardTime(float time)
	{
		m_rpc.Invoke("AddShipyardTime", time);
	}

	public override void UnlockAchievement(int id)
	{
		PLog.Log("UnlockAchievement: " + id);
		if (m_gdpBackend != null)
		{
			m_gdpBackend.UnlockAchievement(id);
		}
		m_rpc.Invoke("UnlockAchievement", id);
	}

	public override void BuyPackage(string packageName)
	{
		m_rpc.Invoke("BuyPackage", packageName);
	}

	public override void SetOwnedPackages(List<string> owned)
	{
		string[] item = owned.ToArray();
		List<object> list = new List<object>();
		list.Add(item);
		m_rpc.Invoke("SetOwnedPackages", list);
	}
}
