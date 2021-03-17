using System.Xml;

public class SectionSettings
{
	public string m_prefab = string.Empty;

	public int m_value;

	public static SectionSettings FromXml(XmlNode node)
	{
		SectionSettings sectionSettings = new SectionSettings();
		sectionSettings.m_prefab = node.Attributes["prefab"].Value;
		sectionSettings.m_value = int.Parse(node.Attributes["value"].Value);
		return sectionSettings;
	}
}
