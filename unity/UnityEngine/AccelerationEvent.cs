namespace UnityEngine
{
	public struct AccelerationEvent
	{
		private Vector3 m_Acceleration;

		private float m_TimeDelta;

		public Vector3 acceleration => m_Acceleration;

		public float deltaTime => m_TimeDelta;
	}
}
