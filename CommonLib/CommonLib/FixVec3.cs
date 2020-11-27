using System;
using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct FixVec3 : IYamlCustomSerialization, IEquatable<FixVec3>
	{
		public static readonly FixVec3 zero = default(FixVec3);

		public static readonly FixVec3 one = new FixVec3(1, 1, 1);

		public static readonly FixVec3 UnitX = new FixVec3(1, 0, 0);

		public static readonly FixVec3 UnitY = new FixVec3(0, 1, 0);

		public static readonly FixVec3 UnitZ = new FixVec3(0, 0, 1);

		[Tag(1, null, true)]
		public Fix x;

		[Tag(2, null, true)]
		public Fix y;

		[Tag(3, null, true)]
		public Fix z;

		public FixVec2 Xy => new FixVec2(x, y);

		public bool IsZero
		{
			get
			{
				if (x == 0 && y == 0)
				{
					return z == 0;
				}
				return false;
			}
		}

		public FixVec3(Fix x, Fix y, Fix z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public FixVec3(Vec2 v)
		{
			x = (Fix)v.x;
			y = (Fix)v.y;
			z = 0;
		}

		public FixVec3(FixVec2 v, Fix z)
		{
			x = v.x;
			y = v.y;
			this.z = z;
		}

		public FixVec3(Vec3 v)
		{
			x = (Fix)v.x;
			y = (Fix)v.y;
			z = (Fix)v.z;
		}

		public static explicit operator Vec3(FixVec3 value)
		{
			return new Vec3((float)value.x, (float)value.y, (float)value.z);
		}

		public static explicit operator FixVec3(Vec3 value)
		{
			return new FixVec3((Fix)value.x, (Fix)value.y, (Fix)value.z);
		}

		public static explicit operator Vector3(FixVec3 value)
		{
			return new Vector3((float)value.x, (float)value.y, (float)value.z);
		}

		public static explicit operator FixVec3(Vector3 value)
		{
			return new FixVec3((Fix)value.X, (Fix)value.Y, (Fix)value.Z);
		}

		[Obsolete]
		public Vec3 obsolete_float()
		{
			return (Vec3)this;
		}

		public static FixVec3 operator +(FixVec3 rhs)
		{
			return rhs;
		}

		public static FixVec3 operator -(FixVec3 rhs)
		{
			return new FixVec3(-rhs.x, -rhs.y, -rhs.z);
		}

		public static FixVec3 operator +(FixVec3 lhs, FixVec3 rhs)
		{
			return new FixVec3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
		}

		public static FixVec3 operator +(FixVec3 lhs, FixVec2 rhs)
		{
			return new FixVec3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z);
		}

		public static FixVec3 operator -(FixVec3 lhs, FixVec3 rhs)
		{
			return new FixVec3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
		}

		public static FixVec3 operator +(FixVec3 lhs, Fix rhs)
		{
			return lhs.ScalarAdd(rhs);
		}

		public static FixVec3 operator +(Fix lhs, FixVec3 rhs)
		{
			return rhs.ScalarAdd(lhs);
		}

		public static FixVec3 operator -(FixVec3 lhs, Fix rhs)
		{
			return new FixVec3(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
		}

		public static FixVec3 operator *(FixVec3 lhs, Fix rhs)
		{
			return lhs.ScalarMultiply(rhs);
		}

		public static FixVec3 operator *(FixVec3 lhs, FixVec2 rhs)
		{
			lhs.x *= rhs.x;
			lhs.y *= rhs.y;
			return lhs;
		}

		public static FixVec3 operator *(Fix lhs, FixVec3 rhs)
		{
			return rhs.ScalarMultiply(lhs);
		}

		public static FixVec3 operator /(FixVec3 lhs, Fix rhs)
		{
			return new FixVec3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
		}

		public void lerp(FixVec3 target, Fix factor)
		{
			FixVec3 fixVec = target - this;
			this += fixVec * factor;
		}

		public void lerp(FixVec3 target, Fix factor, Fix maxSpeed)
		{
			if (!(maxSpeed <= 0))
			{
				FixVec3 fixVec = target - this;
				fixVec *= factor;
				Fix magnitude_can_be_zero = fixVec.get_magnitude_can_be_zero();
				if (magnitude_can_be_zero < maxSpeed)
				{
					this += fixVec;
				}
				else
				{
					this += fixVec / magnitude_can_be_zero * maxSpeed;
				}
			}
		}

		public Fix Dot(FixVec3 rhs)
		{
			return x * rhs.x + y * rhs.y + z * rhs.z;
		}

		private FixVec3 ScalarAdd(Fix value)
		{
			return new FixVec3(x + value, y + value, z + value);
		}

		private FixVec3 ScalarMultiply(Fix value)
		{
			return new FixVec3(x * value, y * value, z * value);
		}

		public Fix get_magnitude()
		{
			ulong num = (ulong)((long)x.Raw * (long)x.Raw + (long)y.Raw * (long)y.Raw + (long)z.Raw * (long)z.Raw);
			return new Fix((int)(math.sqrt_ulong(num << 2) + 1) >> 1);
		}

		public Fix get_magnitude_can_be_zero()
		{
			if (x == 0 && y == 0 && z == 0)
			{
				return 0;
			}
			return get_magnitude();
		}

		public Fix get_distance(FixVec3 other)
		{
			return (other - this).get_magnitude_can_be_zero();
		}

		public FixVec3 get_min(FixVec3 other)
		{
			return new FixVec3(math.min(x, other.x), math.min(y, other.y), math.min(z, other.z));
		}

		public FixVec3 get_max(FixVec3 other)
		{
			return new FixVec3(math.max(x, other.x), math.max(y, other.y), math.max(z, other.z));
		}

		public FixVec3 get_normalized()
		{
			if (x == 0 && y == 0 && z == 0)
			{
				return zero;
			}
			Fix magnitude = get_magnitude();
			return new FixVec3(x / magnitude, y / magnitude, z / magnitude);
		}

		public override string ToString()
		{
			return $"{x} {y} {z}";
		}

		public static FixVec3 parse(string s)
		{
			string[] array = s.Split(' ');
			FixVec3 result = default(FixVec3);
			result.x = Fix.parse(array.try_get(0));
			result.y = Fix.parse(array.try_get(1));
			result.z = Fix.parse(array.try_get(2));
			return result;
		}

		public string yaml_serialize()
		{
			return ToString();
		}

		public void yaml_deserialize(string src)
		{
			this = parse(src);
		}

		public static bool operator ==(FixVec3 lhs, FixVec3 rhs)
		{
			if (lhs.x.raw == rhs.x.raw && lhs.y.raw == rhs.y.raw)
			{
				return lhs.z.raw == rhs.z.raw;
			}
			return false;
		}

		public static bool operator !=(FixVec3 lhs, FixVec3 rhs)
		{
			if (lhs.x.raw == rhs.x.raw && lhs.y.raw == rhs.y.raw)
			{
				return lhs.z.raw != rhs.z.raw;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return x.raw + y.raw + z.raw;
		}

		public bool Equals(FixVec3 other)
		{
			if (x.raw == other.x.raw && y.raw == other.y.raw)
			{
				return z.raw == other.z.raw;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is FixVec3)
			{
				return Equals((FixVec3)obj);
			}
			return false;
		}
	}
}
