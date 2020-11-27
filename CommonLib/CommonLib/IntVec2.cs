using System;
using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct IntVec2 : IYamlCustomSerialization, IEquatable<IntVec2>
	{
		[Tag(1, null, true)]
		public int x;

		[Tag(2, null, true)]
		public int y;

		public static IntVec2 one = new IntVec2(1, 1);

		public static IntVec2 zero = new IntVec2(0, 0);

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out x, out y);
		}

		public string yaml_serialize()
		{
			return utils.pack_to_string(x, y);
		}

		public override string ToString()
		{
			return yaml_serialize();
		}

		public bool Equals(IntVec2 other)
		{
			if (x == other.x)
			{
				return y == other.y;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is IntVec2))
			{
				return false;
			}
			return Equals((IntVec2)obj);
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() + y.GetHashCode();
		}

		public IntVec2(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public static IntVec2 operator *(IntVec2 value, int scaleFactor)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			return value;
		}

		public static IntVec2 operator /(IntVec2 value, int scaleFactor)
		{
			value.x /= scaleFactor;
			value.y /= scaleFactor;
			return value;
		}

		public static IntVec2 operator *(int scaleFactor, IntVec2 value)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			return value;
		}

		public static IntVec2 operator -(IntVec2 value1, IntVec2 value2)
		{
			value1.x -= value2.x;
			value1.y -= value2.y;
			return value1;
		}

		public static IntVec2 operator +(IntVec2 value1, IntVec2 value2)
		{
			value1.x += value2.x;
			value1.y += value2.y;
			return value1;
		}

		public static explicit operator IntVec2(Vec2 v)
		{
			return new IntVec2((int)v.x, (int)v.y);
		}

		public static implicit operator Point(IntVec2 v)
		{
			return new Point(v.x, v.y);
		}

		public static implicit operator IntVec2(Point v)
		{
			return new IntVec2(v.X, v.Y);
		}

		public static bool operator ==(IntVec2 value1, IntVec2 value2)
		{
			if (value1.x == value2.x)
			{
				return value1.y == value2.y;
			}
			return false;
		}

		public static bool operator !=(IntVec2 value1, IntVec2 value2)
		{
			if (value1.x == value2.x)
			{
				return value1.y != value2.y;
			}
			return true;
		}
	}
}
