using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

internal class Localize
{
	private static Localize m_instance;

	private Dictionary<string, string> m_translations = new Dictionary<string, string>();

	private string m_language;

	public static Localize instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new Localize();
			}
			return m_instance;
		}
	}

	public string GetLanguage()
	{
		return m_language;
	}

	public bool SetLanguage(string language)
	{
		//Discarded unreachable code: IL_009f
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.IgnoreComments = true;
		m_language = language;
		m_translations.Clear();
		Object[] array = Resources.LoadAll("localization/" + language);
		Object[] array2 = array;
		foreach (Object @object in array2)
		{
			TextAsset textAsset = @object as TextAsset;
			if (!(textAsset == null))
			{
				XmlReader xmlReader = XmlReader.Create(new StringReader(textAsset.text), xmlReaderSettings);
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.Load(xmlReader);
				}
				catch (XmlException ex)
				{
					PLog.LogError("Parse error " + ex.ToString());
					continue;
				}
				if (!AddTranslation(xmlDocument))
				{
					PLog.LogError("Error adding localization file " + @object.name);
				}
			}
		}
		PLog.Log("nr of translation entries: " + m_translations.Count);
		return true;
	}

	private bool AddTranslation(XmlDocument xmlDoc)
	{
		for (XmlNode xmlNode = xmlDoc.FirstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			XmlNode xmlNode2 = xmlNode.Attributes["text"];
			if (xmlNode2 == null)
			{
				PLog.LogError("missing text attribute in node " + xmlNode.Name + " in file " + xmlDoc.Name);
				xmlNode = xmlNode.NextSibling;
				return false;
			}
			string value = xmlNode2.Value.Replace("\\n", "\n");
			m_translations[xmlNode.Name] = value;
		}
		return true;
	}

	private bool IsBreakCharacter(char c)
	{
		if (c == ' ' || c == '\n' || c == ':' || c == '.' || c == ',' || c == '!')
		{
			return true;
		}
		return false;
	}

	public string TranslateMacros(string text, Dictionary<string, string> macros)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int length = text.Length;
		int num = -1;
		for (int i = 0; i < length; i++)
		{
			char c = text[i];
			if (i == length - 1 && num != -1)
			{
				string text2 = text.Substring(num);
				if (IsBreakCharacter(c))
				{
					text2 = text.Substring(num, i - num);
				}
				PLog.Log("end key " + text2);
				stringBuilder.Append(MacroKey(text2, macros));
			}
			else if (IsBreakCharacter(c))
			{
				if (num != -1)
				{
					string key = text.Substring(num, i - num);
					stringBuilder.Append(MacroKey(key, macros));
					num = -1;
				}
				stringBuilder.Append(c);
			}
			else if (c == '@')
			{
				if (num != -1)
				{
					string key2 = text.Substring(num, i - num);
					stringBuilder.Append(MacroKey(key2, macros));
					num = -1;
				}
				num = i + 1;
			}
			else if (num == -1)
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public string MacroKey(string key, Dictionary<string, string> macros)
	{
		if (macros.TryGetValue(key, out var value))
		{
			return value;
		}
		PLog.LogWarning("missing macro for key : " + key);
		return key;
	}

	public string TranslateRecursive(string text)
	{
		text = Translate(text);
		text = Translate(text);
		return text;
	}

	public string Translate(string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int length = text.Length;
		int num = -1;
		for (int i = 0; i < length; i++)
		{
			char c = text[i];
			if (i == length - 1 && num != -1)
			{
				string key = text.Substring(num);
				if (IsBreakCharacter(c))
				{
					key = text.Substring(num, i - num);
				}
				stringBuilder.Append(TranslateKey(key));
			}
			else if (IsBreakCharacter(c))
			{
				if (num != -1)
				{
					string key2 = text.Substring(num, i - num);
					stringBuilder.Append(TranslateKey(key2));
					num = -1;
				}
				stringBuilder.Append(c);
			}
			else if (c == '$')
			{
				if (num != -1)
				{
					string key3 = text.Substring(num, i - num);
					stringBuilder.Append(TranslateKey(key3));
					num = -1;
				}
				num = i + 1;
			}
			else if (num == -1)
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public string TranslateKey(string key)
	{
		if (m_translations.TryGetValue(key, out var value))
		{
			return value;
		}
		return "[#FF0000][" + key + "]";
	}
}
