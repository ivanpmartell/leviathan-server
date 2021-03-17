using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace PTech
{
	public class UserStats
	{
		public class ModuleStat
		{
			public int m_uses;

			public int m_damage;
		}

		public DateTime m_lastLoginTime = default(DateTime);

		public int m_pcLogins;

		public int m_iosLogins;

		public int m_androidLogins;

		public int m_osxLogins;

		public int m_otherLogins;

		public int m_vsGamesWon;

		public int m_vsPointsWon;

		public int m_vsAssWon;

		public int m_vsGamesLost;

		public int m_vsPointsLost;

		public int m_vsAssLost;

		public int m_vsTotalDamage;

		public int m_vsTotalFriendlyDamage;

		public int m_vsShipsSunk;

		public Dictionary<string, ModuleStat> m_vsModuleStats = new Dictionary<string, ModuleStat>();

		public Dictionary<string, int> m_vsShipUsage = new Dictionary<string, int>();

		public long m_totalPlayTime;

		public long m_totalPlanningTime;

		public long m_totalShipyardTime;

		public Dictionary<int, long> m_achievements = new Dictionary<int, long>();

		public string SaveToXml()
		{
			StringWriterWithEncoding stringWriterWithEncoding = new StringWriterWithEncoding(Encoding.UTF8);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			xmlWriterSettings.IndentChars = "  ";
			xmlWriterSettings.NewLineChars = "\r\n";
			xmlWriterSettings.NewLineHandling = NewLineHandling.Replace;
			xmlWriterSettings.Encoding = Encoding.UTF8;
			XmlWriter xmlWriter = XmlWriter.Create(stringWriterWithEncoding, xmlWriterSettings);
			xmlWriter.WriteStartElement("root");
			xmlWriter.WriteElementString("lastLogin", m_lastLoginTime.ToString("yyyy-MM-d HH:mm"));
			xmlWriter.WriteElementString("pcLogins", m_pcLogins.ToString());
			xmlWriter.WriteElementString("iosLogins", m_iosLogins.ToString());
			xmlWriter.WriteElementString("androidLogins", m_androidLogins.ToString());
			xmlWriter.WriteElementString("osxLogins", m_osxLogins.ToString());
			xmlWriter.WriteElementString("otherLogins", m_otherLogins.ToString());
			xmlWriter.WriteElementString("vsGamesWon", m_vsGamesWon.ToString());
			xmlWriter.WriteElementString("vsPointsWon", m_vsPointsWon.ToString());
			xmlWriter.WriteElementString("vsAssWon", m_vsAssWon.ToString());
			xmlWriter.WriteElementString("vsGamesLost", m_vsGamesLost.ToString());
			xmlWriter.WriteElementString("vsPointsLost", m_vsPointsLost.ToString());
			xmlWriter.WriteElementString("vsAssLost", m_vsAssLost.ToString());
			xmlWriter.WriteElementString("vsTotalDamage", m_vsTotalDamage.ToString());
			xmlWriter.WriteElementString("vsTotalFriendlyDamage", m_vsTotalFriendlyDamage.ToString());
			xmlWriter.WriteElementString("vsShipsSunk", m_vsShipsSunk.ToString());
			xmlWriter.WriteElementString("totalPlayTime", m_totalPlayTime.ToString());
			xmlWriter.WriteElementString("totalPlanningTime", m_totalPlanningTime.ToString());
			xmlWriter.WriteElementString("totalShipyardTime", m_totalShipyardTime.ToString());
			xmlWriter.WriteStartElement("vsModuleStats");
			foreach (KeyValuePair<string, ModuleStat> vsModuleStat in m_vsModuleStats)
			{
				xmlWriter.WriteStartElement("module");
				xmlWriter.WriteAttributeString("name", vsModuleStat.Key);
				xmlWriter.WriteAttributeString("uses", vsModuleStat.Value.m_uses.ToString());
				xmlWriter.WriteAttributeString("damage", vsModuleStat.Value.m_damage.ToString());
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("vsShipUsage");
			foreach (KeyValuePair<string, int> item in m_vsShipUsage)
			{
				xmlWriter.WriteElementString(item.Key, item.Value.ToString());
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("achievements");
			foreach (KeyValuePair<int, long> achievement in m_achievements)
			{
				xmlWriter.WriteStartElement("achievement");
				xmlWriter.WriteAttributeString("id", achievement.Key.ToString());
				xmlWriter.WriteAttributeString("date", achievement.Value.ToString());
				xmlWriter.WriteAttributeString("TextDate", DateTime.FromBinary(achievement.Value).ToString("yyyy-MM-d HH:mm"));
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			xmlWriter.Flush();
			return stringWriterWithEncoding.ToString();
		}

		public void LoadFromXml(string str)
		{
			TextReader reader = new StringReader(str);
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.IgnoreComments = true;
			XmlReader xmlReader = XmlReader.Create(reader, xmlReaderSettings);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(xmlReader);
			for (XmlNode xmlNode = xmlDocument.FirstChild.NextSibling.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
			{
				if (xmlNode.Name == "pcLogins")
				{
					m_pcLogins = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "iosLogins")
				{
					m_iosLogins = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "androidLogins")
				{
					m_androidLogins = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "osxLogins")
				{
					m_osxLogins = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "otherLogins")
				{
					m_otherLogins = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsGamesWon")
				{
					m_vsGamesWon = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsPointsWon")
				{
					m_vsPointsLost = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsAssWon")
				{
					m_vsAssWon = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsGamesLost")
				{
					m_vsGamesLost = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsPointsLost")
				{
					m_vsPointsLost = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsAssLost")
				{
					m_vsAssLost = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsTotalDamage")
				{
					m_vsTotalDamage = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsTotalFriendlyDamage")
				{
					m_vsTotalFriendlyDamage = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsShipsSunk")
				{
					m_vsShipsSunk = int.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "totalPlayTime")
				{
					m_totalPlayTime = long.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "totalPlanningTime")
				{
					m_totalPlanningTime = long.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "totalShipyardTime")
				{
					m_totalShipyardTime = long.Parse(xmlNode.FirstChild.Value);
				}
				if (xmlNode.Name == "vsModuleStats")
				{
					for (XmlElement xmlElement = xmlNode.FirstChild as XmlElement; xmlElement != null; xmlElement = xmlElement.NextSibling as XmlElement)
					{
						string attribute = xmlElement.GetAttribute("name");
						ModuleStat moduleStat = new ModuleStat();
						moduleStat.m_uses = int.Parse(xmlElement.GetAttribute("uses"));
						moduleStat.m_damage = int.Parse(xmlElement.GetAttribute("damage"));
						m_vsModuleStats.Add(attribute, moduleStat);
					}
				}
				if (xmlNode.Name == "vsShipUsage")
				{
					for (XmlNode xmlNode2 = xmlNode.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
					{
						string name = xmlNode2.Name;
						int value = int.Parse(xmlNode2.FirstChild.Value);
						m_vsShipUsage.Add(name, value);
					}
				}
				if (xmlNode.Name == "achievements")
				{
					for (XmlElement xmlElement2 = xmlNode.FirstChild as XmlElement; xmlElement2 != null; xmlElement2 = xmlElement2.NextSibling as XmlElement)
					{
						int key = int.Parse(xmlElement2.GetAttribute("id"));
						long value2 = long.Parse(xmlElement2.GetAttribute("date"));
						m_achievements.Add(key, value2);
					}
				}
			}
		}

		public byte[] ToArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_lastLoginTime.ToBinary());
			binaryWriter.Write(m_pcLogins);
			binaryWriter.Write(m_iosLogins);
			binaryWriter.Write(m_androidLogins);
			binaryWriter.Write(m_osxLogins);
			binaryWriter.Write(m_otherLogins);
			binaryWriter.Write(m_totalPlayTime);
			binaryWriter.Write(m_totalPlanningTime);
			binaryWriter.Write(m_totalShipyardTime);
			binaryWriter.Write(m_vsGamesLost);
			binaryWriter.Write(m_vsPointsLost);
			binaryWriter.Write(m_vsAssLost);
			binaryWriter.Write(m_vsGamesWon);
			binaryWriter.Write(m_vsPointsWon);
			binaryWriter.Write(m_vsAssWon);
			binaryWriter.Write(m_vsShipsSunk);
			binaryWriter.Write(m_vsTotalDamage);
			binaryWriter.Write(m_vsTotalFriendlyDamage);
			binaryWriter.Write(m_vsModuleStats.Count);
			foreach (KeyValuePair<string, ModuleStat> vsModuleStat in m_vsModuleStats)
			{
				binaryWriter.Write(vsModuleStat.Key);
				binaryWriter.Write(vsModuleStat.Value.m_damage);
				binaryWriter.Write(vsModuleStat.Value.m_uses);
			}
			binaryWriter.Write(m_vsShipUsage.Count);
			foreach (KeyValuePair<string, int> item in m_vsShipUsage)
			{
				binaryWriter.Write(item.Key);
				binaryWriter.Write(item.Value);
			}
			binaryWriter.Write(m_achievements.Count);
			foreach (KeyValuePair<int, long> achievement in m_achievements)
			{
				binaryWriter.Write((short)achievement.Key);
				binaryWriter.Write(achievement.Value);
			}
			return memoryStream.ToArray();
		}

		public void FromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			m_lastLoginTime = DateTime.FromBinary(binaryReader.ReadInt64());
			m_pcLogins = binaryReader.ReadInt32();
			m_iosLogins = binaryReader.ReadInt32();
			m_androidLogins = binaryReader.ReadInt32();
			m_osxLogins = binaryReader.ReadInt32();
			m_otherLogins = binaryReader.ReadInt32();
			m_totalPlayTime = binaryReader.ReadInt64();
			m_totalPlanningTime = binaryReader.ReadInt64();
			m_totalShipyardTime = binaryReader.ReadInt64();
			m_vsGamesLost = binaryReader.ReadInt32();
			m_vsPointsLost = binaryReader.ReadInt32();
			m_vsAssLost = binaryReader.ReadInt32();
			m_vsGamesWon = binaryReader.ReadInt32();
			m_vsPointsWon = binaryReader.ReadInt32();
			m_vsAssWon = binaryReader.ReadInt32();
			m_vsShipsSunk = binaryReader.ReadInt32();
			m_vsTotalDamage = binaryReader.ReadInt32();
			m_vsTotalFriendlyDamage = binaryReader.ReadInt32();
			int num = binaryReader.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				string key = binaryReader.ReadString();
				ModuleStat moduleStat = new ModuleStat();
				moduleStat.m_damage = binaryReader.ReadInt32();
				moduleStat.m_uses = binaryReader.ReadInt32();
				m_vsModuleStats.Add(key, moduleStat);
			}
			int num2 = binaryReader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				string key2 = binaryReader.ReadString();
				int value = binaryReader.ReadInt32();
				m_vsShipUsage.Add(key2, value);
			}
			int num3 = binaryReader.ReadInt32();
			for (int k = 0; k < num3; k++)
			{
				int key3 = binaryReader.ReadInt16();
				long value2 = binaryReader.ReadInt64();
				m_achievements.Add(key3, value2);
			}
		}

		public void AddModuleUsage(string module, int uses)
		{
			if (m_vsModuleStats.TryGetValue(module, out var value))
			{
				value.m_uses += uses;
				return;
			}
			value = new ModuleStat();
			value.m_uses = uses;
			m_vsModuleStats.Add(module, value);
		}

		public void AddModuleDamage(string module, int damage)
		{
			if (m_vsModuleStats.TryGetValue(module, out var value))
			{
				value.m_damage += damage;
				return;
			}
			value = new ModuleStat();
			value.m_damage = damage;
			m_vsModuleStats.Add(module, value);
		}

		public void AddModuleDamages(Dictionary<string, int> damages)
		{
			foreach (KeyValuePair<string, int> damage in damages)
			{
				AddModuleDamage(damage.Key, damage.Value);
			}
		}

		public void AddShipUsage(string module, int newUses)
		{
			if (m_vsShipUsage.TryGetValue(module, out var value))
			{
				Dictionary<string, int> vsShipUsage;
				Dictionary<string, int> dictionary = (vsShipUsage = m_vsShipUsage);
				string key;
				string key2 = (key = module);
				int num = vsShipUsage[key];
				dictionary[key2] = num + value;
			}
			else
			{
				m_vsShipUsage.Add(module, newUses);
			}
		}

		public void AddLogin(PlatformType platform)
		{
			m_lastLoginTime = DateTime.Now;
			switch (platform)
			{
			case PlatformType.WindowsPC:
				m_pcLogins++;
				break;
			case PlatformType.Ios:
				m_iosLogins++;
				break;
			case PlatformType.Android:
				m_androidLogins++;
				break;
			case PlatformType.Osx:
				m_osxLogins++;
				break;
			case PlatformType.Other:
				m_otherLogins++;
				break;
			}
		}

		public bool UnlockAchievement(int id)
		{
			if (m_achievements.ContainsKey(id))
			{
				return false;
			}
			DateTime now = DateTime.Now;
			m_achievements.Add(id, now.ToBinary());
			return true;
		}
	}
}
