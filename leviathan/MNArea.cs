using System.Collections.Generic;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNArea")]
public class MNArea : MNTrigger
{
	public string m_group = string.Empty;

	public bool m_onlyPlayers = true;

	public bool m_allPlayers;

	private HashSet<int> m_units = new HashSet<int>();

	public override void Awake()
	{
		base.Awake();
	}

	protected void Destroy()
	{
	}

	protected void Update()
	{
	}

	protected void FixedUpdate()
	{
	}

	public override void Trigger()
	{
		base.Trigger();
	}

	public override void OnEvent(string eventName)
	{
		if (eventName == "on")
		{
			m_disabled = false;
			ResetTrigger();
		}
		else if (eventName == "off")
		{
			m_disabled = true;
		}
		else
		{
			EventWarning(eventName);
		}
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		BoxCollider component = GetComponent<BoxCollider>();
		if (component != null)
		{
			Color color2 = (Gizmos.color = new Color(0.5f, 0.9f, 1f, 0.15f));
			Gizmos.DrawCube(component.bounds.center, component.bounds.size);
		}
	}

	private void ResetTrigger()
	{
		foreach (int unit in m_units)
		{
			NetObj byID = NetObj.GetByID(unit);
			if (!byID)
			{
				continue;
			}
			Ship component = byID.GetComponent<Ship>();
			if ((bool)component && ShouldTrigger(component))
			{
				Trigger();
				if (m_disabled)
				{
					break;
				}
			}
		}
	}

	private bool ShouldTrigger(Ship ship)
	{
		if (NetObj.m_phase == TurnPhase.Testing)
		{
			return false;
		}
		if (m_onlyPlayers)
		{
			if (ship.GetOwner() > 3)
			{
				return false;
			}
			if (m_allPlayers)
			{
				if (NrOfPlayers() != TurnMan.instance.GetNrOfPlayers())
				{
					return false;
				}
				return true;
			}
			if (m_group.Length != 0)
			{
				if (m_group == ship.GetGroup())
				{
					return true;
				}
				return false;
			}
			return true;
		}
		if (m_group.Length == 0)
		{
			return true;
		}
		if (m_group == ship.GetGroup())
		{
			return true;
		}
		return false;
	}

	private void OnTriggerEnter(Collider other)
	{
		Ship component = other.transform.parent.gameObject.GetComponent<Ship>();
		if (!m_units.Contains(component.GetNetID()))
		{
			m_units.Add(component.GetNetID());
			if (ShouldTrigger(component))
			{
				Trigger();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Ship component = other.transform.parent.gameObject.GetComponent<Ship>();
		m_units.Remove(component.GetNetID());
	}

	private int NrOfPlayers()
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int unit in m_units)
		{
			GameObject gameObject = NetObj.GetByID(unit).gameObject;
			Ship component = gameObject.GetComponent<Ship>();
			if (TurnMan.instance.IsHuman(component.GetOwner()))
			{
				hashSet.Add(component.GetOwner());
			}
		}
		return hashSet.Count;
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_group);
		writer.Write(m_onlyPlayers);
		writer.Write(m_allPlayers);
		writer.Write(m_units.Count);
		foreach (int unit in m_units)
		{
			writer.Write(unit);
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_group = reader.ReadString();
		m_onlyPlayers = reader.ReadBoolean();
		m_allPlayers = reader.ReadBoolean();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			m_units.Add(reader.ReadInt32());
		}
	}
}
