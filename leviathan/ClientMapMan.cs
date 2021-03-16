#define DEBUG
using System.Xml;
using UnityEngine;

internal class ClientMapMan : MapMan
{
	public ClientMapMan()
	{
		TextAsset textAsset = Resources.Load("shared_settings/levels") as TextAsset;
		DebugUtils.Assert(textAsset != null, "Missing levels.xml");
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(textAsset.text);
		AddLevels(xmlDocument);
	}
}
