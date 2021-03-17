using System.Xml;

public class UnitSettings
{
	public string m_prefab = string.Empty;

	public int m_value;

	public static UnitSettings FromXml(XmlNode node)
	{
		UnitSettings unitSettings = new UnitSettings();
		unitSettings.m_prefab = node.Attributes["prefab"].Value;
		unitSettings.m_value = int.Parse(node.Attributes["value"].Value);
		return unitSettings;
	}
}
