using System;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class GUIStyleState
	{
		[SerializeField]
		private Texture2D m_Background;

		[SerializeField]
		private Color m_TextColor = Color.black;

		[NonSerialized]
		internal GUIStyle owner;

		public Texture2D background
		{
			get
			{
				return m_Background;
			}
			set
			{
				m_Background = value;
				if (owner != null)
				{
					owner.Apply();
				}
			}
		}

		public Color textColor
		{
			get
			{
				return m_TextColor;
			}
			set
			{
				m_TextColor = value;
				if (owner != null)
				{
					owner.Apply();
				}
			}
		}

		internal void CopyFrom(GUIStyleState other)
		{
			m_Background = other.m_Background;
			m_TextColor = other.m_TextColor;
			if (owner != null)
			{
				owner.Apply();
			}
		}
	}
}
