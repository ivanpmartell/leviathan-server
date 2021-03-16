using System;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNAction")]
public class MNAction : MNode
{
	public enum ActionType
	{
		PlayScene = 0,
		UpdateObjective = 1,
		MissionVictory = 2,
		MissionDefeat = 3,
		MissionGameOver = 3,
		Marker = 4,
		Message = 5,
		PlayerChange = 6,
		Event = 7,
		ShowBriefing = 8,
		ShowTutorial = 9,
		ShowDebriefing = 10,
		MissionAchievement = 11
	}

	public enum ObjectiveStatus
	{
		Hide,
		Visible,
		Active,
		Done
	}

	[Serializable]
	public class MNActionElement
	{
		public ActionType m_type;

		public string m_parameter = string.Empty;

		public ObjectiveStatus m_objectiveStatus;

		public GameObject m_target;

		public Unit.ObjectiveTypes m_objectiveType = Unit.ObjectiveTypes.Move;

		private int m_targetNetID;

		public void SaveState(BinaryWriter writer)
		{
			writer.Write((int)m_type);
			writer.Write(m_parameter);
			writer.Write((int)m_objectiveStatus);
			if (GetTarget() == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(GetTarget().GetComponent<NetObj>().GetNetID());
			}
			writer.Write((int)m_objectiveType);
		}

		public void LoadState(BinaryReader reader)
		{
			m_type = (ActionType)reader.ReadInt32();
			m_parameter = reader.ReadString();
			m_objectiveStatus = (ObjectiveStatus)reader.ReadInt32();
			m_targetNetID = reader.ReadInt32();
			m_objectiveType = (Unit.ObjectiveTypes)reader.ReadInt32();
		}

		public GameObject GetTarget()
		{
			if (m_target != null)
			{
				return m_target;
			}
			if (m_targetNetID == 0)
			{
				return null;
			}
			m_target = NetObj.GetByID(m_targetNetID).gameObject;
			return m_target;
		}
	}

	public MNActionElement[] m_commands = new MNActionElement[0];

	public override void Awake()
	{
		base.Awake();
	}

	private void FixedUpdate()
	{
	}

	public virtual void OnDrawGizmosSelected()
	{
		MNActionElement[] commands = m_commands;
		foreach (MNActionElement mNActionElement in commands)
		{
			GameObject target = mNActionElement.GetTarget();
			if (target != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(GetComponent<Transform>().position, target.GetComponent<Transform>().position);
			}
		}
	}

	private void AddCommands(MNActionElement[] commands)
	{
		if (TurnMan.instance.m_dialog == null)
		{
			TurnMan.instance.m_dialog = m_commands;
			return;
		}
		MNActionElement[] array = new MNActionElement[TurnMan.instance.m_dialog.Length + commands.Length];
		TurnMan.instance.m_dialog.CopyTo(array, 0);
		m_commands.CopyTo(array, TurnMan.instance.m_dialog.Length);
		TurnMan.instance.m_dialog = TurnMan.instance.m_dialog;
	}

	public override void DoAction()
	{
		Camera component = GameObject.Find("GameCamera").GetComponent<Camera>();
		if (component != null && component.enabled && NetObj.IsSimulating())
		{
			AddCommands(m_commands);
			return;
		}
		Dialog dialog = new Dialog(null, null, null, null);
		dialog.SetCommands(m_commands);
		dialog.PlayAll();
		dialog = null;
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_commands.Length);
		MNActionElement[] commands = m_commands;
		foreach (MNActionElement mNActionElement in commands)
		{
			mNActionElement.SaveState(writer);
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		int num = reader.ReadInt32();
		m_commands = new MNActionElement[num];
		for (int i = 0; i < num; i++)
		{
			MNActionElement mNActionElement = new MNActionElement();
			mNActionElement.LoadState(reader);
			m_commands[i] = mNActionElement;
		}
	}
}
