using System;
using Microsoft.Xna.Framework;

namespace CommonLib
{
	public struct Vec2 : IYamlCustomSerialization
	{
		[Tag(1, null, true)]
		public float x;

		[Tag(2, null, true)]
		public float y;

		public static Vec2 zero = new Vec2(0f, 0f);

		public static Vec2 one = new Vec2(1f, 1f);

		public static Vec2 down = new Vec2(0f, -1f);

		public static Vec2 up = new Vec2(0f, 1f);

		public static Vec2 left = new Vec2(-1f, 0f);

		public static Vec2 right = new Vec2(1f, 0f);

		public static Vec2 invalid = new Vec2(float.NaN);

		public static Vec2 infinity = new Vec2(float.MaxValue);

		public bool IsZero
		{
			get
			{
				if (x == 0f)
				{
					return y == 0f;
				}
				return false;
			}
		}

		public bool IsNotZero
		{
			get
			{
				if (x == 0f)
				{
					return y != 0f;
				}
				return true;
			}
		}

		public Vec2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public Vec2(float xy)
		{
			x = xy;
			y = xy;
		}

		public Vec2(Vec3 v)
		{
			x = v.x;
			y = v.y;
		}

		public Vec2(Vector2 v)
		{
			x = v.X;
			y = v.Y;
		}

		public static explicit operator Vec2(Vector2 v)
		{
			return new Vec2(v.X, v.Y);
		}

		public static explicit operator Vec2(Vec3 v)
		{
			return new Vec2(v.x, v.y);
		}

		public static Vec2 new_direction(float angle)
		{
			return new Vec2(angle.cos(), angle.sin());
		}

		public float get_angle()
		{
			return math.atan2(x, y);
		}

		public float normalize_and_get_magnitude()
		{
			float num = (x * x + y * y).sqrt();
			x /= num;
			y /= num;
			return num;
		}

		public void linear_damping(Vec2 dest, float speed)
		{
			Vec2 vec = dest - this;
			float num = vec.normalize_and_get_magnitude_can_be_zero();
			if (num < speed)
			{
				this = dest;
			}
			else
			{
				this += vec * speed;
			}
		}

		public void damping(Vec2 dest, float dt, float factor)
		{
			Vec2 vec = dest - this;
			float num = vec.normalize_and_get_magnitude_can_be_zero();
			num = num * dt / (factor + dt);
			this += vec * num;
		}

		public float get_magnitude()
		{
			return (x * x + y * y).sqrt();
		}

		public float get_magnitude_squared()
		{
			return x * x + y * y;
		}

		public void normalize()
		{
			float num = (x * x + y * y).sqrt();
			x /= num;
			y /= num;
		}

		public void normalize_can_be_zero()
		{
			float num = (x * x + y * y).sqrt();
			if (num != 0f)
			{
				x /= num;
				y /= num;
			}
		}

		public void lerp(Vec2 dest, float t)
		{
			x = x.lerp(dest.x, t);
			y = y.lerp(dest.y, t);
		}

		public float normalize_and_get_magnitude_can_be_zero()
		{
			float num = (x * x + y * y).sqrt();
			if (num == 0f)
			{
				return 0f;
			}
			x /= num;
			y /= num;
			return num;
		}

		public void clamp_magnitude(float maxMagnitude)
		{
			float num = (x * x + y * y).sqrt();
			if (num != 0f && num > maxMagnitude)
			{
				x /= num;
				y /= num;
				x *= maxMagnitude;
				y *= maxMagnitude;
			}
		}

		public Vec2 get_clamp_magnitude(float maxMagnitude)
		{
			Vec2 result = this;
			result.clamp_magnitude(maxMagnitude);
			return result;
		}

		public Vec2 get_rotated_90_degrees_counter_clockwise()
		{
			return new Vec2(0f - y, x);
		}

		public Vec2 get_rotated_90_degrees_clockwise()
		{
			return new Vec2(y, 0f - x);
		}

		public void rotate(float angle)
		{
			float num = angle.sin();
			float num2 = angle.cos();
			float num3 = num2 * x - num * y;
			y = num * x + num2 * y;
			x = num3;
		}

		public Vec2 get_rotated(float angle)
		{
			float num = angle.sin();
			float num2 = angle.cos();
			return new Vec2(num2 * x - num * y, num * x + num2 * y);
		}

		public Vec2 get_normalized()
		{
			float num = (x * x + y * y).sqrt();
			return new Vec2(x / num, y / num);
		}

		public Vec2 get_normalized_can_be_zero()
		{
			float num = (x * x + y * y).sqrt();
			if (num == 0f)
			{
				return zero;
			}
			return new Vec2(x / num, y / num);
		}

		public Vec2 get_digital_direction()
		{
			if (Math.Abs(y) > Math.Abs(x))
			{
				return new Vec2(0f, Math.Sign(y));
			}
			return new Vec2(Math.Sign(x), 0f);
		}

		public float dot(Vec2 other)
		{
			return x * other.x + y * other.y;
		}

		public Vec2 get_floor()
		{
			return new Vec2(x.floor(), y.floor());
		}

		public Vec2 get_ceil()
		{
			return new Vec2(x.ceiling(), y.ceiling());
		}

		public float get_distance(Vec2 other)
		{
			float num = other.x - x;
			float num2 = other.y - y;
			return (num * num + num2 * num2).sqrt();
		}

		public float get_distance_squared(Vec2 other)
		{
			float num = other.x - x;
			float num2 = other.y - y;
			return num * num + num2 * num2;
		}

		public void set_min(Vec2 other)
		{
			x = Math.Min(x, other.x);
			y = Math.Min(y, other.y);
		}

		public void set_max(Vec2 other)
		{
			x = Math.Max(x, other.x);
			y = Math.Max(y, other.y);
		}

		public Vec2 get_min(Vec2 other)
		{
			return new Vec2(Math.Min(x, other.x), Math.Min(y, other.y));
		}

		public Vec2 get_max(Vec2 other)
		{
			return new Vec2(Math.Max(x, other.x), Math.Max(y, other.y));
		}

		public Vec2 get_projected(Matrix viewProj)
		{
			float num = viewProj.M14 * x + viewProj.M24 * y + viewProj.M44;
			return new Vec2((viewProj.M11 * x + viewProj.M21 * y + viewProj.M41) / num, (viewProj.M12 * x + viewProj.M22 * y + viewProj.M42) / num);
		}

		public static implicit operator Vec2(IntVec2 v)
		{
			return new Vec2(v.x, v.y);
		}

		public static implicit operator Vector2(Vec2 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static implicit operator Vector3(Vec2 v)
		{
			return new Vector3(v.x, v.y, 0f);
		}

		public static Vec2 operator *(Vec2 value, float scaleFactor)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			return value;
		}

		public static Vec2 operator *(float scaleFactor, Vec2 value)
		{
			value.x *= scaleFactor;
			value.y *= scaleFactor;
			return value;
		}

		public static bool operator <=(Vec2 a, Vec2 b)
		{
			if (a.x <= b.x)
			{
				return a.y <= b.y;
			}
			return false;
		}

		public static bool operator >=(Vec2 a, Vec2 b)
		{
			if (a.x >= b.x)
			{
				return a.y >= b.y;
			}
			return false;
		}

		public static Vec2 operator -(Vec2 value1, Vec2 value2)
		{
			value1.x -= value2.x;
			value1.y -= value2.y;
			return value1;
		}

		public static Vec2 operator -(Vec2 value1, float value2)
		{
			value1.x -= value2;
			value1.y -= value2;
			return value1;
		}

		public static Vec2 operator +(Vec2 value1, Vec2 value2)
		{
			value1.x += value2.x;
			value1.y += value2.y;
			return value1;
		}

		public static Vec2 operator +(Vec2 value1, float value2)
		{
			value1.x += value2;
			value1.y += value2;
			return value1;
		}

		public static Vec2 operator *(Vec2 value1, Vec2 value2)
		{
			value1.x *= value2.x;
			value1.y *= value2.y;
			return value1;
		}

		public static Vec2 operator /(Vec2 value1, Vec2 value2)
		{
			value1.x /= value2.x;
			value1.y /= value2.y;
			return value1;
		}

		public static Vec2 operator /(Vec2 value, float scaleFactor)
		{
			value.x /= scaleFactor;
			value.y /= scaleFactor;
			return value;
		}

		public static Vec2 operator -(Vec2 value)
		{
			value.x = 0f - value.x;
			value.y = 0f - value.y;
			return value;
		}

		public static bool operator ==(Vec2 value1, Vec2 value2)
		{
			if (value1.x == value2.x)
			{
				return value1.y == value2.y;
			}
			return false;
		}

		public static bool operator !=(Vec2 value1, Vec2 value2)
		{
			if (value1.x == value2.x)
			{
				return value1.y != value2.y;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Vec2))
			{
				return false;
			}
			return this == (Vec2)obj;
		}

		public override string ToString()
		{
			return yaml_serialize();
		}

		public override int GetHashCode()
		{
			return (int)(x + y);
		}

		public string yaml_serialize()
		{
			return utils.pack_to_string(x, y);
		}

		public void yaml_deserialize(string src)
		{
			utils.unpack_from_string(src, out x, out y);
		}
	}
}
