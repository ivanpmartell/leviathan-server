using System;
using System.Collections.Generic;
using PTech;

public class ChatClient
{
	public struct ChatMessage
	{
		public DateTime m_date;

		public string m_name;

		public string m_message;

		public ChatMessage(DateTime date, string name, string msg)
		{
			m_date = date;
			m_name = name;
			m_message = msg;
		}
	}

	private const int m_maxQueueSize = 40;

	public Action<ChannelID, ChatMessage> m_onNewMessage;

	private RPC m_rpc;

	private Queue<ChatMessage>[] m_channels = new Queue<ChatMessage>[3];

	public ChatClient(RPC rpc)
	{
		m_rpc = rpc;
		m_rpc.Register("ChatMsg", RPC_ChatMessage);
		for (int i = 0; i < 3; i++)
		{
			m_channels[i] = new Queue<ChatMessage>();
		}
	}

	public void Close()
	{
		m_rpc.Unregister("ChatMsg");
	}

	private void RPC_ChatMessage(RPC rpc, List<object> args)
	{
		ChannelID channelID = (ChannelID)(int)args[0];
		DateTime date = DateTime.FromBinary((long)args[1]);
		string name = (string)args[2];
		string msg = (string)args[3];
		ChatMessage chatMessage = new ChatMessage(date, name, msg);
		CacheMessage(channelID, chatMessage);
		if (m_onNewMessage != null)
		{
			m_onNewMessage(channelID, chatMessage);
		}
	}

	public void SendMessage(ChannelID channel, string message)
	{
		m_rpc.Invoke("ChatMessage", (int)channel, message);
	}

	private void CacheMessage(ChannelID channel, ChatMessage msg)
	{
		Queue<ChatMessage> queue = m_channels[(int)channel];
		queue.Enqueue(msg);
		while (queue.Count > 40)
		{
			queue.Dequeue();
		}
	}

	public List<ChatMessage> GetAllMessages(ChannelID channel)
	{
		List<ChatMessage> list = new List<ChatMessage>();
		Queue<ChatMessage> queue = m_channels[(int)channel];
		foreach (ChatMessage item in queue)
		{
			list.Add(item);
		}
		return list;
	}
}
