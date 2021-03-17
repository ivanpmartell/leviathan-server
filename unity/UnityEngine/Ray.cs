namespace UnityEngine
{
	public struct Ray
	{
		private Vector3 m_Origin;

		private Vector3 m_Direction;

		public Vector3 origin
		{
			get
			{
				return m_Origin;
			}
			set
			{
				m_Origin = value;
			}
		}

		public Vector3 direction
		{
			get
			{
				return m_Direction;
			}
			set
			{
				m_Direction = value.normalized;
			}
		}

		public Ray(Vector3 origin, Vector3 direction)
		{
			m_Origin = origin;
			m_Direction = direction.normalized;
		}

		public Vector3 GetPoint(float distance)
		{
			return m_Origin + m_Direction * distance;
		}

		public override string ToString()
		{
			return $"Origin: {m_Origin}, Dir: {m_Direction}";
		}

		public string ToString(string format)
		{
			return $"Origin: {m_Origin.ToString(format)}, Dir: {m_Direction.ToString(format)}";
		}
	}
}
