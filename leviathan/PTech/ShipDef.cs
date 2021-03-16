using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PTech
{
	public class ShipDef
	{
		public string m_name;

		public int m_campaignID;

		public string m_prefab;

		public int m_value;

		public bool m_available = true;

		public SectionDef m_frontSection;

		public SectionDef m_midSection;

		public SectionDef m_rearSection;

		public SectionDef m_topSection;

		public ShipDef()
		{
		}

		public ShipDef(byte[] data)
		{
			FromArray(data);
		}

		public void Load(XmlNode xmlFile)
		{
			m_name = xmlFile.Attributes["name"].Value;
			m_prefab = xmlFile.Attributes["prefab"].Value;
			XmlNode node = xmlFile.SelectSingleNode("front");
			m_frontSection = new SectionDef();
			LoadSection(node, m_frontSection);
			XmlNode node2 = xmlFile.SelectSingleNode("mid");
			m_midSection = new SectionDef();
			LoadSection(node2, m_midSection);
			XmlNode node3 = xmlFile.SelectSingleNode("rear");
			m_rearSection = new SectionDef();
			LoadSection(node3, m_rearSection);
			XmlNode node4 = xmlFile.SelectSingleNode("top");
			m_topSection = new SectionDef();
			LoadSection(node4, m_topSection);
		}

		private void LoadSection(XmlNode node, SectionDef section)
		{
			section.m_prefab = node.Attributes["prefab"].Value;
			for (XmlNode xmlNode = node.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.NodeType != XmlNodeType.Comment && xmlNode.Name == "module")
				{
					string value = xmlNode.Attributes["prefab"].Value;
					int battery = int.Parse(xmlNode.Attributes["battery"].Value);
					int x = int.Parse(xmlNode.Attributes["x"].Value);
					int y = int.Parse(xmlNode.Attributes["y"].Value);
					Direction direction = (Direction)int.Parse(xmlNode.Attributes["dir"].Value);
					section.m_modules.Add(new ModuleDef(value, battery, new Vector2i(x, y), direction));
				}
			}
		}

		private void SaveSection(XmlWriter writer, SectionDef section, string sectionName)
		{
			writer.WriteStartElement(sectionName);
			writer.WriteAttributeString("prefab", section.m_prefab);
			foreach (ModuleDef module in section.m_modules)
			{
				writer.WriteStartElement("module");
				writer.WriteAttributeString("prefab", module.m_prefab);
				writer.WriteAttributeString("battery", module.m_battery.ToString());
				writer.WriteAttributeString("x", module.m_pos.x.ToString());
				writer.WriteAttributeString("y", module.m_pos.y.ToString());
				int direction = (int)module.m_direction;
				writer.WriteAttributeString("dir", direction.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteStartElement("ship");
			writer.WriteAttributeString("name", m_name);
			writer.WriteAttributeString("prefab", m_prefab);
			SaveSection(writer, m_frontSection, "front");
			SaveSection(writer, m_midSection, "mid");
			SaveSection(writer, m_rearSection, "rear");
			SaveSection(writer, m_topSection, "top");
			writer.WriteEndElement();
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(m_name);
			writer.Write(m_campaignID);
			writer.Write(m_prefab);
			writer.Write(m_value);
			SaveSection(writer, m_frontSection);
			SaveSection(writer, m_midSection);
			SaveSection(writer, m_rearSection);
			SaveSection(writer, m_topSection);
		}

		private void SaveSection(BinaryWriter writer, SectionDef section)
		{
			writer.Write(section.m_prefab);
			writer.Write(section.m_modules.Count);
			foreach (ModuleDef module in section.m_modules)
			{
				writer.Write(module.m_prefab);
				writer.Write(module.m_battery);
				writer.Write(module.m_pos.x);
				writer.Write(module.m_pos.y);
				writer.Write((int)module.m_direction);
			}
		}

		public void Load(BinaryReader reader)
		{
			m_name = reader.ReadString();
			m_campaignID = reader.ReadInt32();
			m_prefab = reader.ReadString();
			m_value = reader.ReadInt32();
			m_frontSection = new SectionDef();
			LoadSection(reader, m_frontSection);
			m_midSection = new SectionDef();
			LoadSection(reader, m_midSection);
			m_rearSection = new SectionDef();
			LoadSection(reader, m_rearSection);
			m_topSection = new SectionDef();
			LoadSection(reader, m_topSection);
		}

		private void LoadSection(BinaryReader reader, SectionDef section)
		{
			section.m_prefab = reader.ReadString();
			int num = reader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string prefab = reader.ReadString();
				int battery = reader.ReadInt32();
				Vector2i pos = new Vector2i(reader.ReadInt32(), reader.ReadInt32());
				Direction direction = (Direction)reader.ReadInt32();
				section.m_modules.Add(new ModuleDef(prefab, battery, pos, direction));
			}
		}

		public byte[] ToArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(memoryStream);
			Save(writer);
			return memoryStream.ToArray();
		}

		public void FromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader reader = new BinaryReader(input);
			Load(reader);
		}

		public bool IsValid(User user, ComponentDB cdb)
		{
			List<string> availableShips = user.GetAvailableShips(m_campaignID);
			List<string> availableSections = user.GetAvailableSections(m_campaignID);
			List<string> availableHPModules = user.GetAvailableHPModules(m_campaignID);
			if (!availableShips.Contains(m_prefab))
			{
				return false;
			}
			if (!IsSectionValid(m_frontSection, availableSections, availableHPModules))
			{
				return false;
			}
			if (!IsSectionValid(m_midSection, availableSections, availableHPModules))
			{
				return false;
			}
			if (!IsSectionValid(m_rearSection, availableSections, availableHPModules))
			{
				return false;
			}
			if (!IsSectionValid(m_topSection, availableSections, availableHPModules))
			{
				return false;
			}
			int shipValue = ShipDefUtils.GetShipValue(this, cdb);
			if (shipValue != m_value)
			{
				PLog.LogError("Player " + user.m_name + " uploaded ship " + m_name + " with value missmatch , user value " + m_value + "  server value " + shipValue);
				return false;
			}
			return true;
		}

		public void UpdateAvailability(ComponentDB cdb, List<string> ships, List<string> sections, List<string> modules)
		{
			m_available = true;
			if (!ships.Contains(m_prefab))
			{
				m_available = false;
			}
			else if (!IsSectionValid(m_frontSection, sections, modules))
			{
				m_available = false;
			}
			else if (!IsSectionValid(m_midSection, sections, modules))
			{
				m_available = false;
			}
			else if (!IsSectionValid(m_rearSection, sections, modules))
			{
				m_available = false;
			}
			else if (!IsSectionValid(m_topSection, sections, modules))
			{
				m_available = false;
			}
		}

		private bool IsSectionValid(SectionDef section, List<string> sections, List<string> modules)
		{
			if (!sections.Contains(section.m_prefab))
			{
				PLog.LogWarning(" missing section " + section.m_prefab);
				return false;
			}
			foreach (ModuleDef module in section.m_modules)
			{
				if (!modules.Contains(module.m_prefab))
				{
					PLog.LogWarning(" missing module " + module.m_prefab);
					return false;
				}
			}
			return true;
		}

		public int NumberOfHardpoints()
		{
			int num = 0;
			num += m_frontSection.m_modules.Count;
			num += m_midSection.m_modules.Count;
			num += m_rearSection.m_modules.Count;
			return num + m_topSection.m_modules.Count;
		}

		public List<string> GetHardpointNames()
		{
			List<string> list = new List<string>();
			list.AddRange(m_frontSection.GetHardpointNames());
			list.AddRange(m_midSection.GetHardpointNames());
			list.AddRange(m_rearSection.GetHardpointNames());
			list.AddRange(m_topSection.GetHardpointNames());
			return list;
		}

		public ShipDef Clone()
		{
			byte[] data = ToArray();
			ShipDef shipDef = new ShipDef(data);
			shipDef.m_available = m_available;
			return shipDef;
		}
	}
}
