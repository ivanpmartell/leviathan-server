using System;
using System.Runtime.InteropServices;

namespace UnityEngine
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public sealed class RectOffset
	{
		[SerializeField]
		internal int m_Left;

		[SerializeField]
		internal int m_Right;

		[SerializeField]
		internal int m_Top;

		[SerializeField]
		internal int m_Bottom;

		[NonSerialized]
		internal GUIStyle owner;

		public int left
		{
			get
			{
				return m_Left;
			}
			set
			{
				m_Left = value;
				if (owner != null)
				{
					owner.Apply();
				}
			}
		}

		public int right
		{
			get
			{
				return m_Right;
			}
			set
			{
				m_Right = value;
				if (owner != null)
				{
					owner.Apply();
				}
			}
		}

		public int top
		{
			get
			{
				return m_Top;
			}
			set
			{
				m_Top = value;
				if (owner != null)
				{
					owner.Apply();
				}
			}
		}

		public int bottom
		{
			get
			{
				return m_Bottom;
			}
			set
			{
				m_Bottom = value;
				if (owner != null)
				{
					owner.Apply();
				}
			}
		}

		public int horizontal => m_Left + m_Right;

		public int vertical => m_Top + m_Bottom;

		public RectOffset()
		{
			m_Left = 0;
			m_Top = 0;
			m_Right = 0;
			m_Bottom = 0;
		}

		public RectOffset(int left, int right, int top, int bottom)
		{
			m_Left = left;
			m_Top = top;
			m_Right = right;
			m_Bottom = bottom;
		}

		public Rect Add(Rect rect)
		{
			return new Rect(rect.x - (float)m_Left, rect.y - (float)top, rect.width + (float)m_Left + (float)m_Right, rect.height + (float)m_Top + (float)m_Bottom);
		}

		public Rect Remove(Rect rect)
		{
			return new Rect(rect.x + (float)m_Left, rect.y + (float)m_Top, rect.width - (float)m_Left - (float)m_Right, rect.height - (float)m_Top - (float)m_Bottom);
		}

		internal void CopyFrom(RectOffset other)
		{
			m_Left = other.m_Left;
			m_Right = other.m_Right;
			m_Top = other.m_Top;
			m_Bottom = other.m_Bottom;
			if (owner != null)
			{
				owner.Apply();
			}
		}

		public override string ToString()
		{
			return $"RectOffset (l:{m_Left} r:{m_Right} t:{m_Top} b:{m_Bottom})";
		}
	}
}
