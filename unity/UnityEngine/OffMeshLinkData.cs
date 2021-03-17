using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public struct OffMeshLinkData
	{
		private int m_Valid;

		private int m_Activated;

		private int m_InstanceID;

		private OffMeshLinkType m_LinkType;

		private Vector3 m_StartPos;

		private Vector3 m_EndPos;

		public bool valid => m_Valid != 0;

		public bool activated => m_Activated != 0;

		public OffMeshLinkType linkType => m_LinkType;

		public Vector3 startPos => m_StartPos;

		public Vector3 endPos => m_EndPos;

		public OffMeshLink offMeshLink => GetOffMeshLinkInternal(m_InstanceID);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern OffMeshLink GetOffMeshLinkInternal(int instanceID);
	}
}
