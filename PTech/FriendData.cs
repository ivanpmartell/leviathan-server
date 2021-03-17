using System.IO;

namespace PTech
{
	internal class FriendData
	{
		public enum FriendStatus
		{
			IsFriend,
			Requested,
			NeedAccept,
			NotFriend
		}

		public int m_friendID;

		public string m_name;

		public FriendStatus m_status;

		public int m_flagID;

		public bool m_online;

		public byte[] ToArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(m_friendID);
			binaryWriter.Write(m_name);
			binaryWriter.Write((int)m_status);
			binaryWriter.Write(m_flagID);
			binaryWriter.Write(m_online);
			return memoryStream.ToArray();
		}

		public void FromArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader binaryReader = new BinaryReader(input);
			m_friendID = binaryReader.ReadInt32();
			m_name = binaryReader.ReadString();
			m_status = (FriendStatus)binaryReader.ReadInt32();
			m_flagID = binaryReader.ReadInt32();
			m_online = binaryReader.ReadBoolean();
		}
	}
}
