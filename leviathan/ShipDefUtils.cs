using System.Collections.Generic;
using System.Xml;
using PTech;

internal class ShipDefUtils
{
	public static void LoadFromXMLFile(XmlDocument xmlDoc, Dictionary<string, ShipDef> ships, ComponentDB cdb)
	{
		for (XmlNode xmlNode = xmlDoc.FirstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "ship")
			{
				ShipDef shipDef = new ShipDef();
				shipDef.Load(xmlNode);
				shipDef.m_value = GetShipValue(shipDef, cdb);
				ships[shipDef.m_name] = shipDef;
			}
		}
	}

	public static int GetShipValue(ShipDef ship, ComponentDB cdb)
	{
		int num = 0;
		UnitSettings unit = cdb.GetUnit(ship.m_prefab);
		if (unit == null)
		{
			PLog.LogError("Missing prefab in cdb: " + ship.m_prefab);
			return -1;
		}
		num += unit.m_value;
		num += GetSectionValue(ship.m_frontSection, cdb);
		num += GetSectionValue(ship.m_midSection, cdb);
		num += GetSectionValue(ship.m_rearSection, cdb);
		return num + GetSectionValue(ship.m_topSection, cdb);
	}

	private static int GetSectionValue(SectionDef section, ComponentDB cdb)
	{
		int num = 0;
		SectionSettings section2 = cdb.GetSection(section.m_prefab);
		if (section2 == null)
		{
			PLog.LogError("Missing section prefab in cdb:" + section.m_prefab);
			return -1;
		}
		num += section2.m_value;
		foreach (ModuleDef module2 in section.m_modules)
		{
			HPModuleSettings module = cdb.GetModule(module2.m_prefab);
			if (module == null)
			{
				PLog.LogError("Missing module prefab in cdb:" + module2.m_prefab);
				return -1;
			}
			num += module.m_value;
		}
		return num;
	}
}
