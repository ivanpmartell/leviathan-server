using System.Collections.Generic;
using System.Xml;
using PTech;

public class PackMan
{
	private Dictionary<string, ContentPack> m_packs = new Dictionary<string, ContentPack>();

	public PackMan()
	{
		AddPacksInDir("shared_settings/packs");
		AddPacksInDir("shared_settings/campaign_packs");
	}

	public void AddPacksInDir(string dir)
	{
		XmlDocument[] array = Utils.LoadXmlInDirectory(dir);
		XmlDocument[] array2 = array;
		foreach (XmlDocument xmlDoc in array2)
		{
			ContentPack contentPack = new ContentPack();
			contentPack.Load(xmlDoc);
			AddPack(contentPack);
		}
	}

	public ContentPack GetPack(string name)
	{
		if (m_packs.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public ContentPack[] GetAllPacks()
	{
		List<ContentPack> list = new List<ContentPack>();
		foreach (KeyValuePair<string, ContentPack> pack in m_packs)
		{
			list.Add(pack.Value);
		}
		return list.ToArray();
	}

	public int GetTotalNrOfFlags()
	{
		int num = 0;
		foreach (KeyValuePair<string, ContentPack> pack in m_packs)
		{
			num += pack.Value.m_flags.Count;
		}
		return num;
	}

	private void AddPack(ContentPack pack)
	{
		m_packs.Add(pack.m_name, pack);
	}
}
