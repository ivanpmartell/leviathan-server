using System.Runtime.InteropServices;

namespace UnityEngine
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class SplatPrototype
	{
		private Texture2D m_Texture;

		private Vector2 m_TileSize = new Vector2(15f, 15f);

		private Vector2 m_TileOffset = new Vector2(0f, 0f);

		public Texture2D texture
		{
			get
			{
				return m_Texture;
			}
			set
			{
				m_Texture = value;
			}
		}

		public Vector2 tileSize
		{
			get
			{
				return m_TileSize;
			}
			set
			{
				m_TileSize = value;
			}
		}

		public Vector2 tileOffset
		{
			get
			{
				return m_TileOffset;
			}
			set
			{
				m_TileOffset = value;
			}
		}
	}
}
