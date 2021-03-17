using System.IO;

namespace PTech
{
	internal class GamePlayer
	{
		public string m_userName;

		public bool m_inGame;

		public int m_id;

		public int m_team;

		public bool m_leftGame;

		public bool m_seenEndGame;

		public bool m_dead;

		public bool m_surrender;

		public bool m_admin;

		public bool m_readyToStart;

		public string m_selectedFleetName = string.Empty;

		public FleetDef m_fleet;

		public int m_score = -1;

		public int m_teamScore = -1;

		public int m_place = -1;

		public int[] m_flagshipKiller = new int[2] { -1, -1 };

		private User m_user;

		public GamePlayer(int id)
		{
			m_id = id;
		}

		public void SetUser(User u)
		{
			m_user = u;
			m_userName = u.m_name;
			m_user.m_inGames++;
		}

		public User GetUser()
		{
			return m_user;
		}

		public PlayerPresenceStatus GetPlayerPresenceStatus()
		{
			if (m_user == null)
			{
				return PlayerPresenceStatus.Offline;
			}
			if (m_inGame)
			{
				return PlayerPresenceStatus.InGame;
			}
			if (m_user.m_rpc != null)
			{
				return PlayerPresenceStatus.Online;
			}
			return PlayerPresenceStatus.Offline;
		}

		public void Save(BinaryWriter stream)
		{
			stream.Write(m_userName);
			stream.Write(m_team);
			stream.Write(m_leftGame);
			stream.Write(m_seenEndGame);
			stream.Write(m_dead);
			stream.Write(m_surrender);
			stream.Write(m_admin);
			stream.Write(m_readyToStart);
			stream.Write(m_selectedFleetName);
			stream.Write(m_score);
			stream.Write(m_teamScore);
			stream.Write(m_place);
			stream.Write(m_flagshipKiller[0]);
			stream.Write(m_flagshipKiller[1]);
			if (m_fleet != null)
			{
				byte[] array = m_fleet.ToArray();
				stream.Write(array.Length);
				stream.Write(array);
			}
			else
			{
				stream.Write(-1);
			}
		}

		public void Load(BinaryReader stream)
		{
			m_userName = stream.ReadString();
			m_team = stream.ReadInt32();
			m_leftGame = stream.ReadBoolean();
			m_seenEndGame = stream.ReadBoolean();
			m_dead = stream.ReadBoolean();
			m_surrender = stream.ReadBoolean();
			m_admin = stream.ReadBoolean();
			m_readyToStart = stream.ReadBoolean();
			m_selectedFleetName = stream.ReadString();
			m_score = stream.ReadInt32();
			m_teamScore = stream.ReadInt32();
			m_place = stream.ReadInt32();
			m_flagshipKiller[0] = stream.ReadInt32();
			m_flagshipKiller[1] = stream.ReadInt32();
			int num = stream.ReadInt32();
			if (num != -1)
			{
				byte[] data = stream.ReadBytes(num);
				m_fleet = new FleetDef(data);
			}
			else
			{
				m_fleet = null;
			}
		}
	}
}
