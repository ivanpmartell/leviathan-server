using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace PTech
{
	public class PacketSocket
	{
		public enum RecvStatus
		{
			PackageReady,
			ReceivedData,
			NoData
		}

		private Socket m_socket;

		private bool m_error;

		private int m_packetSize;

		private int m_gotPacketBytes;

		private byte[] m_packetBuffer;

		private int m_gotSizeBytes;

		private byte[] m_idBuffer = new byte[4];

		private Thread m_thread;

		private Mutex m_recvQueueMutex = new Mutex();

		private Mutex m_sendQueueMutex = new Mutex();

		private Mutex m_closeMutex = new Mutex();

		private Queue<byte[]> m_recvQueue = new Queue<byte[]>();

		private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

		private bool m_abortThread;

		private bool m_received;

		public PacketSocket(Socket socket)
		{
			m_socket = socket;
			m_socket.Blocking = false;
			m_thread = new Thread(RecvThread);
			m_thread.Start();
		}

		public bool Send(byte[] packet)
		{
			if (m_error)
			{
				return false;
			}
			m_sendQueueMutex.WaitOne();
			m_sendQueue.Enqueue(packet);
			m_sendQueueMutex.ReleaseMutex();
			return true;
		}

		public RecvStatus Recv(out byte[] packet)
		{
			packet = GetRecvQueuePackage();
			if (packet != null)
			{
				m_received = false;
				return RecvStatus.PackageReady;
			}
			if (m_received)
			{
				m_received = false;
				return RecvStatus.ReceivedData;
			}
			m_received = false;
			return RecvStatus.NoData;
		}

		private void RecvThread()
		{
			//Discarded unreachable code: IL_0038
			while (!m_abortThread && !m_error)
			{
				try
				{
					ReceiveData();
					SendData();
				}
				catch (Exception ex)
				{
					m_error = true;
					Console.WriteLine("Socket error:" + ex.ToString());
					return;
				}
				Thread.Sleep(10);
			}
		}

		private void ReceiveData()
		{
			if (m_gotSizeBytes < 4)
			{
				SocketError error;
				int num = m_socket.Receive(m_idBuffer, m_gotSizeBytes, 4 - m_gotSizeBytes, SocketFlags.None, out error);
				m_gotSizeBytes += num;
				if (num > 0)
				{
					m_received = true;
				}
				if (error != 0 && error != SocketError.WouldBlock)
				{
					m_error = true;
				}
				if (m_gotSizeBytes == 4)
				{
					m_packetSize = BitConverter.ToInt32(m_idBuffer, 0);
					m_gotPacketBytes = 0;
					m_packetBuffer = new byte[m_packetSize];
				}
				return;
			}
			SocketError error2;
			int num2 = m_socket.Receive(m_packetBuffer, m_gotPacketBytes, m_packetSize - m_gotPacketBytes, SocketFlags.None, out error2);
			m_gotPacketBytes += num2;
			if (num2 > 0)
			{
				m_received = true;
			}
			if (error2 != 0 && error2 != SocketError.WouldBlock)
			{
				m_error = true;
			}
			if (m_gotPacketBytes == m_packetSize)
			{
				m_recvQueueMutex.WaitOne();
				m_recvQueue.Enqueue(m_packetBuffer);
				m_recvQueueMutex.ReleaseMutex();
				m_packetBuffer = null;
				m_packetSize = 0;
				m_gotSizeBytes = 0;
			}
		}

		private void SendData()
		{
			if (!m_error)
			{
				byte[] sendQueuePackage = GetSendQueuePackage();
				while (sendQueuePackage != null && SendPackage(sendQueuePackage))
				{
					sendQueuePackage = GetSendQueuePackage();
				}
			}
		}

		private byte[] GetSendQueuePackage()
		{
			byte[] result = null;
			m_sendQueueMutex.WaitOne();
			if (m_sendQueue.Count > 0)
			{
				result = m_sendQueue.Dequeue();
			}
			m_sendQueueMutex.ReleaseMutex();
			return result;
		}

		private byte[] GetRecvQueuePackage()
		{
			byte[] result = null;
			m_recvQueueMutex.WaitOne();
			if (m_recvQueue.Count > 0)
			{
				result = m_recvQueue.Dequeue();
			}
			m_recvQueueMutex.ReleaseMutex();
			return result;
		}

		public bool SendPackage(byte[] packet)
		{
			//Discarded unreachable code: IL_0044, IL_0058
			int value = packet.Length;
			byte[] bytes = BitConverter.GetBytes(value);
			try
			{
				m_socket.Blocking = true;
				m_socket.Send(bytes);
				m_socket.Send(packet);
				m_socket.Blocking = false;
				return true;
			}
			catch (SocketException)
			{
				m_error = true;
				return false;
			}
		}

		public bool GotError()
		{
			return m_error;
		}

		public void Close()
		{
			m_closeMutex.WaitOne();
			if (m_socket != null)
			{
				m_socket.Close();
				m_thread.Abort();
				m_socket = null;
			}
			m_closeMutex.ReleaseMutex();
		}

		public bool IsOpen()
		{
			return m_socket != null;
		}
	}
}
