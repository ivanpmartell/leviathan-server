namespace UnityEngine
{
	public struct ContactPoint
	{
		private Vector3 m_Point;

		private Vector3 m_Normal;

		private Collider m_ThisCollider;

		private Collider m_OtherCollider;

		public Vector3 point => m_Point;

		public Vector3 normal => m_Normal;

		public Collider thisCollider => m_ThisCollider;

		public Collider otherCollider => m_OtherCollider;
	}
}
