using System.Collections.Generic;
using System.IO;

namespace PTech
{
	internal class ChatServer
	{
		private ChatChannel[] m_channels = new ChatChannel[3];

		public ChatServer()
		{
			m_channels[0] = new ChatChannel(ChannelID.General);
			m_channels[1] = new ChatChannel(ChannelID.Team0);
			m_channels[2] = new ChatChannel(ChannelID.Team1);
		}

		public void Register(GamePlayer player, bool sendOldMessages)
		{
			User user = player.GetUser();
			m_channels[0].AddUser(user, sendOldMessages);
			if (player.m_team == 0)
			{
				m_channels[1].AddUser(user, sendOldMessages);
			}
			else if (player.m_team == 1)
			{
				m_channels[2].AddUser(user, sendOldMessages);
			}
			user.m_rpc.Register("ChatMessage", RPC_ChatMessage);
		}

		public void Unregister(GamePlayer player)
		{
			User user = player.GetUser();
			ChatChannel[] channels = m_channels;
			foreach (ChatChannel chatChannel in channels)
			{
				chatChannel.RemoveUser(user);
			}
			if (user.m_rpc != null)
			{
				user.m_rpc.Unregister("ChatMessage");
			}
		}

		private void RPC_ChatMessage(RPC rpc, List<object> args)
		{
			int num = (int)args[0];
			string message = (string)args[1];
			if (num >= 0 && num < m_channels.Length)
			{
				m_channels[num].AddMessage(rpc, message);
			}
		}

		public void Save(BinaryWriter stream)
		{
			ChatChannel[] channels = m_channels;
			foreach (ChatChannel chatChannel in channels)
			{
				chatChannel.Save(stream);
			}
		}

		public void Load(BinaryReader stream)
		{
			ChatChannel[] channels = m_channels;
			foreach (ChatChannel chatChannel in channels)
			{
				chatChannel.Load(stream);
			}
		}
	}
}
