using System.Collections.Generic;
using System.Xml;

public class ComponentDB
{
	private static ComponentDB m_instance;

	private Dictionary<string, HPModuleSettings> m_modules = new Dictionary<string, HPModuleSettings>();

	private Dictionary<string, UnitSettings> m_units = new Dictionary<string, UnitSettings>();

	private Dictionary<string, SectionSettings> m_sections = new Dictionary<string, SectionSettings>();

	public static ComponentDB instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new ComponentDB();
			}
			return m_instance;
		}
	}

	public ComponentDB()
	{
		AddPacksInDir("shared_settings/components");
	}

	public static void ResetInstance()
	{
		m_instance = null;
	}

	public void AddPacksInDir(string dir)
	{
		XmlDocument[] array = Utils.LoadXmlInDirectory(dir);
		XmlDocument[] array2 = array;
		foreach (XmlDocument xmlDoc in array2)
		{
			AddSettings(xmlDoc);
		}
	}

	public void AddSettings(XmlDocument xmlDoc)
	{
		for (XmlNode xmlNode = xmlDoc.FirstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "modules")
			{
				LoadModules(xmlNode);
			}
			else if (xmlNode.Name == "units")
			{
				LoadUnits(xmlNode);
			}
			else if (xmlNode.Name == "sections")
			{
				LoadSections(xmlNode);
			}
		}
	}

	private void LoadModules(XmlNode it)
	{
		for (it = it.FirstChild; it != null; it = it.NextSibling)
		{
			HPModuleSettings hPModuleSettings = HPModuleSettings.FromXml(it);
			m_modules.Add(hPModuleSettings.m_prefab, hPModuleSettings);
		}
	}

	private void LoadUnits(XmlNode it)
	{
		for (it = it.FirstChild; it != null; it = it.NextSibling)
		{
			UnitSettings unitSettings = UnitSettings.FromXml(it);
			m_units.Add(unitSettings.m_prefab, unitSettings);
		}
	}

	private void LoadSections(XmlNode it)
	{
		for (it = it.FirstChild; it != null; it = it.NextSibling)
		{
			SectionSettings sectionSettings = SectionSettings.FromXml(it);
			m_sections.Add(sectionSettings.m_prefab, sectionSettings);
		}
	}

	public UnitSettings GetUnit(string name)
	{
		string key = name;
		if (name.Contains("(Clone)"))
		{
			key = name.Substring(0, name.Length - 7);
		}
		if (m_units.TryGetValue(key, out var value))
		{
			return value;
		}
		PLog.LogError("Failed to find unit " + name);
		return null;
	}

	public HPModuleSettings GetModule(string name)
	{
		if (m_modules.TryGetValue(name, out var value))
		{
			return value;
		}
		PLog.LogError("Failed to find module " + name);
		return null;
	}

	public SectionSettings GetSection(string name)
	{
		if (m_sections.TryGetValue(name, out var value))
		{
			return value;
		}
		PLog.LogError("Failed to find section " + name + " Check that it is included in base_components.xml");
		return null;
	}
}
