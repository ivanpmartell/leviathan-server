#define DEBUG
using System.Collections.Generic;
using System.IO;
using PTech;
using UnityEngine;

public class TurnMan
{
	public class PlayerTurnData
	{
		public string m_name = string.Empty;

		public int m_score;

		public int m_team = -1;

		public bool m_isHuman;

		public int m_flag = 4;

		public Color m_primaryColor = Color.red;

		public int[] m_flagshipKilledBy = new int[2] { -1, -1 };

		public int m_totalShipsSunk;

		public int m_totalShipsLost;

		public int m_totalDamageInflicted;

		public int m_totalDamageAbsorbed;

		public int m_totalTimeTraveled;

		public int m_turnDamage;

		public int m_turnFriendlyDamage;

		public int m_turnShipsSunk;

		public Dictionary<string, int> m_turnGunDamage = new Dictionary<string, int>();
	}

	public class MissionObjective
	{
		public MNAction.ObjectiveStatus m_status;

		public string m_text;

		public void SaveState(BinaryWriter writer)
		{
			writer.Write((int)m_status);
			writer.Write(m_text);
		}

		public void LoadState(BinaryReader reader)
		{
			m_status = (MNAction.ObjectiveStatus)reader.ReadInt32();
			m_text = reader.ReadString();
		}
	}

	private static TurnMan m_instance;

	public GameOutcome m_endGame;

	public MNAction.MNActionElement[] m_dialog;

	public List<MissionObjective> m_missionObjectives = new List<MissionObjective>();

	private List<PlayerTurnData> m_players = new List<PlayerTurnData>();

	private List<int> m_teamScore = new List<int>();

	private int m_nrOfPlayers;

	private GameType m_gameType;

	private string m_music = string.Empty;

	public int m_missionAchievement = -1;

	public static TurnMan instance => m_instance;

	public TurnMan()
	{
		m_instance = this;
	}

	public void Close()
	{
		m_instance = null;
	}

	public bool IsHostile(int playerID, int otherPlayerID)
	{
		if (playerID == otherPlayerID)
		{
			return false;
		}
		int team = GetPlayer(playerID).m_team;
		int team2 = GetPlayer(otherPlayerID).m_team;
		if (team == 4 || team2 == 4)
		{
			return false;
		}
		return team == -1 || team2 == -1 || team != team2;
	}

	public void SetGameType(GameType gameType)
	{
		m_gameType = gameType;
	}

	public GameType GetGameType()
	{
		return m_gameType;
	}

	public void SetNrOfPlayers(int nr)
	{
		m_nrOfPlayers = nr;
	}

	public int GetNrOfPlayers()
	{
		return m_nrOfPlayers;
	}

	public void SetPlayerHuman(int playerID, bool ishuman)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_isHuman = ishuman;
	}

	public void SetPlayerName(int playerID, string name)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_name = name;
	}

	public void SetPlayerFlag(int playerID, int flag)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_flag = flag;
	}

	public void SetPlayerColors(int playerID, Color color)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_primaryColor = color;
	}

	public bool IsHuman(int playerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		return player.m_isHuman;
	}

	public void SetPlayerTeam(int playerID, int team)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_team = team;
	}

	public int GetPlayerTeam(int playerID)
	{
		if (playerID < 0)
		{
			return -1;
		}
		return GetPlayer(playerID).m_team;
	}

	public string GetPlayerName(int playerID)
	{
		if (playerID < 0)
		{
			return string.Empty;
		}
		return GetPlayer(playerID).m_name;
	}

	public int GetPlayerFlag(int playerID)
	{
		if (playerID < 0)
		{
			return -1;
		}
		return GetPlayer(playerID).m_flag;
	}

	public void GetPlayerColors(int playerID, out Color primaryColor)
	{
		if (playerID < 0 || playerID >= m_players.Count)
		{
			primaryColor = Color.white;
			return;
		}
		PlayerTurnData player = GetPlayer(playerID);
		primaryColor = player.m_primaryColor;
	}

	public void ResetTurnStats()
	{
		foreach (PlayerTurnData player in m_players)
		{
			player.m_turnDamage = 0;
			player.m_turnFriendlyDamage = 0;
			player.m_turnShipsSunk = 0;
			player.m_turnGunDamage.Clear();
		}
	}

	public void AddFlagshipKiller(int playerID, int flagshipKillerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		if (player.m_flagshipKilledBy[0] != -1)
		{
			player.m_flagshipKilledBy[1] = flagshipKillerID;
		}
		else
		{
			player.m_flagshipKilledBy[0] = flagshipKillerID;
		}
	}

	public int GetFlagshipKiller(int playerID, int flagShipNr)
	{
		DebugUtils.Assert(flagShipNr == 0 || flagShipNr == 1);
		PlayerTurnData player = GetPlayer(playerID);
		return player.m_flagshipKilledBy[flagShipNr];
	}

	public void AddPlayerScore(int playerID, int score)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_score += score;
	}

	public void AddShieldAbsorb(int playerID, int damage)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_totalDamageAbsorbed += damage;
	}

	public void AddPlayerDamage(int playerID, int damage, bool friendly, string gunName)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_turnDamage += damage;
		player.m_totalDamageInflicted += damage;
		if (friendly)
		{
			player.m_turnFriendlyDamage += damage;
		}
		if (gunName.Length > 0)
		{
			if (player.m_turnGunDamage.TryGetValue(gunName, out var value))
			{
				player.m_turnGunDamage[gunName] = value + damage;
			}
			else
			{
				player.m_turnGunDamage.Add(gunName, damage);
			}
		}
	}

	public int GetTotalShipsSunk(int playerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		return player.m_totalShipsSunk;
	}

	public int GetTotalShipsLost(int playerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		return player.m_totalShipsLost;
	}

	public int GetPlayerTurnDamage(int playerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		return player.m_turnDamage;
	}

	public void AddShipsSunk(int playerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_turnShipsSunk++;
		player.m_totalShipsSunk++;
	}

	public void AddShipsLost(int playerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		player.m_totalShipsLost++;
	}

	public void AddTeamScore(int team, int score)
	{
		while (m_teamScore.Count < team + 1)
		{
			m_teamScore.Add(0);
		}
		List<int> teamScore;
		List<int> list = (teamScore = m_teamScore);
		int index;
		int index2 = (index = team);
		index = teamScore[index];
		list[index2] = index + score;
	}

	public int GetPlayerScore(int playerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		return player.m_score;
	}

	public int GetTeamSize(int team)
	{
		int num = 0;
		foreach (PlayerTurnData player in m_players)
		{
			if (player.m_team == team)
			{
				num++;
			}
		}
		return num;
	}

	public int GetTeamScoreForPlayer(int playerID)
	{
		PlayerTurnData player = GetPlayer(playerID);
		if (player.m_team >= 0)
		{
			return GetTeamScore(player.m_team);
		}
		return 0;
	}

	public int GetTeamScore(int team)
	{
		if (m_teamScore.Count <= team)
		{
			return 0;
		}
		return m_teamScore[team];
	}

	public int[] GetPlayerScoreList()
	{
		int[] array = new int[m_players.Count];
		for (int i = 0; i < m_players.Count; i++)
		{
			array[i] = m_players[i].m_score;
		}
		return array;
	}

	public PlayerTurnData GetAccoladeAbsorbed()
	{
		PlayerTurnData playerTurnData = m_players[0];
		for (int i = 0; i < m_players.Count; i++)
		{
			if (m_players[i].m_totalDamageAbsorbed > playerTurnData.m_totalDamageAbsorbed)
			{
				playerTurnData = m_players[i];
			}
		}
		return playerTurnData;
	}

	public PlayerTurnData GetAccoladeDestory(bool highest)
	{
		PlayerTurnData playerTurnData = m_players[0];
		int totalDamageInflicted = m_players[0].m_totalDamageInflicted;
		for (int i = 0; i < m_players.Count; i++)
		{
			if (!m_players[i].m_isHuman)
			{
				continue;
			}
			if (highest)
			{
				if (m_players[i].m_totalDamageInflicted > playerTurnData.m_totalDamageInflicted)
				{
					playerTurnData = m_players[i];
				}
			}
			else if (m_players[i].m_totalDamageInflicted < playerTurnData.m_totalDamageInflicted)
			{
				playerTurnData = m_players[i];
			}
		}
		return playerTurnData;
	}

	public int[] GetTeamScoreList()
	{
		return m_teamScore.ToArray();
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(m_nrOfPlayers);
		writer.Write((byte)m_gameType);
		writer.Write(m_players.Count);
		foreach (PlayerTurnData player in m_players)
		{
			writer.Write(player.m_name);
			writer.Write(player.m_score);
			writer.Write((byte)player.m_team);
			writer.Write(player.m_isHuman);
			writer.Write((short)player.m_flag);
			writer.Write((short)player.m_flagshipKilledBy[0]);
			writer.Write((short)player.m_flagshipKilledBy[1]);
			writer.Write((short)player.m_totalShipsSunk);
			writer.Write((short)player.m_totalShipsLost);
			writer.Write((short)player.m_totalDamageInflicted);
			writer.Write((short)player.m_totalDamageAbsorbed);
			writer.Write((short)player.m_totalTimeTraveled);
			writer.Write(player.m_primaryColor.r);
			writer.Write(player.m_primaryColor.g);
			writer.Write(player.m_primaryColor.b);
			writer.Write(player.m_primaryColor.a);
		}
		writer.Write(m_teamScore.Count);
		foreach (int item in m_teamScore)
		{
			writer.Write(item);
		}
		writer.Write((int)m_endGame);
		writer.Write(m_music);
		writer.Write(m_missionObjectives.Count);
		foreach (MissionObjective missionObjective in m_missionObjectives)
		{
			missionObjective.SaveState(writer);
		}
		writer.Write(m_missionAchievement);
	}

	public void Load(BinaryReader reader)
	{
		m_nrOfPlayers = reader.ReadInt32();
		m_gameType = (GameType)reader.ReadByte();
		m_players.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PlayerTurnData playerTurnData = new PlayerTurnData();
			playerTurnData.m_name = reader.ReadString();
			playerTurnData.m_score = reader.ReadInt32();
			playerTurnData.m_team = reader.ReadByte();
			playerTurnData.m_isHuman = reader.ReadBoolean();
			playerTurnData.m_flag = reader.ReadInt16();
			playerTurnData.m_flagshipKilledBy[0] = reader.ReadInt16();
			playerTurnData.m_flagshipKilledBy[1] = reader.ReadInt16();
			playerTurnData.m_totalShipsSunk = reader.ReadInt16();
			playerTurnData.m_totalShipsLost = reader.ReadInt16();
			playerTurnData.m_totalDamageInflicted = reader.ReadInt16();
			playerTurnData.m_totalDamageAbsorbed = reader.ReadInt16();
			playerTurnData.m_totalTimeTraveled = reader.ReadInt16();
			playerTurnData.m_primaryColor.r = reader.ReadSingle();
			playerTurnData.m_primaryColor.g = reader.ReadSingle();
			playerTurnData.m_primaryColor.b = reader.ReadSingle();
			playerTurnData.m_primaryColor.a = reader.ReadSingle();
			m_players.Add(playerTurnData);
		}
		m_teamScore.Clear();
		int num2 = reader.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			m_teamScore.Add(reader.ReadInt32());
		}
		m_endGame = (GameOutcome)reader.ReadInt32();
		m_music = reader.ReadString();
		int num3 = reader.ReadInt32();
		m_missionObjectives = new List<MissionObjective>();
		for (int k = 0; k < num3; k++)
		{
			MissionObjective missionObjective = new MissionObjective();
			missionObjective.LoadState(reader);
			m_missionObjectives.Add(missionObjective);
		}
		m_missionAchievement = reader.ReadInt32();
	}

	public PlayerTurnData GetPlayer(int id)
	{
		while (m_players.Count < id + 1)
		{
			PlayerTurnData playerTurnData = new PlayerTurnData();
			playerTurnData.m_team = m_players.Count;
			m_players.Add(playerTurnData);
		}
		return m_players[id];
	}

	public void SetMissionObjective(string name, MNAction.ObjectiveStatus status)
	{
		foreach (MissionObjective missionObjective2 in m_missionObjectives)
		{
			if (missionObjective2.m_text == name)
			{
				missionObjective2.m_status = status;
				if (status == MNAction.ObjectiveStatus.Done)
				{
					MessageLog.instance.ShowMessage(MessageLog.TextPosition.Top, "$" + name, "$message_objectivedone", "ObjectiveDoneMessage", 3f);
				}
				return;
			}
		}
		MissionObjective missionObjective = new MissionObjective();
		missionObjective.m_text = name;
		missionObjective.m_status = status;
		m_missionObjectives.Add(missionObjective);
		MessageLog.instance.ShowMessage(MessageLog.TextPosition.Top, "$message_newobjective", "$" + name, string.Empty, 4f);
	}

	public GameOutcome GetOutcome()
	{
		GameOutcome endGameStatus = CheatMan.instance.GetEndGameStatus();
		if (endGameStatus != 0)
		{
			return endGameStatus;
		}
		return m_endGame;
	}

	public void SetTurnMusic(string music)
	{
		if (!(music == m_music))
		{
			m_music = music;
		}
	}

	public string GetTurnMusic()
	{
		return m_music;
	}

	public void PlayBriefing(string name)
	{
		MNAction.MNActionElement[] array = new MNAction.MNActionElement[1]
		{
			new MNAction.MNActionElement()
		};
		array[0].m_type = MNAction.ActionType.ShowDebriefing;
		array[0].m_parameter = name;
		m_dialog = array;
	}
}
