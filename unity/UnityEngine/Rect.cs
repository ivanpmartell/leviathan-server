using System;

namespace UnityEngine
{
	public struct Rect
	{
		private float m_XMin;

		private float m_YMin;

		private float m_Width;

		private float m_Height;

		public float x
		{
			get
			{
				return m_XMin;
			}
			set
			{
				m_XMin = value;
			}
		}

		public float y
		{
			get
			{
				return m_YMin;
			}
			set
			{
				m_YMin = value;
			}
		}

		public Vector2 center
		{
			get
			{
				return new Vector2(x + m_Width / 2f, y + m_Height / 2f);
			}
			set
			{
				m_XMin = value.x - m_Width / 2f;
				m_YMin = value.y - m_Height / 2f;
			}
		}

		public float width
		{
			get
			{
				return m_Width;
			}
			set
			{
				m_Width = value;
			}
		}

		public float height
		{
			get
			{
				return m_Height;
			}
			set
			{
				m_Height = value;
			}
		}

		[Obsolete("use xMin")]
		public float left => m_XMin;

		[Obsolete("use xMax")]
		public float right => m_XMin + m_Width;

		[Obsolete("use yMin")]
		public float top => m_YMin;

		[Obsolete("use yMax")]
		public float bottom => m_YMin + m_Height;

		public float xMin
		{
			get
			{
				return m_XMin;
			}
			set
			{
				float num = xMax;
				m_XMin = value;
				m_Width = num - m_XMin;
			}
		}

		public float yMin
		{
			get
			{
				return m_YMin;
			}
			set
			{
				float num = yMax;
				m_YMin = value;
				m_Height = num - m_YMin;
			}
		}

		public float xMax
		{
			get
			{
				return m_Width + m_XMin;
			}
			set
			{
				m_Width = value - m_XMin;
			}
		}

		public float yMax
		{
			get
			{
				return m_Height + m_YMin;
			}
			set
			{
				m_Height = value - m_YMin;
			}
		}

		public Rect(float left, float top, float width, float height)
		{
			m_XMin = left;
			m_YMin = top;
			m_Width = width;
			m_Height = height;
		}

		public Rect(Rect source)
		{
			m_XMin = source.m_XMin;
			m_YMin = source.m_YMin;
			m_Width = source.m_Width;
			m_Height = source.m_Height;
		}

		public static Rect MinMaxRect(float left, float top, float right, float bottom)
		{
			return new Rect(left, top, right - left, bottom - top);
		}

		public void Set(float left, float top, float width, float height)
		{
			m_XMin = left;
			m_YMin = top;
			m_Width = width;
			m_Height = height;
		}

		public override string ToString()
		{
			return $"(left:{x:F2}, top:{y:F2}, width:{width:F2}, height:{height:F2})";
		}

		public string ToString(string format)
		{
			return $"(left:{x.ToString(format)}, top:{y.ToString(format)}, width:{width.ToString(format)}, height:{height.ToString(format)})";
		}

		public bool Contains(Vector2 point)
		{
			return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
		}

		public bool Contains(Vector3 point)
		{
			return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ (width.GetHashCode() << 2) ^ (y.GetHashCode() >> 2) ^ (height.GetHashCode() >> 1);
		}

		public override bool Equals(object other)
		{
			if (!(other is Rect))
			{
				return false;
			}
			Rect rect = (Rect)other;
			return x.Equals(rect.x) && y.Equals(rect.y) && width.Equals(rect.width) && height.Equals(rect.height);
		}

		public static bool operator !=(Rect lhs, Rect rhs)
		{
			return lhs.x != rhs.x || lhs.y != rhs.y || lhs.width != rhs.width || lhs.height != rhs.height;
		}

		public static bool operator ==(Rect lhs, Rect rhs)
		{
			return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
		}
	}
}
