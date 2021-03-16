using System.Collections.Generic;
using System.Xml;
using PTech;

public class MapMan
{
	private List<CampaignInfo> m_campaigns = new List<CampaignInfo>();

	private List<MapInfo> m_skirmish = new List<MapInfo>();

	private List<MapInfo> m_assassination = new List<MapInfo>();

	private List<MapInfo> m_challenge = new List<MapInfo>();

	private List<MapInfo> m_custom = new List<MapInfo>();

	public void AddLevels(XmlDocument xmlDoc)
	{
		XmlNode firstChild = xmlDoc.FirstChild;
		XmlNode root = firstChild.SelectSingleNode("skirmish");
		AddMaps(root, GameType.Points, m_skirmish);
		XmlNode root2 = firstChild.SelectSingleNode("challenge");
		AddMaps(root2, GameType.Challenge, m_challenge);
		XmlNode root3 = firstChild.SelectSingleNode("custom");
		AddMaps(root3, GameType.Custom, m_custom);
		XmlNode root4 = firstChild.SelectSingleNode("assassination");
		AddMaps(root4, GameType.Assassination, m_assassination);
		XmlNode root5 = firstChild.SelectSingleNode("campaigns");
		AddCampaigns(root5);
		PLog.Log("added levels, skirmish:" + m_skirmish.Count + " challenge: " + m_challenge.Count + " campaigns: " + m_campaigns.Count);
	}

	private void AddCampaigns(XmlNode root)
	{
		for (XmlNode xmlNode = root.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "campaign")
			{
				CampaignInfo campaignInfo = new CampaignInfo();
				campaignInfo.m_name = xmlNode.Attributes["name"].Value;
				campaignInfo.m_thumbnail = xmlNode.Attributes["thumbnail"].Value;
				if (xmlNode.Attributes["description"] != null)
				{
					campaignInfo.m_description = xmlNode.Attributes["description"].Value;
				}
				if (xmlNode.Attributes["tutorial"] != null)
				{
					campaignInfo.m_tutorial = bool.Parse(xmlNode.Attributes["tutorial"].Value);
				}
				AddMaps(xmlNode, GameType.Campaign, campaignInfo.m_maps);
				m_campaigns.Add(campaignInfo);
			}
		}
	}

	private void AddMaps(XmlNode root, GameType type, List<MapInfo> mapList)
	{
		for (XmlNode xmlNode = root.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "map")
			{
				MapInfo mapInfo = new MapInfo();
				mapInfo.m_gameMode = type;
				mapInfo.m_name = xmlNode.Attributes["name"].Value;
				mapInfo.m_scene = xmlNode.Attributes["scene"].Value;
				mapInfo.m_thumbnail = xmlNode.Attributes["thumbnail"].Value;
				mapInfo.m_player = int.Parse(xmlNode.Attributes["players"].Value);
				mapInfo.m_size = int.Parse(xmlNode.Attributes["size"].Value);
				if (xmlNode.Attributes["nofleet"] != null)
				{
					mapInfo.m_noFleet = bool.Parse(xmlNode.Attributes["nofleet"].Value);
				}
				if (xmlNode.Attributes["loadscreen"] != null)
				{
					mapInfo.m_loadscreen = xmlNode.Attributes["loadscreen"].Value;
				}
				if (xmlNode.Attributes["description"] != null)
				{
					mapInfo.m_description = xmlNode.Attributes["description"].Value;
				}
				if (xmlNode.Attributes["briefdescription"] != null)
				{
					mapInfo.m_briefDescription = xmlNode.Attributes["briefdescription"].Value;
				}
				if (xmlNode.Attributes["fleetlimit"] != null)
				{
					int min = ((xmlNode.Attributes["fleetmin"] == null) ? 1 : int.Parse(xmlNode.Attributes["fleetmin"].Value));
					int max = int.Parse(xmlNode.Attributes["fleetlimit"].Value);
					mapInfo.m_fleetLimit = new FleetSize(min, max);
				}
				if (xmlNode.Attributes["defaults"] != null)
				{
					mapInfo.m_defaults = xmlNode.Attributes["defaults"].Value;
				}
				if (xmlNode.Attributes["content"] != null)
				{
					mapInfo.m_contentPack = xmlNode.Attributes["content"].Value;
				}
				mapList.Add(mapInfo);
			}
		}
	}

	public MapInfo GetMapByName(GameType mode, string campaign, string name)
	{
		switch (mode)
		{
		case GameType.Points:
			foreach (MapInfo item in m_skirmish)
			{
				if (item.m_name == name)
				{
					return item;
				}
			}
			break;
		case GameType.Assassination:
			foreach (MapInfo item2 in m_assassination)
			{
				if (item2.m_name == name)
				{
					return item2;
				}
			}
			break;
		case GameType.Challenge:
			foreach (MapInfo item3 in m_challenge)
			{
				if (item3.m_name == name)
				{
					return item3;
				}
			}
			break;
		case GameType.Campaign:
		{
			CampaignInfo campaign2 = GetCampaign(campaign);
			if (campaign2 != null)
			{
				return campaign2.GetMap(name);
			}
			break;
		}
		case GameType.Custom:
			foreach (MapInfo item4 in m_custom)
			{
				if (item4.m_name == name)
				{
					return item4;
				}
			}
			break;
		}
		return null;
	}

	public CampaignInfo GetCampaign(string campaign)
	{
		foreach (CampaignInfo campaign2 in m_campaigns)
		{
			if (campaign2.m_name == campaign)
			{
				return campaign2;
			}
		}
		return null;
	}

	public List<MapInfo> GetChallengeMaps()
	{
		return m_challenge;
	}

	public List<MapInfo> GetCustomMaps()
	{
		return m_custom;
	}

	public List<MapInfo> GetSkirmishMaps()
	{
		return m_skirmish;
	}

	public List<MapInfo> GetAssassinationMaps()
	{
		return m_assassination;
	}

	public List<MapInfo> GetCampaignMaps(string campaign)
	{
		CampaignInfo campaign2 = GetCampaign(campaign);
		if (campaign2 != null)
		{
			return campaign2.m_maps;
		}
		return new List<MapInfo>();
	}

	public List<CampaignInfo> GetCampaigns()
	{
		return m_campaigns;
	}

	public MapInfo GetNextCampaignMap(string campaign, string map)
	{
		CampaignInfo campaign2 = GetCampaign(campaign);
		for (int i = 0; i < campaign2.m_maps.Count; i++)
		{
			if (campaign2.m_maps[i].m_name == map && i + 1 < campaign2.m_maps.Count)
			{
				return campaign2.m_maps[i + 1];
			}
		}
		return null;
	}
}
