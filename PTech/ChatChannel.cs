using System;
using System.Collections.Generic;
using System.IO;

namespace PTech
{
	internal class ChatChannel
	{
		private struct ChatMessage
		{
			public long m_date;

			public string m_name;

			public string m_message;

			public ChatMessage(long date, string name, string msg)
			{
				m_date = date;
				m_name = name;
				m_message = msg;
			}
		}

		private const int m_maxMessages = 40;

		private ChannelID m_channelID;

		private Queue<ChatMessage> m_messages = new Queue<ChatMessage>();

		private List<User> m_users = new List<User>();

		public ChatChannel(ChannelID id)
		{
			m_channelID = id;
		}

		public void AddUser(User user, bool sendOldMessages)
		{
			if (m_users.Contains(user))
			{
				return;
			}
			m_users.Add(user);
			if (!sendOldMessages)
			{
				return;
			}
			foreach (ChatMessage message in m_messages)
			{
				user.m_rpc.Invoke("ChatMsg", (int)m_channelID, message.m_date, message.m_name, message.m_message);
			}
		}

		public void RemoveUser(User user)
		{
			m_users.Remove(user);
		}

		public void AddMessage(RPC rpc, string message)
		{
			User userByRPC = GetUserByRPC(rpc);
			if (userByRPC != null)
			{
				AddMessage(userByRPC.m_name, message);
			}
		}

		public void AddMessage(string name, string message)
		{
			long num = DateTime.Now.ToBinary();
			foreach (User user in m_users)
			{
				user.m_rpc.Invoke("ChatMsg", (int)m_channelID, num, name, message);
			}
			m_messages.Enqueue(new ChatMessage(num, name, message));
			while (m_messages.Count > 40)
			{
				m_messages.Dequeue();
			}
		}

		public void Save(BinaryWriter stream)
		{
			stream.Write(m_messages.Count);
			foreach (ChatMessage message in m_messages)
			{
				stream.Write(message.m_date);
				stream.Write(message.m_name);
				stream.Write(message.m_message);
			}
		}

		public void Load(BinaryReader stream)
		{
			m_messages.Clear();
			int num = stream.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				ChatMessage item = default(ChatMessage);
				item.m_date = stream.ReadInt64();
				item.m_name = stream.ReadString();
				item.m_message = stream.ReadString();
				m_messages.Enqueue(item);
			}
		}

		private User GetUserByRPC(RPC rpc)
		{
			foreach (User user in m_users)
			{
				if (user.m_rpc == rpc)
				{
					return user;
				}
			}
			return null;
		}
	}
}
