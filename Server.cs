using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

namespace leviathan_server
{
    class Server
    {
        TcpListener server = null;
		Dictionary<Guid, Delegate> dic;

		public Server(string ip, int port)
        {
			dic = new Dictionary<Guid, Delegate>
			{
				{ GenerateGuid("Login"), new Func<object[], byte[]>(Login) }
			};

			IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            server.Start();
            StartListener();
        }
        public void StartListener()
        {
            try
            {
				while (true)
                {
					TcpClient client = server.AcceptTcpClient();
					Console.WriteLine("Connected");
					Thread t = new Thread(new ParameterizedThreadStart(HandleDevice));
					t.Start(client);
				}
            }
            catch (SocketException ex)
            {
				Console.WriteLine("SocketException: {0}", ex);
				server.Stop();
				Console.WriteLine("Listener  stopped.");
			}
        }

		public void HandleDevice(Object obj)
        {
            TcpClient client = (TcpClient)obj;
			while(true)
			{
				NetworkStream stream = client.GetStream();
				try
				{
					using (BinaryReader r = new BinaryReader(stream, Encoding.Default, true))
					{
						var length = r.ReadInt32();
						Console.WriteLine("Received {0} bytes from client.", length);
						var flag = (int)r.ReadByte();
						if (flag == 1 && length > 15)
						{
							var guid_b = r.ReadBytes(16);
							var guid = new Guid(guid_b);
							var args_l = (int)r.ReadChar();
							var args = new List<object>();
							for (var j = 0; j < args_l; j++)
							{
								Deserialize(r, args);
							}
							//Call guid method with args
							//This method will reply to registered methods
							try
							{
								byte[] response = (byte[])dic[guid].DynamicInvoke((object)args.ToArray());
								Send(stream, response);
								Console.WriteLine("Sent message: {0}", BitConverter.ToString(response));
							}
							catch(KeyNotFoundException)
							{
								Console.WriteLine("Method not found for guid {0}", guid.ToString());
							}
						}
						else if(flag == 2)
						{
							byte[] response = Ping();
							Send(stream, response);
						}
					}
				}
				catch (EndOfStreamException){
					Console.WriteLine("Client Disconnected.");
					client.Close();
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Exception: {0}", ex.ToString());
					client.Close();
					break;
				}
				Thread.Sleep(10);
			}
        }

		private void Send(NetworkStream stream, byte[] message)
		{
			stream.Write(message, 0, message.Length);
		}

		private byte[] Login(object[] args)
        {
			var guid = GenerateGuid("LoginOK");
            Console.WriteLine(guid.ToString());
			return Invoke(guid);
        }

		private byte[] Ping()
        {
            MemoryStream memoryStream = new MemoryStream();
            byte[] length = BitConverter.GetBytes(1);
            memoryStream.Write(length, 0, length.Length);
            memoryStream.WriteByte(2);
            byte[] byteData = memoryStream.ToArray();
            Console.WriteLine("Sending ping: {0}", BitConverter.ToString(byteData));
            return byteData;
        }

		private byte[] Invoke(Guid method, params object[] args)
		{
			MemoryStream memoryStream2 = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream2);
			int num = 0;
			foreach (object data in args)
			{
				try
				{
					Serialize(binaryWriter, data);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Serialize error: " + ex.Message);
					Console.WriteLine("Error invoking " + method.ToString() + " in argument " + num);
					throw;
				}
				num++;
			}
            MemoryStream memoryStream = new MemoryStream();
            Console.WriteLine("Current stream: {0}, Args: {1}", BitConverter.ToString(memoryStream.ToArray()), binaryWriter.BaseStream.Length);
            byte[] length = BitConverter.GetBytes((int)binaryWriter.BaseStream.Length + 16 + 1 + 2);
            memoryStream.Write(length, 0, length.Length);
            Console.WriteLine("Length: {0}", BitConverter.ToString(length));
            memoryStream.WriteByte(1);
			byte[] array = method.ToByteArray();
			memoryStream.Write(array, 0, array.Length);
            Console.WriteLine("current stream: {0}", BitConverter.ToString(memoryStream.ToArray()));
            byte[] arguments_len = BitConverter.GetBytes((char)args.Length);
            memoryStream.Write(arguments_len, 0, arguments_len.Length);
            Console.WriteLine("Current stream: {0}, Args: {1}, Char Representation:{2}", BitConverter.ToString(memoryStream.ToArray()), args.Length, arguments_len.Length);
            byte[] arguments = memoryStream2.ToArray();
            memoryStream.Write(arguments, 0, arguments.Length);
            return memoryStream.ToArray();
		}

		private Guid GenerateGuid(string name)
		{
			//Discarded unreachable code: IL_0027
			using MD5 mD = MD5.Create();
			byte[] b = mD.ComputeHash(Encoding.Default.GetBytes(name));
			return new Guid(b);
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
			var type_c = reader.ReadChar();
			Console.WriteLine(type_c);
			Type type = (Type)type_c;
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
	}
}
