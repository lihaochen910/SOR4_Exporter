using System;
using System.Runtime.CompilerServices;

namespace CommonLib
{
	public struct FixVec2 : IYamlCustomSerialization, IEquatable<FixVec2>
	{
		public static readonly FixVec2 zero = default(FixVec2);

		public static readonly FixVec2 one = new FixVec2(1, 1);

		public static readonly FixVec2 UnitX = new FixVec2(1, 0);

		public static readonly FixVec2 UnitY = new FixVec2(0, 1);

		public static readonly FixVec2 minValue = new FixVec2(Fix.MinValue, Fix.MinValue);

		public static readonly FixVec2 maxValue = new FixVec2(Fix.MaxValue, Fix.MaxValue);

		[Tag(1, null, true)]
		public Fix x;

		[Tag(2, null, true)]
		public Fix y;

		public bool IsZero
		{
			get
			{
				if (x == 0)
				{
					return y == 0;
				}
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FixVec2(Fix x, Fix y)
		{
			this.x = x;
			this.y = y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FixVec2(Fix xy)
		{
			x = xy;
			y = xy;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FixVec2(Vec2 v)
		{
			x = (Fix)v.x;
			y = (Fix)v.y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FixVec2 new_direction_degrees(Fix angle)
		{
			return new FixVec2(math.cos_degrees(angle), math.sin_degrees(angle));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FixVec2 new_direction_radians(Fix angle)
		{
			return new FixVec2(math.cos_radians(angle), math.sin_radians(angle));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Vec2(FixVec2 value)
		{
			return new Vec2((float)value.x, (float)value.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator FixVec2(FixVec3 value)
		{
			return new FixVec2(value.x, value.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator FixVec3(FixVec2 value)
		{
			return new FixVec3(value.x, value.y, 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator FixVec2(Vec2 value)
		{
			return new FixVec2((Fix)value.x, (Fix)value.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator FixVec2(Vec3 value)
		{
			return new FixVec2((Fix)value.x, (Fix)value.y);
		}

		[Obsolete]
		public Vec2 obsolete_float()
		{
			return (Vec2)this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FixVec2 operator +(FixVec2 rhs)
		{
			return rhs;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FixVec2 operator -(FixVec2 rhs)
		{
			return new FixVec2(-rhs.x, -rhs.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FixVec2 operator +(FixVec2 lhs, FixVec2 rhs)
		{
			return new FixVec2(lhs.x + rhs.x, lhs.y + rhs.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FixVec2 operator -(FixVec2 lhs, FixVec2 rhs)
		{
			return new FixVec2(lhs.x - rhs.x, lhs.y - rhs.y);
		}

		public static FixVec2 operator +(FixVec2 lhs, Fix rhs)
		{
			return lhs.ScalarAdd(rhs);
		}

		public static FixVec2 operator +(Fix lhs, FixVec2 rhs)
		{
			return rhs.ScalarAdd(lhs);
		}

		public static FixVec2 operator -(FixVec2 lhs, Fix rhs)
		{
			return new FixVec2(lhs.x - rhs, lhs.y - rhs);
		}

		public static FixVec2 operator *(FixVec2 lhs, Fix rhs)
		{
			return lhs.ScalarMultiply(rhs);
		}

		public static FixVec2 operator *(Fix lhs, FixVec2 rhs)
		{
			return rhs.ScalarMultiply(lhs);
		}

		public static FixVec2 operator *(FixVec2 lhs, FixVec2 rhs)
		{
			lhs.x *= rhs.x;
			lhs.y *= rhs.y;
			return lhs;
		}

		public static FixVec2 operator /(FixVec2 lhs, Fix rhs)
		{
			return new FixVec2(lhs.x / rhs, lhs.y / rhs);
		}

		public static FixVec2 operator /(FixVec2 lhs, FixVec2 rhs)
		{
			return new FixVec2(lhs.x / rhs.x, lhs.y / rhs.y);
		}

		public static bool operator <=(FixVec2 a, FixVec2 b)
		{
			if (a.x <= b.x)
			{
				return a.y <= b.y;
			}
			return false;
		}

		public static bool operator >=(FixVec2 a, FixVec2 b)
		{
			if (a.x >= b.x)
			{
				return a.y >= b.y;
			}
			return false;
		}

		public static bool operator ==(FixVec2 lhs, FixVec2 rhs)
		{
			if (lhs.x.raw == rhs.x.raw)
			{
				return lhs.y.raw == rhs.y.raw;
			}
			return false;
		}

		public static bool operator !=(FixVec2 lhs, FixVec2 rhs)
		{
			if (lhs.x.raw == rhs.x.raw)
			{
				return lhs.y.raw != rhs.y.raw;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return x.raw + y.raw;
		}

		public bool Equals(FixVec2 other)
		{
			if (x.raw == other.x.raw)
			{
				return y.raw == other.y.raw;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is FixVec2)
			{
				return Equals((FixVec2)obj);
			}
			return false;
		}

		public void swapXY()
		{
			this = new FixVec2(y, x);
		}

		public FixVec2 get_rotated_90_degrees_counter_clockwise()
		{
			return new FixVec2(-y, x);
		}

		public FixVec2 get_rotated_90_degrees_clockwise()
		{
			return new FixVec2(y, -x);
		}

		public FixVec2 get_normalized_can_be_zero()
		{
			if (this == zero)
			{
				return zero;
			}
			return this / get_magnitude();
		}

		public Fix normalize_and_get_magnitude_can_be_zero()
		{
			Fix magnitude_can_be_zero = get_magnitude_can_be_zero();
			if (magnitude_can_be_zero == 0)
			{
				return 0;
			}
			x /= magnitude_can_be_zero;
			y /= magnitude_can_be_zero;
			return magnitude_can_be_zero;
		}

		public FixVec2 get_min(FixVec2 other)
		{
			return new FixVec2(math.min(x, other.x), math.min(y, other.y));
		}

		public FixVec2 get_max(FixVec2 other)
		{
			return new FixVec2(math.max(x, other.x), math.max(y, other.y));
		}

		public Fix dot(FixVec2 rhs)
		{
			return x * rhs.x + y * rhs.y;
		}

		public Fix Cross(FixVec2 rhs)
		{
			return x * rhs.y - y * rhs.x;
		}

		private FixVec2 ScalarAdd(Fix value)
		{
			return new FixVec2(x + value, y + value);
		}

		private FixVec2 ScalarMultiply(Fix value)
		{
			return new FixVec2(x * value, y * value);
		}

		public Fix get_magnitude()
		{
			ulong num = (ulong)((long)x.Raw * (long)x.Raw + (long)y.Raw * (long)y.Raw);
			return new Fix((int)(math.sqrt_ulong(num << 2) + 1) >> 1);
		}

		public Fix get_magnitude_can_be_zero()
		{
			if (x == 0 && y == 0)
			{
				return 0;
			}
			return get_magnitude();
		}

		public Fix get_distance(FixVec2 other)
		{
			return (other - this).get_magnitude_can_be_zero();
		}

		public Fix get_manhattan_distance(FixVec2 other)
		{
			FixVec2 fixVec = other - this;
			return math.abs(fixVec.x) + math.abs(fixVec.y);
		}

		public void normalize_can_be_zero()
		{
			if (!(x == 0) || !(y == 0))
			{
				Fix magnitude = get_magnitude();
				x /= magnitude;
				y /= magnitude;
			}
		}

		public void normalize()
		{
			Fix magnitude = get_magnitude();
			x /= magnitude;
			y /= magnitude;
		}

		public FixVec2 get_normalized()
		{
			FixVec2 result = this;
			result.normalize();
			return result;
		}

		public void linear_damping(FixVec2 dest, Fix speed)
		{
			FixVec2 fixVec = dest - this;
			Fix fix = fixVec.normalize_and_get_magnitude_can_be_zero();
			if (fix < speed)
			{
				this = dest;
			}
			else
			{
				this += fixVec * speed;
			}
		}

		public override string ToString()
		{
			return $"{x} {y}";
		}

		public static FixVec2 parse(string s)
		{
			string[] array = s.Split(' ');
			FixVec2 result = default(FixVec2);
			result.x = Fix.parse(array.try_get(0));
			result.y = Fix.parse(array.try_get(1));
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

		public void abs()
		{
			x = math.abs(x);
			y = math.abs(y);
		}

		public void rotate_radians(Fix angle)
		{
			Fix fix = math.sin_radians(angle);
			Fix fix2 = math.cos_radians(angle);
			Fix fix3 = fix2 * x - fix * y;
			y = fix * x + fix2 * y;
			x = fix3;
		}

		public FixVec2 get_rotated_radians(Fix angle)
		{
			FixVec2 result = this;
			result.rotate_radians(angle);
			return result;
		}

		public FixVec2 get_abs()
		{
			FixVec2 result = this;
			result.x = math.abs(result.x);
			result.y = math.abs(result.y);
			return result;
		}

		public void lerp(FixVec2 dest, Fix factor)
		{
			x = math.lerp(x, dest.x, factor);
			y = math.lerp(y, dest.y, factor);
		}

		public FixVec2 get_lerp(FixVec2 dest, Fix factor)
		{
			FixVec2 result = this;
			result.lerp(dest, factor);
			return result;
		}
	}
}
