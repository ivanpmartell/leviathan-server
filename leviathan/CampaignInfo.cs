using System.Collections.Generic;

public class CampaignInfo
{
	public string m_name = string.Empty;

	public string m_thumbnail = string.Empty;

	public string m_description = string.Empty;

	public bool m_tutorial;

	public List<MapInfo> m_maps = new List<MapInfo>();

	public MapInfo GetMap(string name)
	{
		foreach (MapInfo map in m_maps)
		{
			if (map.m_name == name)
			{
				return map;
			}
		}
		return null;
	}
}
