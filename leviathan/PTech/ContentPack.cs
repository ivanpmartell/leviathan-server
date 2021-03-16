using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PTech
{
	public class ContentPack
	{
		public enum Category
		{
			None = 0,
			Maps = 1,
			Ships = 2,
			Campaigns = 4,
			Flags = 8
		}

		public string m_name = string.Empty;

		public string m_description = string.Empty;

		public bool m_dev;

		public bool m_newItem;

		public Category m_type;

		public List<string> m_maps = new List<string>();

		public List<string> m_campaigns = new List<string>();

		public List<string> m_ships = new List<string>();

		public List<string> m_sections = new List<string>();

		public List<string> m_hpmodulse = new List<string>();

		public List<int> m_flags = new List<int>();

		private Category CategoryFromString(string str)
		{
			return str switch
			{
				"maps" => Category.Maps, 
				"ships" => Category.Ships, 
				"campaign" => Category.Campaigns, 
				"flags" => Category.Flags, 
				_ => Category.None, 
			};
		}

		public void Load(XmlDocument xmlDoc)
		{
			for (XmlNode xmlNode = xmlDoc.FirstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.Name == "name")
				{
					m_name = xmlNode.FirstChild.Value;
				}
				else if (xmlNode.Name == "dev")
				{
					m_dev = bool.Parse(xmlNode.FirstChild.Value);
				}
				else if (xmlNode.Name == "new")
				{
					m_newItem = bool.Parse(xmlNode.FirstChild.Value);
				}
				else if (xmlNode.Name == "type")
				{
					m_type = CategoryFromString(xmlNode.FirstChild.Value);
				}
				else if (xmlNode.Name == "description")
				{
					m_description = xmlNode.FirstChild.Value;
				}
				else if (xmlNode.Name == "maps")
				{
					LoadMaps(xmlNode);
				}
				else if (xmlNode.Name == "campaigns")
				{
					LoadCampaigns(xmlNode);
				}
				else if (xmlNode.Name == "ships")
				{
					LoadShips(xmlNode);
				}
				else if (xmlNode.Name == "sections")
				{
					LoadSections(xmlNode);
				}
				else if (xmlNode.Name == "hpmodules")
				{
					LoadHPModules(xmlNode);
				}
				else if (xmlNode.Name == "flags")
				{
					LoadFlags(xmlNode);
				}
			}
		}

		public byte[] ToArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_name);
			binaryWriter.Write((byte)m_type);
			binaryWriter.Write(m_description);
			binaryWriter.Write(m_campaigns.Count);
			foreach (string campaign in m_campaigns)
			{
				binaryWriter.Write(campaign);
			}
			binaryWriter.Write(m_maps.Count);
			foreach (string map in m_maps)
			{
				binaryWriter.Write(map);
			}
			binaryWriter.Write(m_hpmodulse.Count);
			foreach (string item in m_hpmodulse)
			{
				binaryWriter.Write(item);
			}
			binaryWriter.Write(m_sections.Count);
			foreach (string section in m_sections)
			{
				binaryWriter.Write(section);
			}
			binaryWriter.Write(m_ships.Count);
			foreach (string ship in m_ships)
			{
				binaryWriter.Write(ship);
			}
			binaryWriter.Write((short)m_flags.Count);
			foreach (int flag in m_flags)
			{
				binaryWriter.Write((short)flag);
			}
			return memoryStream.ToArray();
		}

		public void FromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			m_name = binaryReader.ReadString();
			m_type = (Category)binaryReader.ReadByte();
			m_description = binaryReader.ReadString();
			m_campaigns.Clear();
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				m_campaigns.Add(binaryReader.ReadString());
			}
			m_maps.Clear();
			int num2 = binaryReader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				m_maps.Add(binaryReader.ReadString());
			}
			m_hpmodulse.Clear();
			int num3 = binaryReader.ReadInt32();
			for (int k = 0; k < num3; k++)
			{
				m_hpmodulse.Add(binaryReader.ReadString());
			}
			m_sections.Clear();
			int num4 = binaryReader.ReadInt32();
			for (int l = 0; l < num4; l++)
			{
				m_sections.Add(binaryReader.ReadString());
			}
			m_ships.Clear();
			int num5 = binaryReader.ReadInt32();
			for (int m = 0; m < num5; m++)
			{
				m_ships.Add(binaryReader.ReadString());
			}
			m_flags.Clear();
			int num6 = binaryReader.ReadInt16();
			for (int n = 0; n < num6; n++)
			{
				m_flags.Add(binaryReader.ReadInt16());
			}
		}

		private void LoadCampaigns(XmlNode parent)
		{
			for (XmlNode xmlNode = parent.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.Name == "campaign")
				{
					m_campaigns.Add(xmlNode.Attributes["name"].Value);
				}
			}
		}

		private void LoadMaps(XmlNode parent)
		{
			for (XmlNode xmlNode = parent.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.Name == "map")
				{
					m_maps.Add(xmlNode.Attributes["name"].Value);
				}
			}
		}

		private void LoadShips(XmlNode parent)
		{
			for (XmlNode xmlNode = parent.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.Name == "ship")
				{
					m_ships.Add(xmlNode.Attributes["prefab"].Value);
				}
			}
		}

		private void LoadSections(XmlNode parent)
		{
			for (XmlNode xmlNode = parent.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.Name == "section")
				{
					m_sections.Add(xmlNode.Attributes["prefab"].Value);
				}
			}
		}

		private void LoadHPModules(XmlNode parent)
		{
			for (XmlNode xmlNode = parent.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.Name == "hpmodule")
				{
					m_hpmodulse.Add(xmlNode.Attributes["prefab"].Value);
				}
			}
		}

		private void LoadFlags(XmlNode parent)
		{
			for (XmlNode xmlNode = parent.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.Name == "flag")
				{
					m_flags.Add(int.Parse(xmlNode.Attributes["id"].Value));
				}
			}
		}
	}
}
