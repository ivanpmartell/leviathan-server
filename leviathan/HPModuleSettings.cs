using System.Xml;

public class HPModuleSettings
{
	public string m_prefab = string.Empty;

	public int m_value;

	public static HPModuleSettings FromXml(XmlNode node)
	{
		HPModuleSettings hPModuleSettings = new HPModuleSettings();
		hPModuleSettings.m_prefab = node.Attributes["prefab"].Value;
		hPModuleSettings.m_value = int.Parse(node.Attributes["value"].Value);
		return hPModuleSettings;
	}
}
