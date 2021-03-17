using System;

namespace UnityEngine
{
	public struct Color
	{
		public float r;

		public float g;

		public float b;

		public float a;

		public static Color red => new Color(1f, 0f, 0f, 1f);

		public static Color green => new Color(0f, 1f, 0f, 1f);

		public static Color blue => new Color(0f, 0f, 1f, 1f);

		public static Color white => new Color(1f, 1f, 1f, 1f);

		public static Color black => new Color(0f, 0f, 0f, 1f);

		public static Color yellow => new Color(1f, 47f / 51f, 4f / 255f, 1f);

		public static Color cyan => new Color(0f, 1f, 1f, 1f);

		public static Color magenta => new Color(1f, 0f, 1f, 1f);

		public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);

		public static Color grey => new Color(0.5f, 0.5f, 0.5f, 1f);

		public static Color clear => new Color(0f, 0f, 0f, 0f);

		public float grayscale => 0.299f * r + 0.587f * g + 0.114f * b;

		public Color linear => new Color(Mathf.GammaToLinearSpace(r), Mathf.GammaToLinearSpace(g), Mathf.GammaToLinearSpace(b), a);

		public Color gamma => new Color(Mathf.LinearToGammaSpace(r), Mathf.LinearToGammaSpace(g), Mathf.LinearToGammaSpace(b), a);

		public float this[int index]
		{
			get
			{
				return index switch
				{
					0 => r, 
					1 => g, 
					2 => b, 
					3 => a, 
					_ => throw new IndexOutOfRangeException("Invalid Vector3 index!"), 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					r = value;
					break;
				case 1:
					g = value;
					break;
				case 2:
					b = value;
					break;
				case 3:
					a = value;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid Vector3 index!");
				}
			}
		}

		public Color(float r, float g, float b, float a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public Color(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			a = 1f;
		}

		public override string ToString()
		{
			return $"RGBA({r:F3}, {g:F3}, {b:F3}, {a:F3})";
		}

		public string ToString(string format)
		{
			return $"RGBA({r.ToString(format)}, {g.ToString(format)}, {b.ToString(format)}, {a.ToString(format)})";
		}

		public override int GetHashCode()
		{
			return ((Vector4)this).GetHashCode();
		}

		public override bool Equals(object other)
		{
			if (!(other is Color))
			{
				return false;
			}
			Color color = (Color)other;
			return r.Equals(color.r) && g.Equals(color.g) && b.Equals(color.b) && a.Equals(color.a);
		}

		public static Color Lerp(Color a, Color b, float t)
		{
			t = Mathf.Clamp01(t);
			return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
		}

		public static Color operator +(Color a, Color b)
		{
			return new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
		}

		public static Color operator -(Color a, Color b)
		{
			return new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
		}

		public static Color operator *(Color a, Color b)
		{
			return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
		}

		public static Color operator *(Color a, float b)
		{
			return new Color(a.r * b, a.g * b, a.b * b, a.a * b);
		}

		public static Color operator *(float b, Color a)
		{
			return new Color(a.r * b, a.g * b, a.b * b, a.a * b);
		}

		public static Color operator /(Color a, float b)
		{
			return new Color(a.r / b, a.g / b, a.b / b, a.a / b);
		}

		public static bool operator ==(Color lhs, Color rhs)
		{
			return (Vector4)lhs == (Vector4)rhs;
		}

		public static bool operator !=(Color lhs, Color rhs)
		{
			return (Vector4)lhs != (Vector4)rhs;
		}

		public static implicit operator Vector4(Color c)
		{
			return new Vector4(c.r, c.g, c.b, c.a);
		}

		public static implicit operator Color(Vector4 v)
		{
			return new Color(v.x, v.y, v.z, v.w);
		}
	}
}
