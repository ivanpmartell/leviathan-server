using System;
using System.IO;
using PTech;

internal class GamePost : IComparable<GamePost>
{
	public int m_gameID;

	public string m_gameName = string.Empty;

	public string m_campaign = string.Empty;

	public string m_level = string.Empty;

	public GameType m_gameType;

	public FleetSizeClass m_fleetSizeClass;

	public int m_maxPlayers;

	public int m_nrOfPlayers;

	public int m_connectedPlayers;

	public int m_turn;

	public bool m_needAttention;

	public DateTime m_createDate;

	public byte[] ToArray()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(m_gameID);
		binaryWriter.Write(m_gameName);
		binaryWriter.Write(m_campaign);
		binaryWriter.Write(m_level);
		binaryWriter.Write((int)m_gameType);
		binaryWriter.Write((int)m_fleetSizeClass);
		binaryWriter.Write(m_maxPlayers);
		binaryWriter.Write(m_nrOfPlayers);
		binaryWriter.Write(m_connectedPlayers);
		binaryWriter.Write(m_turn);
		binaryWriter.Write(m_needAttention);
		binaryWriter.Write(m_createDate.ToBinary());
		return memoryStream.ToArray();
	}

	public void FromArray(byte[] data)
	{
		MemoryStream input = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(input);
		m_gameID = binaryReader.ReadInt32();
		m_gameName = binaryReader.ReadString();
		m_campaign = binaryReader.ReadString();
		m_level = binaryReader.ReadString();
		m_gameType = (GameType)binaryReader.ReadInt32();
		m_fleetSizeClass = (FleetSizeClass)binaryReader.ReadInt32();
		m_maxPlayers = binaryReader.ReadInt32();
		m_nrOfPlayers = binaryReader.ReadInt32();
		m_connectedPlayers = binaryReader.ReadInt32();
		m_turn = binaryReader.ReadInt32();
		m_needAttention = binaryReader.ReadBoolean();
		m_createDate = DateTime.FromBinary(binaryReader.ReadInt64());
	}

	public int CompareTo(GamePost other)
	{
		if (m_needAttention == other.m_needAttention)
		{
			return other.m_createDate.CompareTo(m_createDate);
		}
		if (m_needAttention)
		{
			return -1;
		}
		return 1;
	}
}
