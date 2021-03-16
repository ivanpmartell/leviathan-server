using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace PTech
{
	public class RPC
	{
		private enum Type
		{
			Int,
			Bool,
			Float,
			Double,
			String,
			ByteArray,
			FloatArray,
			DoubleArray,
			StringIntDic,
			Long,
			StringArray,
			IntArray
		}

		private enum PackageType
		{
			Invocation = 1,
			Ping
		}

		public delegate void Handler(RPC rpc, List<object> arg);

		private float m_pingTimeout = 16f;

		private float m_pingInterval = 1f;

		private float m_timeSinceLastRecvPackage;

		private float m_mainThreadTimeout = -1f;

		private float m_timeSinceMainThreadUpdate;

		private Thread m_pingThread;

		private bool m_abortPingThread;

		private Stopwatch m_stopWatch = new Stopwatch();

		private PacketSocket m_socket;

		private Dictionary<string, Guid> m_nameToID = new Dictionary<string, Guid>();

		private Dictionary<Guid, Handler> m_methods = new Dictionary<Guid, Handler>();

		private int m_totalSentData;

		private int m_totalRecvData;

		private int m_sentData;

		private int m_recvData;

		private int m_sentDataPerSec;

		private int m_recvDataPerSec;

		private float m_updateStatsTimer;

		public RPC(PacketSocket socket)
		{
			m_socket = socket;
			m_stopWatch.Start();
			m_pingThread = new Thread(PingThread);
			m_pingThread.Start();
		}

		public void SetMainThreadTimeout(float timeInSec)
		{
			m_mainThreadTimeout = timeInSec;
		}

		public bool Update(bool recvAll)
		{
			if (m_socket.GotError())
			{
				return false;
			}
			if (!m_socket.IsOpen())
			{
				return false;
			}
			float deltaTime = GetDeltaTime();
			if (!UpdatePingTimeout(deltaTime))
			{
				return false;
			}
			UpdateStats(deltaTime);
			PacketSocket.RecvStatus recvStatus;
			do
			{
				recvStatus = m_socket.Recv(out var packet);
				switch (recvStatus)
				{
				case PacketSocket.RecvStatus.PackageReady:
				{
					m_timeSinceLastRecvPackage = 0f;
					m_recvData += packet.Length;
					m_totalRecvData += packet.Length;
					MemoryStream input = new MemoryStream(packet);
					BinaryReader binaryReader = new BinaryReader(input);
					try
					{
						switch (binaryReader.ReadByte())
						{
						case 1:
							RecvInvocation(binaryReader);
							break;
						}
					}
					catch (Exception ex)
					{
						PLog.LogError("ERROR: exception while handling rpc , disconnecting client: \n " + ex.ToString());
						m_socket.Close();
					}
					break;
				}
				case PacketSocket.RecvStatus.ReceivedData:
					m_timeSinceLastRecvPackage = 0f;
					break;
				}
			}
			while (recvStatus != PacketSocket.RecvStatus.NoData && recvAll);
			return true;
		}

		private void UpdateStats(float dt)
		{
			m_updateStatsTimer += dt;
			if (m_updateStatsTimer > 1f)
			{
				m_sentDataPerSec = (int)((float)m_sentData / m_updateStatsTimer);
				m_recvDataPerSec = (int)((float)m_recvData / m_updateStatsTimer);
				m_sentData = 0;
				m_recvData = 0;
				m_updateStatsTimer = 0f;
			}
		}

		private void RecvInvocation(BinaryReader reader)
		{
			byte[] b = reader.ReadBytes(16);
			Guid key = new Guid(b);
			if (m_methods.TryGetValue(key, out var value))
			{
				char c = reader.ReadChar();
				List<object> list = new List<object>();
				for (int i = 0; i < c; i++)
				{
					Deserialize(reader, list);
				}
				value(this, list);
			}
		}

		private string GetHandlerName(Guid id)
		{
			foreach (KeyValuePair<string, Guid> item in m_nameToID)
			{
				if (item.Value == id)
				{
					return item.Key;
				}
			}
			return string.Empty;
		}

		private float GetDeltaTime()
		{
			float num = (float)m_stopWatch.ElapsedMilliseconds / 1000f;
			m_stopWatch.Reset();
			m_stopWatch.Start();
			if (num > 0.1f)
			{
				num = 0.1f;
			}
			return num;
		}

		private void PingThread()
		{
			float num = 0f;
			float num2 = 0.2f;
			while (!m_abortPingThread && m_socket.IsOpen())
			{
				num += num2;
				if (num > m_pingInterval)
				{
					num = 0f;
					MemoryStream memoryStream = new MemoryStream();
					memoryStream.WriteByte(2);
					m_socket.Send(memoryStream.ToArray());
				}
				Thread.Sleep((int)(num2 * 1000f));
				m_timeSinceMainThreadUpdate += num2;
				if (m_mainThreadTimeout > 0f && m_timeSinceMainThreadUpdate > m_mainThreadTimeout)
				{
					m_socket.Close();
				}
			}
		}

		private bool UpdatePingTimeout(float dt)
		{
			m_timeSinceMainThreadUpdate = 0f;
			m_timeSinceLastRecvPackage += dt;
			if (m_timeSinceLastRecvPackage > m_pingTimeout)
			{
				return false;
			}
			return true;
		}

		public void Register(string name, Handler del)
		{
			Guid guid = GenerateGuid(name);
			m_nameToID[name] = guid;
			m_methods[guid] = del;
		}

		public void Unregister(string name)
		{
			if (m_nameToID.TryGetValue(name, out var value))
			{
				m_nameToID.Remove(name);
				m_methods.Remove(value);
			}
		}

		public void Invoke(string name, List<object> args)
		{
			object[] array = new object[args.Count];
			for (int i = 0; i < args.Count; i++)
			{
				array[i] = args[i];
			}
			Invoke(name, array);
		}

		public void Invoke(string name, params object[] args)
		{
			//Discarded unreachable code: IL_00c4
			if (!m_nameToID.TryGetValue(name, out var value))
			{
				value = GenerateGuid(name);
				m_nameToID.Add(name, value);
			}
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			memoryStream.WriteByte(1);
			byte[] array = value.ToByteArray();
			memoryStream.Write(array, 0, array.Length);
			binaryWriter.Write((char)args.Length);
			int num = 0;
			foreach (object data in args)
			{
				try
				{
					Serialize(binaryWriter, data);
				}
				catch (Exception ex)
				{
					PLog.LogError("Serialize error: " + ex.Message);
					PLog.LogError("Error invoking " + name + " in argument " + num);
					throw;
				}
				num++;
			}
			m_sentData += (int)memoryStream.Length;
			m_totalSentData += (int)memoryStream.Length;
			m_socket.Send(memoryStream.ToArray());
		}

		private void Serialize(BinaryWriter writer, object data)
		{
			if (data is int)
			{
				writer.Write('\0');
				writer.Write((int)data);
				return;
			}
			if (data is bool)
			{
				writer.Write('\u0001');
				writer.Write((bool)data);
				return;
			}
			if (data is float)
			{
				writer.Write('\u0002');
				writer.Write((float)data);
				return;
			}
			if (data is double)
			{
				writer.Write('\u0003');
				writer.Write((double)data);
				return;
			}
			if (data is long)
			{
				writer.Write('\t');
				writer.Write((long)data);
				return;
			}
			if (data is string)
			{
				string value = data as string;
				writer.Write('\u0004');
				writer.Write(value);
				return;
			}
			if (data is byte[])
			{
				byte[] array = data as byte[];
				writer.Write('\u0005');
				writer.Write(array.Length);
				writer.Write(array);
				return;
			}
			if (data is float[])
			{
				float[] array2 = data as float[];
				writer.Write('\u0006');
				writer.Write(array2.Length);
				float[] array3 = array2;
				foreach (float value2 in array3)
				{
					writer.Write(value2);
				}
				return;
			}
			if (data is int[])
			{
				int[] array4 = data as int[];
				writer.Write('\v');
				writer.Write(array4.Length);
				int[] array5 = array4;
				foreach (int value3 in array5)
				{
					writer.Write(value3);
				}
				return;
			}
			if (data is double[])
			{
				double[] array6 = data as double[];
				writer.Write('\a');
				writer.Write(array6.Length);
				double[] array7 = array6;
				foreach (double value4 in array7)
				{
					writer.Write(value4);
				}
				return;
			}
			if (data is string[])
			{
				string[] array8 = data as string[];
				writer.Write('\n');
				writer.Write(array8.Length);
				string[] array9 = array8;
				foreach (string value5 in array9)
				{
					writer.Write(value5);
				}
				return;
			}
			if (data is Dictionary<string, int>)
			{
				writer.Write('\b');
				Dictionary<string, int> dictionary = data as Dictionary<string, int>;
				writer.Write(dictionary.Count);
				foreach (KeyValuePair<string, int> item in dictionary)
				{
					writer.Write(item.Key);
					writer.Write(item.Value);
				}
				return;
			}
			if (data == null)
			{
				throw new Exception("Unhandled type, is null ");
			}
			throw new Exception("Unhandled type " + data.GetType().ToString());
		}

		private void Deserialize(BinaryReader reader, List<object> args)
		{
			Type type = (Type)reader.ReadChar();
			switch (type)
			{
			case Type.Int:
				args.Add(reader.ReadInt32());
				break;
			case Type.Bool:
				args.Add(reader.ReadBoolean());
				break;
			case Type.Float:
				args.Add(reader.ReadSingle());
				break;
			case Type.Double:
				args.Add(reader.ReadDouble());
				break;
			case Type.Long:
				args.Add(reader.ReadInt64());
				break;
			case Type.String:
				args.Add(reader.ReadString());
				break;
			case Type.ByteArray:
			{
				int count = reader.ReadInt32();
				byte[] item = reader.ReadBytes(count);
				args.Add(item);
				break;
			}
			case Type.FloatArray:
			{
				int num2 = reader.ReadInt32();
				float[] array = new float[num2];
				for (int j = 0; j < num2; j++)
				{
					array[j] = reader.ReadSingle();
				}
				args.Add(array);
				break;
			}
			case Type.IntArray:
			{
				int num5 = reader.ReadInt32();
				int[] array4 = new int[num5];
				for (int m = 0; m < num5; m++)
				{
					array4[m] = reader.ReadInt32();
				}
				args.Add(array4);
				break;
			}
			case Type.DoubleArray:
			{
				int num4 = reader.ReadInt32();
				double[] array3 = new double[num4];
				for (int l = 0; l < num4; l++)
				{
					array3[l] = reader.ReadDouble();
				}
				args.Add(array3);
				break;
			}
			case Type.StringArray:
			{
				int num3 = reader.ReadInt32();
				string[] array2 = new string[num3];
				for (int k = 0; k < num3; k++)
				{
					array2[k] = reader.ReadString();
				}
				args.Add(array2);
				break;
			}
			case Type.StringIntDic:
			{
				int num = reader.ReadInt32();
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				for (int i = 0; i < num; i++)
				{
					string key = reader.ReadString();
					int value = reader.ReadInt32();
					dictionary.Add(key, value);
				}
				args.Add(dictionary);
				break;
			}
			default:
				throw new Exception("Unhandled type " + type);
			}
		}

		private Guid GenerateGuid(string name)
		{
			//Discarded unreachable code: IL_0027
			using MD5 mD = MD5.Create();
			byte[] b = mD.ComputeHash(Encoding.Default.GetBytes(name));
			return new Guid(b);
		}

		public void Close()
		{
			m_abortPingThread = true;
			m_pingThread.Join();
			m_socket.Close();
		}

		public int GetTotalSentData()
		{
			return m_totalSentData;
		}

		public int GetTotalRecvData()
		{
			return m_totalRecvData;
		}

		public int GetSentDataPerSec()
		{
			return m_sentDataPerSec;
		}

		public int GetRecvDataPerSec()
		{
			return m_recvDataPerSec;
		}
	}
}
