using System;

namespace UnityEngine
{
	public struct Vector2
	{
		public const float kEpsilon = 1E-05f;

		public float x;

		public float y;

		public float this[int index]
		{
			get
			{
				return index switch
				{
					0 => x, 
					1 => y, 
					_ => throw new IndexOutOfRangeException("Invalid Vector2 index!"), 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					x = value;
					break;
				case 1:
					y = value;
					break;
				default:
					throw new IndexOutOfRangeException("Invalid Vector2 index!");
				}
			}
		}

		public Vector2 normalized
		{
			get
			{
				Vector2 result = new Vector2(x, y);
				result.Normalize();
				return result;
			}
		}

		public float magnitude => Mathf.Sqrt(x * x + y * y);

		public float sqrMagnitude => x * x + y * y;

		public static Vector2 zero => new Vector2(0f, 0f);

		public static Vector2 one => new Vector2(1f, 1f);

		public static Vector2 up => new Vector2(0f, 1f);

		public static Vector2 right => new Vector2(1f, 0f);

		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public void Set(float new_x, float new_y)
		{
			x = new_x;
			y = new_y;
		}

		public static Vector2 Lerp(Vector2 from, Vector2 to, float t)
		{
			t = Mathf.Clamp01(t);
			return new Vector2(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t);
		}

		public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
		{
			Vector2 vector = target - current;
			float num = vector.magnitude;
			if (num <= maxDistanceDelta || num == 0f)
			{
				return target;
			}
			return current + vector / num * maxDistanceDelta;
		}

		public static Vector2 Scale(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x * b.x, a.y * b.y);
		}

		public void Scale(Vector2 scale)
		{
			x *= scale.x;
			y *= scale.y;
		}

		public void Normalize()
		{
			float num = magnitude;
			if (num > 1E-05f)
			{
				this /= num;
			}
			else
			{
				this = zero;
			}
		}

		public override string ToString()
		{
			return $"({x:F1}, {y:F1})";
		}

		public string ToString(string format)
		{
			return $"({x.ToString(format)}, {y.ToString(format)})";
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ (y.GetHashCode() << 2);
		}

		public override bool Equals(object other)
		{
			if (!(other is Vector2))
			{
				return false;
			}
			Vector2 vector = (Vector2)other;
			return x.Equals(vector.x) && y.Equals(vector.y);
		}

		public static float Dot(Vector2 lhs, Vector2 rhs)
		{
			return lhs.x * rhs.x + lhs.y * rhs.y;
		}

		public static float Angle(Vector2 from, Vector2 to)
		{
			return Mathf.Acos(Mathf.Clamp(Dot(from.normalized, to.normalized), -1f, 1f)) * 57.29578f;
		}

		public static float Distance(Vector2 a, Vector2 b)
		{
			return (a - b).magnitude;
		}

		public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
		{
			if (vector.sqrMagnitude > maxLength * maxLength)
			{
				return vector.normalized * maxLength;
			}
			return vector;
		}

		public static float SqrMagnitude(Vector2 a)
		{
			return a.x * a.x + a.y * a.y;
		}

		public float SqrMagnitude()
		{
			return x * x + y * y;
		}

		public static Vector2 Min(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
		}

		public static Vector2 Max(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
		}

		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x + b.x, a.y + b.y);
		}

		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x - b.x, a.y - b.y);
		}

		public static Vector2 operator -(Vector2 a)
		{
			return new Vector2(0f - a.x, 0f - a.y);
		}

		public static Vector2 operator *(Vector2 a, float d)
		{
			return new Vector2(a.x * d, a.y * d);
		}

		public static Vector2 operator *(float d, Vector2 a)
		{
			return new Vector2(a.x * d, a.y * d);
		}

		public static Vector2 operator /(Vector2 a, float d)
		{
			return new Vector2(a.x / d, a.y / d);
		}

		public static bool operator ==(Vector2 lhs, Vector2 rhs)
		{
			return SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
		}

		public static bool operator !=(Vector2 lhs, Vector2 rhs)
		{
			return SqrMagnitude(lhs - rhs) >= 9.99999944E-11f;
		}

		public static implicit operator Vector2(Vector3 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static implicit operator Vector3(Vector2 v)
		{
			return new Vector3(v.x, v.y, 0f);
		}
	}
}
