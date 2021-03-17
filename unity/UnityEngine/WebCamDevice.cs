namespace UnityEngine
{
	public struct WebCamDevice
	{
		private string m_Name;

		private int m_Flags;

		public string name => m_Name;

		public bool isFrontFacing => (m_Flags & 1) == 1;
	}
}
