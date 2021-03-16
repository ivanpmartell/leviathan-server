using System.Collections.Generic;
using System.Xml;
using UnityEngine;

internal class ObjectFactory
{
	private class FactoryInfo
	{
		public string m_path;

		public GameObject m_prefab;
	}

	private static ObjectFactory m_instance;

	private Dictionary<string, FactoryInfo> m_objects = new Dictionary<string, FactoryInfo>();

	public static ObjectFactory instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new ObjectFactory();
			}
			return m_instance;
		}
	}

	public ObjectFactory()
	{
		TextAsset textAsset = Resources.Load("objectfactorydb") as TextAsset;
		if (textAsset == null)
		{
			PLog.LogError("Failed to load objectfactorydb.xml");
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(textAsset.text.ToString());
		for (XmlNode xmlNode = xmlDocument.FirstChild.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
		{
			if (xmlNode.Name == "prefab")
			{
				string value = xmlNode.Attributes["name"].Value;
				string value2 = xmlNode.Attributes["path"].Value;
				FactoryInfo value3 = new FactoryInfo
				{
					m_path = value2,
					m_prefab = null
				};
				m_objects.Add(value, value3);
			}
		}
		Resources.UnloadAsset(textAsset);
	}

	public static void ResetInstance()
	{
		m_instance = null;
	}

	public static GameObject Clone(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		GameObject gameObject = Object.Instantiate(prefab, pos, rot) as GameObject;
		gameObject.name = prefab.name;
		return gameObject;
	}

	public static GameObject Clone(GameObject prefab)
	{
		GameObject gameObject = Object.Instantiate(prefab) as GameObject;
		gameObject.name = prefab.name;
		return gameObject;
	}

	private void LoadPrefab(FactoryInfo info)
	{
		Object @object = Resources.Load(info.m_path);
		info.m_prefab = @object as GameObject;
	}

	public GameObject Create(string name, Vector3 pos, Quaternion rot)
	{
		if (m_objects.TryGetValue(name, out var value))
		{
			if (value.m_prefab == null)
			{
				LoadPrefab(value);
			}
			GameObject gameObject = Object.Instantiate(value.m_prefab, pos, rot) as GameObject;
			gameObject.name = value.m_prefab.name;
			return gameObject;
		}
		PLog.LogWarning("ObjectFactory: Failed to find prefab " + name + ". Do you need to tools/Build Object Factory.");
		return null;
	}

	public GameObject Create(string name)
	{
		if (m_objects.TryGetValue(name, out var value))
		{
			if (value.m_prefab == null)
			{
				LoadPrefab(value);
			}
			GameObject gameObject = Object.Instantiate(value.m_prefab) as GameObject;
			gameObject.name = value.m_prefab.name;
			return gameObject;
		}
		PLog.LogWarning("ObjectFactory: Failed to find prefab " + name + ". Do you need to tools/Build Object Factory.");
		return null;
	}

	public GameObject GetPrefab(string name)
	{
		if (m_objects.TryGetValue(name, out var value))
		{
			if (value.m_prefab == null)
			{
				LoadPrefab(value);
			}
			return value.m_prefab;
		}
		PLog.LogWarning("ObjectFactory: Failed to find prefab " + name + ". Do you need to tools/Build Object Factory.");
		return null;
	}
}
