using System.Collections.Generic;
using System.IO;

namespace PTech
{
	public class FleetDef
	{
		public string m_name = string.Empty;

		public int m_campaignID;

		public int m_value;

		public bool m_available = true;

		public List<ShipDef> m_ships = new List<ShipDef>();

		public FleetDef()
		{
		}

		public FleetDef(byte[] data)
		{
			FromArray(data);
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(m_name);
			writer.Write(m_campaignID);
			writer.Write(m_value);
			writer.Write(m_ships.Count);
			foreach (ShipDef ship in m_ships)
			{
				ship.Save(writer);
			}
		}

		public void Load(BinaryReader reader)
		{
			m_name = reader.ReadString();
			m_campaignID = reader.ReadInt32();
			m_value = reader.ReadInt32();
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				ShipDef shipDef = new ShipDef();
				shipDef.Load(reader);
				m_ships.Add(shipDef);
			}
		}

		public byte[] ToArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(memoryStream);
			Save(writer);
			return memoryStream.ToArray();
		}

		public void FromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader reader = new BinaryReader(input);
			Load(reader);
		}

		public bool IsValid(User user, ComponentDB cdb)
		{
			int num = 0;
			foreach (ShipDef ship in m_ships)
			{
				ship.IsValid(user, cdb);
				num += ship.m_value;
			}
			if (m_value != num)
			{
				PLog.LogWarning("Fleet value is invalid");
				return false;
			}
			return true;
		}

		public void UpdateAvailability(ComponentDB cdb, List<string> ships, List<string> sections, List<string> modules)
		{
			m_available = true;
			foreach (ShipDef ship in m_ships)
			{
				ship.UpdateAvailability(cdb, ships, sections, modules);
				if (!ship.m_available)
				{
					m_available = false;
				}
			}
		}

		public void UpdateValue()
		{
			m_value = 0;
			foreach (ShipDef ship in m_ships)
			{
				m_value += ship.m_value;
			}
		}

		public FleetDef Clone()
		{
			FleetDef fleetDef = new FleetDef();
			fleetDef.m_name = m_name;
			fleetDef.m_campaignID = m_campaignID;
			fleetDef.m_value = m_value;
			foreach (ShipDef ship in m_ships)
			{
				fleetDef.m_ships.Add(ship.Clone());
			}
			return fleetDef;
		}
	}
}
