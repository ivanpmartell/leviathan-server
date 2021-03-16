using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNode")]
public class MNode : NetObj
{
	public override void Awake()
	{
		base.Awake();
	}

	public virtual void DoAction()
	{
	}

	public virtual void OnEvent(string eventName)
	{
		EventWarning(eventName);
	}

	public void EventWarning(string eventName)
	{
		if (Application.isEditor)
		{
			string text = base.name + "(" + GetNetID() + ") of type " + GetType().ToString();
			string text2 = "Recived event '" + eventName + "' that it do not care about.";
			MessageLog.instance.ShowMessage(MessageLog.TextPosition.Bottom, text, text2, string.Empty, 2f);
			PLog.Log(text + " " + text2);
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(base.transform.rotation.x);
		writer.Write(base.transform.rotation.y);
		writer.Write(base.transform.rotation.z);
		writer.Write(base.transform.rotation.w);
		writer.Write(base.transform.localScale.x);
		writer.Write(base.transform.localScale.y);
		writer.Write(base.transform.localScale.z);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		Vector3 position = default(Vector3);
		position.x = reader.ReadSingle();
		position.y = reader.ReadSingle();
		position.z = reader.ReadSingle();
		base.transform.position = position;
		Quaternion rotation = default(Quaternion);
		rotation.x = reader.ReadSingle();
		rotation.y = reader.ReadSingle();
		rotation.z = reader.ReadSingle();
		rotation.w = reader.ReadSingle();
		base.transform.rotation = rotation;
		Vector3 localScale = default(Vector3);
		localScale.x = reader.ReadSingle();
		localScale.y = reader.ReadSingle();
		localScale.z = reader.ReadSingle();
		base.transform.localScale = localScale;
	}

	public static List<GameObject> GetTargets(GameObject target)
	{
		List<GameObject> list = new List<GameObject>();
		MNRepeater component = target.GetComponent<MNRepeater>();
		if ((bool)component)
		{
			for (int i = 0; i < component.m_repeatTargets.Length; i++)
			{
				GameObject targetObj = component.GetTargetObj(i);
				if (targetObj != null)
				{
					list.Add(targetObj);
				}
			}
			return list;
		}
		list.Add(target);
		return list;
	}
}
