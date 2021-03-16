using System;
using System.IO;
using UnityEngine;

[AddComponentMenu("Scripts/Mission/MNPlayers")]
public class MNPlayers : MNode
{
	public enum PlayerUpdateType
	{
		Team,
		Flag,
		Color
	}

	[Serializable]
	public class PlayerUpdate
	{
		public int m_playerId;

		public PlayerUpdateType m_type;

		public int m_parameter;

		public Color m_color = new Color(1f, 1f, 0f);

		public void SaveState(BinaryWriter writer)
		{
			writer.Write(m_playerId);
			writer.Write((int)m_type);
			writer.Write(m_parameter);
			writer.Write(m_color.r);
			writer.Write(m_color.g);
			writer.Write(m_color.b);
			writer.Write(m_color.a);
		}

		public void LoadState(BinaryReader reader)
		{
			m_playerId = reader.ReadInt32();
			m_type = (PlayerUpdateType)reader.ReadInt32();
			m_parameter = reader.ReadInt32();
			Color color = default(Color);
			color.r = reader.ReadSingle();
			color.g = reader.ReadSingle();
			color.b = reader.ReadSingle();
			color.a = reader.ReadSingle();
			m_color = color;
		}
	}

	public PlayerUpdate[] m_commands = new PlayerUpdate[0];

	public bool m_atStartup;

	public override void Awake()
	{
		base.Awake();
	}

	private void FixedUpdate()
	{
		if (NetObj.m_simulating && m_atStartup)
		{
			DoAction();
			m_atStartup = false;
		}
	}

	public override void DoAction()
	{
		PlayerUpdate[] commands = m_commands;
		foreach (PlayerUpdate playerUpdate in commands)
		{
			if (playerUpdate.m_type == PlayerUpdateType.Flag)
			{
				TurnMan.instance.SetPlayerFlag(playerUpdate.m_playerId, playerUpdate.m_parameter);
			}
			if (playerUpdate.m_type == PlayerUpdateType.Team)
			{
				TurnMan.instance.SetPlayerTeam(playerUpdate.m_playerId, playerUpdate.m_parameter);
			}
			if (playerUpdate.m_type == PlayerUpdateType.Color)
			{
				TurnMan.instance.SetPlayerColors(playerUpdate.m_playerId, playerUpdate.m_color);
			}
		}
	}

	public override void SaveState(BinaryWriter writer)
	{
		base.SaveState(writer);
		writer.Write(m_atStartup);
		writer.Write(m_commands.Length);
		PlayerUpdate[] commands = m_commands;
		foreach (PlayerUpdate playerUpdate in commands)
		{
			playerUpdate.SaveState(writer);
		}
	}

	public override void LoadState(BinaryReader reader)
	{
		base.LoadState(reader);
		m_atStartup = reader.ReadBoolean();
		int num = reader.ReadInt32();
		m_commands = new PlayerUpdate[num];
		for (int i = 0; i < num; i++)
		{
			PlayerUpdate playerUpdate = new PlayerUpdate();
			m_commands[i] = playerUpdate;
			playerUpdate.LoadState(reader);
		}
	}
}
