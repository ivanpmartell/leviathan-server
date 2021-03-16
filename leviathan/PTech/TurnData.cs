using System.Collections.Generic;
using System.IO;

namespace PTech
{
	internal class TurnData
	{
		public byte[] m_startState;

		public byte[] m_startOrders;

		public int[] m_startSurrenders = new int[0];

		public byte[] m_endState;

		public byte[] m_endOrders;

		public List<byte[]> m_newOrders = new List<byte[]>();

		public List<int> m_newSurrenders = new List<int>();

		private int m_turn;

		private TurnType m_type;

		private int m_playbackFrames;

		private int m_frames;

		public TurnData(int turn, int players, int playbackFrames, int frames, TurnType type)
		{
			m_turn = turn;
			m_type = type;
			m_playbackFrames = playbackFrames;
			m_frames = frames;
			for (int i = 0; i < players; i++)
			{
				m_newOrders.Add(null);
			}
		}

		public TurnData()
		{
		}

		public bool AllCommited()
		{
			foreach (byte[] newOrder in m_newOrders)
			{
				if (newOrder == null)
				{
					return false;
				}
			}
			return true;
		}

		public bool Commited(int playerID)
		{
			return m_newOrders[playerID] != null;
		}

		public int GetTurn()
		{
			return m_turn;
		}

		public int GetPlaybackFrames()
		{
			return m_playbackFrames;
		}

		public int GetFrames()
		{
			return m_frames;
		}

		public TurnType GetTurnType()
		{
			return m_type;
		}

		public void SetSurrender(int player)
		{
			if (!m_newSurrenders.Contains(player))
			{
				m_newSurrenders.Add(player);
			}
		}

		public void Save(BinaryWriter stream)
		{
			SaveByteArray(m_startState, stream);
			SaveByteArray(m_startOrders, stream);
			SaveByteArray(m_endState, stream);
			SaveByteArray(m_endOrders, stream);
			stream.Write(m_newOrders.Count);
			foreach (byte[] newOrder in m_newOrders)
			{
				SaveByteArray(newOrder, stream);
			}
			stream.Write(m_startSurrenders.Length);
			int[] startSurrenders = m_startSurrenders;
			foreach (int num in startSurrenders)
			{
				stream.Write((byte)num);
			}
			stream.Write(m_newSurrenders.Count);
			foreach (int newSurrender in m_newSurrenders)
			{
				stream.Write((byte)newSurrender);
			}
			stream.Write(m_turn);
			stream.Write((int)m_type);
			stream.Write(m_playbackFrames);
			stream.Write(m_frames);
		}

		public void Load(BinaryReader stream)
		{
			m_startState = LoadByteArray(stream);
			m_startOrders = LoadByteArray(stream);
			m_endState = LoadByteArray(stream);
			m_endOrders = LoadByteArray(stream);
			int num = stream.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				m_newOrders.Add(LoadByteArray(stream));
			}
			int num2 = stream.ReadInt32();
			m_startSurrenders = new int[num2];
			for (int j = 0; j < num2; j++)
			{
				m_startSurrenders[j] = stream.ReadByte();
			}
			int num3 = stream.ReadInt32();
			for (int k = 0; k < num3; k++)
			{
				m_newSurrenders.Add(stream.ReadByte());
			}
			m_turn = stream.ReadInt32();
			m_type = (TurnType)stream.ReadInt32();
			m_playbackFrames = stream.ReadInt32();
			m_frames = stream.ReadInt32();
		}

		private void SaveByteArray(byte[] data, BinaryWriter stream)
		{
			if (data == null)
			{
				stream.Write(-1);
				return;
			}
			stream.Write(data.Length);
			stream.Write(data);
		}

		private byte[] LoadByteArray(BinaryReader stream)
		{
			int num = stream.ReadInt32();
			if (num == -1)
			{
				return null;
			}
			return stream.ReadBytes(num);
		}
	}
}
