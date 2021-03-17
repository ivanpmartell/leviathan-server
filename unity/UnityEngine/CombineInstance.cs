namespace UnityEngine
{
	public struct CombineInstance
	{
		private Mesh m_Mesh;

		private int m_SubMeshIndex;

		private Matrix4x4 m_Transform;

		public Mesh mesh
		{
			get
			{
				return m_Mesh;
			}
			set
			{
				m_Mesh = value;
			}
		}

		public int subMeshIndex
		{
			get
			{
				return m_SubMeshIndex;
			}
			set
			{
				m_SubMeshIndex = value;
			}
		}

		public Matrix4x4 transform
		{
			get
			{
				return m_Transform;
			}
			set
			{
				m_Transform = value;
			}
		}
	}
}
