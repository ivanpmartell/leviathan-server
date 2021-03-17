namespace UnityEngine
{
	public struct LocationInfo
	{
		private double m_Timestamp;

		private float m_Latitude;

		private float m_Longitude;

		private float m_Altitude;

		private float m_HorizontalAccuracy;

		private float m_VerticalAccuracy;

		public float latitude => m_Latitude;

		public float longitude => m_Longitude;

		public float altitude => m_Altitude;

		public float horizontalAccuracy => m_HorizontalAccuracy;

		public float verticalAccuracy => m_VerticalAccuracy;

		public double timestamp => m_Timestamp;
	}
}
