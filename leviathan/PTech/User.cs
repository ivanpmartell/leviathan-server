using System;
using System.Collections.Generic;
using System.IO;

namespace PTech
{
	public class User
	{
		public enum MailFlags
		{
			AllMail = 1
		}

		public RPC m_rpc;

		public int m_userID = -1;

		public string m_name = string.Empty;

		public int m_gamesCreated;

		public int m_inGames;

		public float m_onlineTime;

		public PlatformType m_platform;

		public int m_mailFlags;

		public bool m_unlockCampaignMaps;

		public int m_flag = 4;

		public UserStats m_stats = new UserStats();

		public List<UserEvent> m_events = new List<UserEvent>();

		public Action<User> m_onShipsUpdate;

		public Action<User> m_onFleetsUpdate;

		public Action<User> m_onContentPackUpdate;

		private List<FleetDef> m_fleetDefs = new List<FleetDef>();

		private List<ShipDef> m_shipDefs = new List<ShipDef>();

		private List<ContentPack> m_contentPacks = new List<ContentPack>();

		private Dictionary<int, ContentPack> m_campaignContentPacks = new Dictionary<int, ContentPack>();

		private List<KeyValuePair<string, string>> m_unlockedCampaignMaps = new List<KeyValuePair<string, string>>();

		public User(string name, int userid)
		{
			m_name = name;
			m_userID = userid;
		}

		public User()
		{
		}

		public bool Update(float dt)
		{
			if (m_rpc != null)
			{
				m_onlineTime += dt;
				return m_rpc.Update(recvAll: false);
			}
			return false;
		}

		public void Connect(RPC rpc, PlatformType platform)
		{
			m_rpc = rpc;
			m_platform = platform;
			m_stats.AddLogin(platform);
		}

		public void Disconnect()
		{
			if (m_rpc != null)
			{
				m_rpc.Close();
				m_rpc = null;
				m_stats.m_totalPlayTime += (long)m_onlineTime;
				m_onlineTime = 0f;
			}
		}

		public bool IsConnected()
		{
			return m_rpc != null;
		}

		public void ClearCampaign(int campaignID)
		{
			if (campaignID <= 0)
			{
				PLog.LogError("Cant clear campaign " + campaignID + " its not a valid campaign ID");
				return;
			}
			m_fleetDefs.RemoveAll((FleetDef item) => item.m_campaignID == campaignID);
			m_shipDefs.RemoveAll((ShipDef item) => item.m_campaignID == campaignID);
		}

		public bool HaveCampaignFleet(int campaignID)
		{
			foreach (FleetDef fleetDef in m_fleetDefs)
			{
				if (fleetDef.m_campaignID == campaignID)
				{
					return true;
				}
			}
			return false;
		}

		public FleetDef GetFleetDef(string name, int campaignID)
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

		public ShipDef GetShipDef(string name, int campaignID)
		{
			foreach (ShipDef shipDef in m_shipDefs)
			{
				if (shipDef.m_name == name && shipDef.m_campaignID == campaignID)
				{
					return shipDef;
				}
			}
			return null;
		}

		public void AddFleetDef(FleetDef newFleet)
		{
			for (int i = 0; i < m_fleetDefs.Count; i++)
			{
				if (m_fleetDefs[i].m_name == newFleet.m_name && m_fleetDefs[i].m_campaignID == newFleet.m_campaignID)
				{
					m_fleetDefs[i] = newFleet;
					if (m_onFleetsUpdate != null)
					{
						m_onFleetsUpdate(this);
					}
					return;
				}
			}
			m_fleetDefs.Add(newFleet);
			if (m_onFleetsUpdate != null)
			{
				m_onFleetsUpdate(this);
			}
		}

		public void AddShipDef(ShipDef newShip)
		{
			for (int i = 0; i < m_shipDefs.Count; i++)
			{
				if (m_shipDefs[i].m_name == newShip.m_name)
				{
					m_shipDefs[i] = newShip;
					if (m_onShipsUpdate != null)
					{
						m_onShipsUpdate(this);
					}
					return;
				}
			}
			m_shipDefs.Add(newShip);
			if (m_onShipsUpdate != null)
			{
				m_onShipsUpdate(this);
			}
		}

		public void RemoveFleetDef(string name)
		{
			for (int i = 0; i < m_fleetDefs.Count; i++)
			{
				if (m_fleetDefs[i].m_name == name)
				{
					m_fleetDefs.RemoveAt(i);
					if (m_onFleetsUpdate != null)
					{
						m_onFleetsUpdate(this);
					}
					break;
				}
			}
		}

		public bool RemoveShipDef(string name)
		{
			for (int i = 0; i < m_shipDefs.Count; i++)
			{
				if (m_shipDefs[i].m_name == name)
				{
					m_shipDefs.RemoveAt(i);
					if (m_onShipsUpdate != null)
					{
						m_onShipsUpdate(this);
					}
					return true;
				}
			}
			return false;
		}

		public List<FleetDef> GetFleetDefs()
		{
			return m_fleetDefs;
		}

		public List<ShipDef> GetShipDefs()
		{
			return m_shipDefs;
		}

		public void SetCampaignContentPack(int campaignID, ContentPack pack)
		{
			m_campaignContentPacks[campaignID] = pack;
			if (m_onContentPackUpdate != null)
			{
				m_onContentPackUpdate(this);
			}
		}

		public void AddContentPack(ContentPack pack, MapMan mapman, bool unlockAllMaps)
		{
			if (!m_contentPacks.Contains(pack))
			{
				m_contentPacks.Add(pack);
				foreach (string campaign in pack.m_campaigns)
				{
					List<MapInfo> campaignMaps = mapman.GetCampaignMaps(campaign);
					if (campaignMaps.Count > 0)
					{
						UnlockCampaignMap(campaign, campaignMaps[0].m_name);
					}
				}
				if (m_onContentPackUpdate != null)
				{
					m_onContentPackUpdate(this);
				}
			}
			if (!unlockAllMaps && !m_unlockCampaignMaps)
			{
				return;
			}
			foreach (string campaign2 in pack.m_campaigns)
			{
				List<MapInfo> campaignMaps2 = mapman.GetCampaignMaps(campaign2);
				foreach (MapInfo item in campaignMaps2)
				{
					UnlockCampaignMap(campaign2, item.m_name);
				}
			}
		}

		public void RemoveAllContentPacks()
		{
			m_contentPacks.Clear();
		}

		public List<string> GetAvailableMaps()
		{
			List<string> list = new List<string>();
			foreach (ContentPack contentPack in m_contentPacks)
			{
				list.AddRange(contentPack.m_maps);
			}
			return list;
		}

		public List<string> GetAvailableCampaigns()
		{
			List<string> list = new List<string>();
			foreach (ContentPack contentPack in m_contentPacks)
			{
				list.AddRange(contentPack.m_campaigns);
			}
			return list;
		}

		public List<string> GetAvailableShips(int campaignID)
		{
			if (campaignID <= 0)
			{
				List<string> list = new List<string>();
				{
					foreach (ContentPack contentPack in m_contentPacks)
					{
						list.AddRange(contentPack.m_ships);
					}
					return list;
				}
			}
			if (m_campaignContentPacks.TryGetValue(campaignID, out var value))
			{
				return value.m_ships;
			}
			return null;
		}

		public List<string> GetAvailableSections(int campaignID)
		{
			if (campaignID <= 0)
			{
				List<string> list = new List<string>();
				{
					foreach (ContentPack contentPack in m_contentPacks)
					{
						list.AddRange(contentPack.m_sections);
					}
					return list;
				}
			}
			if (m_campaignContentPacks.TryGetValue(campaignID, out var value))
			{
				return value.m_sections;
			}
			return null;
		}

		public List<string> GetAvailableHPModules(int campaignID)
		{
			if (campaignID <= 0)
			{
				List<string> list = new List<string>();
				{
					foreach (ContentPack contentPack in m_contentPacks)
					{
						list.AddRange(contentPack.m_hpmodulse);
					}
					return list;
				}
			}
			if (m_campaignContentPacks.TryGetValue(campaignID, out var value))
			{
				return value.m_hpmodulse;
			}
			return null;
		}

		public List<int> GetAvailableFlags()
		{
			List<int> list = new List<int>();
			foreach (ContentPack contentPack in m_contentPacks)
			{
				list.AddRange(contentPack.m_flags);
			}
			return list;
		}

		public bool SetFlag(int flagID)
		{
			foreach (ContentPack contentPack in m_contentPacks)
			{
				if (contentPack.m_flags.Contains(flagID))
				{
					m_flag = flagID;
					return true;
				}
			}
			return false;
		}

		public List<KeyValuePair<string, string>> GetUnlockedCampaignMaps()
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, string> unlockedCampaignMap in m_unlockedCampaignMaps)
			{
				list.Add(unlockedCampaignMap);
			}
			return list;
		}

		public void UnlockCampaignMap(string campaign, string mapName)
		{
			foreach (KeyValuePair<string, string> unlockedCampaignMap in m_unlockedCampaignMaps)
			{
				if (unlockedCampaignMap.Key == campaign && unlockedCampaignMap.Value == mapName)
				{
					return;
				}
			}
			m_unlockedCampaignMaps.Add(new KeyValuePair<string, string>(campaign, mapName));
			if (m_onContentPackUpdate != null)
			{
				m_onContentPackUpdate(this);
			}
		}

		public bool IsCampaignMapUnlocked(string campaign, string map)
		{
			foreach (KeyValuePair<string, string> unlockedCampaignMap in m_unlockedCampaignMaps)
			{
				if (unlockedCampaignMap.Key == campaign && unlockedCampaignMap.Value == map)
				{
					return true;
				}
			}
			return false;
		}

		public List<ContentPack> GetContentPacks()
		{
			return m_contentPacks;
		}

		public Dictionary<int, ContentPack> GetCampaignContentPacks()
		{
			return m_campaignContentPacks;
		}

		public byte[] GetFleetsAsArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_fleetDefs.Count);
			foreach (FleetDef fleetDef in m_fleetDefs)
			{
				fleetDef.Save(binaryWriter);
			}
			return memoryStream.ToArray();
		}

		public void SetFleetsFromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				FleetDef fleetDef = new FleetDef();
				fleetDef.Load(binaryReader);
				foreach (ShipDef ship in fleetDef.m_ships)
				{
					ship.m_value = ShipDefUtils.GetShipValue(ship, ComponentDB.instance);
				}
				fleetDef.UpdateValue();
				m_fleetDefs.Add(fleetDef);
			}
		}

		public byte[] GetShipsAsArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_shipDefs.Count);
			foreach (ShipDef shipDef in m_shipDefs)
			{
				shipDef.Save(binaryWriter);
			}
			return memoryStream.ToArray();
		}

		public void SetShipsFromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				ShipDef shipDef = new ShipDef();
				shipDef.Load(binaryReader);
				shipDef.m_value = ShipDefUtils.GetShipValue(shipDef, ComponentDB.instance);
				m_shipDefs.Add(shipDef);
			}
		}

		public byte[] GetCampaignCPsAsArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_campaignContentPacks.Count);
			foreach (KeyValuePair<int, ContentPack> campaignContentPack in m_campaignContentPacks)
			{
				binaryWriter.Write(campaignContentPack.Key);
				binaryWriter.Write(campaignContentPack.Value.m_name);
			}
			return memoryStream.ToArray();
		}

		public void SetCampaignCPsFromArray(byte[] data, PackMan packman)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				int key = binaryReader.ReadInt32();
				string name = binaryReader.ReadString();
				ContentPack pack = packman.GetPack(name);
				m_campaignContentPacks.Add(key, pack);
			}
		}

		public byte[] GetUnlockedMapsAsArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_unlockedCampaignMaps.Count);
			foreach (KeyValuePair<string, string> unlockedCampaignMap in m_unlockedCampaignMaps)
			{
				binaryWriter.Write(unlockedCampaignMap.Key);
				binaryWriter.Write(unlockedCampaignMap.Value);
			}
			return memoryStream.ToArray();
		}

		public void SetUnlockedMapsFromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string key = binaryReader.ReadString();
				string value = binaryReader.ReadString();
				m_unlockedCampaignMaps.Add(new KeyValuePair<string, string>(key, value));
			}
		}

		public void OfflineSave(BinaryWriter stream)
		{
			stream.Write(1);
			stream.Write(m_name);
			stream.Write(m_flag);
			stream.Write(m_gamesCreated);
			byte[] fleetsAsArray = GetFleetsAsArray();
			stream.Write(fleetsAsArray.Length);
			stream.Write(fleetsAsArray);
			byte[] shipsAsArray = GetShipsAsArray();
			stream.Write(shipsAsArray.Length);
			stream.Write(shipsAsArray);
			byte[] campaignCPsAsArray = GetCampaignCPsAsArray();
			stream.Write(campaignCPsAsArray.Length);
			stream.Write(campaignCPsAsArray);
			byte[] unlockedMapsAsArray = GetUnlockedMapsAsArray();
			stream.Write(unlockedMapsAsArray.Length);
			stream.Write(unlockedMapsAsArray);
		}

		public void OfflineLoad(BinaryReader stream, PackMan packman)
		{
			int num = stream.ReadInt32();
			m_name = stream.ReadString();
			m_flag = stream.ReadInt32();
			m_gamesCreated = stream.ReadInt32();
			int count = stream.ReadInt32();
			byte[] fleetsFromArray = stream.ReadBytes(count);
			SetFleetsFromArray(fleetsFromArray);
			int count2 = stream.ReadInt32();
			byte[] shipsFromArray = stream.ReadBytes(count2);
			SetShipsFromArray(shipsFromArray);
			int count3 = stream.ReadInt32();
			byte[] data = stream.ReadBytes(count3);
			SetCampaignCPsFromArray(data, packman);
			int count4 = stream.ReadInt32();
			byte[] unlockedMapsFromArray = stream.ReadBytes(count4);
			SetUnlockedMapsFromArray(unlockedMapsFromArray);
		}

		public void AddEvent(UserEvent e)
		{
			m_events.Add(e);
		}

		public void ClearEvents()
		{
			m_events.Clear();
		}

		public byte[] GetEventsAsArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_events.Count);
			foreach (UserEvent @event in m_events)
			{
				@event.Save(binaryWriter);
			}
			return memoryStream.ToArray();
		}

		public void SetEventsFromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			m_events.Clear();
			for (int i = 0; i < num; i++)
			{
				UserEvent userEvent = new UserEvent();
				userEvent.Load(binaryReader);
				m_events.Add(userEvent);
			}
		}

		public void SetStatsFromXml(string xml)
		{
			m_stats = new UserStats();
			m_stats.LoadFromXml(xml);
		}

		public string GetStatsAsXml()
		{
			return m_stats.SaveToXml();
		}
	}
}
