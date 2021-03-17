using System.Collections.Generic;
using System.Xml;
using PTech;

internal class FleetDefUtils
{
	private static int GetShipValue(List<ShipDef> ships, string name)
	{
		foreach (ShipDef ship in ships)
		{
			if (ship.m_name == name)
			{
				return ship.m_value;
			}
		}
		return -1;
	}

	public static void LoadFleetsAndShipsXMLFile(XmlDocument xmlDoc, out List<FleetDef> fleets, out List<ShipDef> blueprints, ComponentDB cdb)
	{
		fleets = new List<FleetDef>();
		blueprints = new List<ShipDef>();
		for (XmlNode xmlNode = xmlDoc.FirstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "fleet")
			{
				FleetDef item = LoadFleet(xmlNode, cdb);
				fleets.Add(item);
			}
			if (xmlNode.Name == "blueprint")
			{
				ShipDef shipDef = new ShipDef();
				shipDef.Load(xmlNode);
				shipDef.m_value = ShipDefUtils.GetShipValue(shipDef, cdb);
				blueprints.Add(shipDef);
			}
		}
	}

	public static FleetDef LoadFleet(XmlNode root, ComponentDB cdb)
	{
		FleetDef fleetDef = new FleetDef();
		fleetDef.m_name = root.Attributes["name"].Value;
		for (XmlNode xmlNode = root.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "ship")
			{
				ShipDef shipDef = new ShipDef();
				shipDef.Load(xmlNode);
				shipDef.m_value = ShipDefUtils.GetShipValue(shipDef, cdb);
				fleetDef.m_ships.Add(shipDef);
			}
		}
		fleetDef.UpdateValue();
		return fleetDef;
	}

	public static Dictionary<string, int> GetModuleUsage(FleetDef fleet)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (ShipDef ship in fleet.m_ships)
		{
			List<string> hardpointNames = ship.GetHardpointNames();
			foreach (string item in hardpointNames)
			{
				if (dictionary.TryGetValue(item, out var value))
				{
					dictionary[item] = value + 1;
				}
				else
				{
					dictionary.Add(item, 1);
				}
			}
		}
		return dictionary;
	}

	public static Dictionary<string, int> GetShipUsage(FleetDef fleet)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (ShipDef ship in fleet.m_ships)
		{
			if (dictionary.TryGetValue(ship.m_prefab, out var value))
			{
				dictionary[ship.m_prefab] = value + 1;
			}
			else
			{
				dictionary.Add(ship.m_prefab, 1);
			}
		}
		return dictionary;
	}
}
