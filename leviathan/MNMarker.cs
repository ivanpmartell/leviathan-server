using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNMarker")]
public class MNMarker : MNode
{
	public GameObject m_marker;

	public bool m_show;

	public Unit.ObjectiveTypes m_objectiveType = Unit.ObjectiveTypes.Move;

	private int m_markerNetID;

	public override void Awake()
	{
		base.Awake();
	}

	private void FixedUpdate()
	{
	}

	public override void DoAction()
	{
		GameObject marker = GetMarker();
		if (marker == null)
		{
			return;
		}
		List<GameObject> targets = MNode.GetTargets(marker);
		for (int i = 0; i < targets.Count; i++)
		{
			GameObject gameObject = targets[i];
			Marker component = gameObject.GetComponent<Marker>();
			if (component != null)
			{
				component.SetVisible(m_show);
			}
			Unit component2 = gameObject.GetComponent<Unit>();
			if (component2 != null)
			{
				component2.SetObjective(m_objectiveType);
			}
			MNSpawn component3 = gameObject.GetComponent<MNSpawn>();
			if (component3 != null)
			{
				component3.SetObjective(m_objectiveType);
			}
		}
	}

	public void OnDrawGizmos()
	{
		if (GetMarker() != null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(GetComponent<Transform>().position, GetMarker().GetComponent<Transform>().position);
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		if (GetMarker() == null)
		{
			writer.Write(0);
		}
		else
		{
			writer.Write(GetMarker().GetComponent<NetObj>().GetNetID());
		}
		writer.Write(m_show);
		writer.Write((int)m_objectiveType);
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_markerNetID = reader.ReadInt32();
		m_show = reader.ReadBoolean();
		m_objectiveType = (Unit.ObjectiveTypes)reader.ReadInt32();
	}

	public GameObject GetMarker()
	{
		if (m_marker != null)
		{
			return m_marker;
		}
		if (m_markerNetID == 0)
		{
			return null;
		}
		m_marker = NetObj.GetByID(m_markerNetID).gameObject;
		return m_marker;
	}
}
