using System;

namespace CommonLib
{
	public struct IntVec3 : IYamlCustomSerialization, IEquatable<IntVec3>
	{
		[Tag(1, null, true)]
		public int x;

		[Tag(2, null, true)]
		public int y;

		[Tag(3, null, true)]
		public int z;

		public IntVec2 Xy
		{
			get
			{
				return new IntVec2(x, y);
			}
			set
			{
				x = value.x;
				y = value.y;
			}
		}

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out x, out y, out z);
		}

		public string yaml_serialize()
		{
			return utils.pack_to_string(x, y, z);
		}

		public override string ToString()
		{
			return utils.pack_to_string(x, y, z);
		}

		public IntVec3(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static explicit operator Vec3(IntVec3 v)
		{
			return new Vec3(v.x, v.y, v.z);
		}

		public static explicit operator IntVec3(Vec3 v)
		{
			return new IntVec3((int)v.x, (int)v.y, (int)v.z);
		}

		public static implicit operator IntVec3(IntVec2 v)
		{
			return new IntVec3(v.x, v.y, 0);
		}

		public static IntVec3 operator +(IntVec3 value1, IntVec3 value2)
		{
			value1.x += value2.x;
			value1.y += value2.y;
			value1.z += value2.z;
			return value1;
		}

		public static IntVec3 operator -(IntVec3 value1, IntVec3 value2)
		{
			value1.x -= value2.x;
			value1.y -= value2.y;
			value1.z -= value2.z;
			return value1;
		}

		public static IntVec3 operator *(IntVec3 value, int scaleFactor)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			value.z *= scaleFactor;
			return value;
		}

		public static bool operator !=(IntVec3 a, IntVec3 b)
		{
			if (a.x == b.x && a.y == b.y)
			{
				return a.z != b.z;
			}
			return true;
		}

		public static bool operator ==(IntVec3 a, IntVec3 b)
		{
			if (a.x == b.x && a.y == b.y)
			{
				return a.z == b.z;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return x + y + z;
		}

		public bool Equals(IntVec3 other)
		{
			if (x == other.x && y == other.y)
			{
				return z == other.z;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is IntVec3)
			{
				return Equals((IntVec3)obj);
			}
			return false;
		}
	}
}
