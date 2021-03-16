using System.Collections.Generic;
using System.Xml;
using PTech;
using UnityEngine;

internal class ShipFactory
{
	private static ShipFactory m_instance;

	private Dictionary<string, ShipDef> m_ships = new Dictionary<string, ShipDef>();

	public static ShipFactory instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new ShipFactory();
			}
			return m_instance;
		}
	}

	public static void ResetInstance()
	{
		m_instance = null;
	}

	public void RegisterShips(string file)
	{
		TextAsset textAsset = Resources.Load(file) as TextAsset;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(textAsset.text);
		ShipDefUtils.LoadFromXMLFile(xmlDocument, m_ships, ComponentDB.instance);
	}

	public void RegisterShip(string name, ShipDef def)
	{
		m_ships.Add(name, def);
	}

	public ShipDef GetShipDef(string name)
	{
		if (m_ships.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public float GetShipWidth(string name)
	{
		ShipDef shipDef = GetShipDef(name);
		if (shipDef == null)
		{
			PLog.LogError("Could not find ship " + name + " in factory");
			return 0f;
		}
		return GetShipWidth(shipDef);
	}

	public static float GetShipWidth(ShipDef def)
	{
		GameObject prefab = ObjectFactory.instance.GetPrefab(def.m_prefab);
		if (prefab == null)
		{
			return 0f;
		}
		Ship component = prefab.GetComponent<Ship>();
		return component.GetWidth();
	}

	public bool ShipExist(string name)
	{
		ShipDef shipDef = GetShipDef(name);
		if (shipDef == null)
		{
			return false;
		}
		return true;
	}

	public GameObject CreateShip(string name, Vector3 pos, Quaternion rot, int owner)
	{
		ShipDef shipDef = GetShipDef(name);
		if (shipDef == null)
		{
			PLog.LogError("Could not find ship " + name + " in factory");
			return null;
		}
		return CreateShip(shipDef, pos, rot, owner);
	}

	public static GameObject CreateShip(ShipDef def, Vector3 pos, Quaternion rot, int owner)
	{
		GameObject gameObject = ObjectFactory.instance.Create(def.m_prefab, pos, rot);
		if (gameObject == null)
		{
			PLog.LogError("Could not find prefab " + def.m_prefab + " for ship " + def.m_name);
			return null;
		}
		Ship component = gameObject.GetComponent<Ship>();
		component.SetName(def.m_name);
		Section section = component.SetSection(Section.SectionType.Front, def.m_frontSection.m_prefab);
		AddHPModules(section, def.m_frontSection.m_modules);
		Section section2 = component.SetSection(Section.SectionType.Mid, def.m_midSection.m_prefab);
		AddHPModules(section2, def.m_midSection.m_modules);
		Section section3 = component.SetSection(Section.SectionType.Rear, def.m_rearSection.m_prefab);
		AddHPModules(section3, def.m_rearSection.m_modules);
		Section section4 = component.SetSection(Section.SectionType.Top, def.m_topSection.m_prefab);
		AddHPModules(section4, def.m_topSection.m_modules);
		component.SetOwner(owner);
		component.ResetStats();
		NetObj[] componentsInChildren = gameObject.GetComponentsInChildren<NetObj>();
		NetObj[] array = componentsInChildren;
		foreach (NetObj netObj in array)
		{
			netObj.SetVisible(visible: false);
		}
		return gameObject;
	}

	private static void AddHPModules(Section section, List<ModuleDef> modules)
	{
		foreach (ModuleDef module in modules)
		{
			Battery battery = section.GetBattery(module.m_battery);
			if (battery == null)
			{
				PLog.LogError("Error , tried to add module to non existing battery: " + module.m_battery + " on section " + section.name);
			}
			else
			{
				battery.AddHPModule(module.m_prefab, module.m_pos.x, module.m_pos.y, module.m_direction);
			}
		}
	}
}
