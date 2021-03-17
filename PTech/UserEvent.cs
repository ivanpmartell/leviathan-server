using System.IO;

public class UserEvent
{
	public enum EventType
	{
		NewTurn,
		Achievement,
		FriendRequest,
		GameInvite,
		FriendRequestAccepted,
		NewAccount,
		ResetPassword,
		ServerMessage
	}

	private EventType m_type;

	private string m_gameName;

	private int m_gameID;

	private string m_friendName;

	private int m_achievementID;

	private int m_turn;

	public UserEvent(byte[] data)
	{
		FromArray(data);
	}

	public UserEvent()
	{
	}

	public UserEvent(EventType type, string gameName, int gameID, string friendName, int achievementID, int turn)
	{
		m_type = type;
		m_gameName = gameName;
		m_gameID = gameID;
		m_friendName = friendName;
		m_achievementID = achievementID;
		m_turn = turn;
	}

	public EventType GetEventType()
	{
		return m_type;
	}

	public string GetGameName()
	{
		return m_gameName;
	}

	public int GetGameID()
	{
		return m_gameID;
	}

	public string GetFriendName()
	{
		return m_friendName;
	}

	public int GetAchievementID()
	{
		return m_achievementID;
	}

	public int GetTurn()
	{
		return m_turn;
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write((int)m_type);
		writer.Write(m_gameName);
		writer.Write(m_gameID);
		writer.Write(m_friendName);
		writer.Write(m_achievementID);
		writer.Write(m_turn);
	}

	public void Load(BinaryReader reader)
	{
		m_type = (EventType)reader.ReadInt32();
		m_gameName = reader.ReadString();
		m_gameID = reader.ReadInt32();
		m_friendName = reader.ReadString();
		m_achievementID = reader.ReadInt32();
		m_turn = reader.ReadInt32();
	}

	public byte[] ToArray()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(memoryStream);
		Save(writer);
		return memoryStream.ToArray();
	}

	public void FromArray(byte[] data)
	{
		MemoryStream input = new MemoryStream(data);
		BinaryReader reader = new BinaryReader(input);
		Load(reader);
	}
}
